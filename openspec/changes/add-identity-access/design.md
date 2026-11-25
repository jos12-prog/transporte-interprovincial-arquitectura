# Design

## Context

Ver `proposal.md` para la motivación y `specs/identity-access/spec.md` para los contratos observables. MI01 ya tiene proyectos de cuatro capas y `IdentityAccessDbContext` con esquema `identity_access`, pero no tiene entidades, servicios de aplicación ni migraciones y su registro solo configura EF Core. El migrador de Development actual no incluye ese contexto.

MI02 expone `ICompaniesAgenciesApi.GetPublicAgenciesAsync` y el host ya siembra empresas y agencias con IDs deterministas antes de los viajes y asientos. Se debe consultar MI02 mediante ese contrato, sin leer sus tablas desde MI01. `LayeringTests` de NetArchTest ya incluye los assemblies de IdentityAccess.

El frontend tiene rutas placeholder para `/agente`, `/admin-agencia` y `/superadmin`; no hay proveedor de sesión ni pantallas React de autenticación. Las pantallas de referencia están en `design/stitch/pasajero/01-login`, `pasajero/02-registro`, `personal/01-login` y `admin-agencia/01-selector-agencia`. `DESIGN.md` §9 muestra Google, pero el alcance de este change lo excluye. Búsqueda, mapa y holds hoy son públicos y así permanecen.

## Goals / Non-Goals

**Goals:**

- Completar autenticación en MI01 sin que Domain dependa de ASP.NET Core, EF Core ni Identity.
- Emitir y validar claims compatibles con ADR-06 y mantener la consulta de agencias en límites públicos de módulos.
- Ofrecer rutas de acceso accesibles y estados de interfaz consistentes con el sistema visual y con `prefers-reduced-motion`.

**Non-Goals:**

- Integrar Google, recuperación de contraseña, refresh tokens, CRUD general de usuarios o cambio de credenciales.
- Requerir login para búsqueda, mapa de asientos o holds, ni vincular la reserva temporal al pasajero en este change.
- Implementar las operaciones de negocio de agentes, administradores o superadministradores; sus pantallas permanecen shells salvo el selector de agencias.

## Decisions

### Persistencia modular de MI01

Crear tablas propias bajo `identity_access`:

- `users`: `id`, nombre, DNI y correo normalizados, celular, hash de contraseña y rol fijo.
- `user_agency_assignments`: usuario, `company_id` y `agency_id`; clave única por usuario/agencia. Los IDs de MI02 son referencias lógicas, sin FK a otro esquema.
- `login_attempt_windows`: partición hash del identificador normalizado, contador y ventana/bloqueo UTC para fallos por cuenta. No almacena contraseña ni identificador en claro.

Unique indexes sobre DNI y correo normalizados previenen duplicados concurrentes. El alta crea la cuenta completa en una operación EF Core. Los valores para el registro son: nombre requerido, DNI de ocho dígitos, correo válido, celular requerido y contraseña de al menos ocho caracteres, coincidiendo con `pasajero/02-registro`.

La aplicación define contratos/repositorios y casos de uso. Infraestructura implementa EF Core, hashing, emisión de token y contadores. Presentation traduce validación, duplicados y autorización a `ProblemDetails`. No se usa ASP.NET Identity `UserManager` como modelo de dominio; se usa `IPasswordHasher<TUser>` (o el hasher equivalente de ASP.NET Core) detrás de una implementación de Infraestructura.

### Credenciales, JWT y roles

El registro crea solo el rol `Pasajero`; los roles de personal se asignan por semilla Development y, fuera de Development, por administración futura. El login recibe un único `identifier` y contraseña: para pasajeros acepta correo o DNI; personal usa correo corporativo. Las respuestas de credenciales incorrectas son uniformes para no revelar la existencia de la cuenta.

El token dura 60 minutos y usa `sub` para `userId`, `role` para códigos técnicos estables (`passenger`, `agent`, `agency_admin`, `super_admin`), y `company_id` + arreglo `agency_ids` para los roles con ámbito de tenant. Superadministrador (`super_admin`) es global y no recibe asignaciones tenant. Los nombres `Pasajero`, `Agente/Vendedor`, `Administrador de Agencia` y `Superadministrador` son etiquetas de presentación/documentación, nunca valores del claim. La configuración debe mapear claims sin transformaciones implícitas inesperadas (`MapInboundClaims = false`).

ADR-06 y Táctica 6 describen firma asimétrica: se usará clave privada de firma y emisor de configuración/variables de entorno; solo la clave pública valida tokens. El host falla al arrancar si la clave de firma o emisor faltan, sin clave de desarrollo embebida. Las variables se documentan con placeholders, nunca con valores reales. No se emiten refresh tokens.

### Scope reutilizable y consultas de agencia

Crear `AgencyScope` como política/handler de autorización reutilizable en Presentation, con la comprobación de pertenencia aislada en una regla de Application que no depende de ASP.NET. Solo los códigos `agent` y `agency_admin` tienen acceso a estas rutas. Para el detalle de RF29, el ID objetivo procede de `{agencyId}` en la ruta: se acepta solo si aparece en `agency_ids` y el resumen de MI02 confirma el mismo `company_id`. No se lee directamente `CompaniesAgenciesDbContext`.

`GET /api/v1/staff/agencies` proyecta únicamente IDs del token y obtiene nombre/ciudad por `ICompaniesAgenciesApi`; `GET /api/v1/staff/agencies/{agencyId}` autoriza la ruta antes de consultar el resumen. Los dos endpoints se limitan a Agente/Vendedor y Administrador de Agencia. Un ID fuera del ámbito recibe 403 sin revelar si corresponde a una agencia existente. Este identificador de recurso en la ruta no sustituye el contexto activo de futuras operaciones; esas operaciones continúan validando `X-Agency-Id` conforme a ADR-06 y AGENTS.md.

### Rate limiting doble

Se aplican ambos controles trazados a Táctica 7: middleware de ASP.NET Core Rate Limiting con ventana fija de veinte solicitudes por minuto por IP, y contador de cinco fallos por identificador normalizado en cinco minutos. La cuota por IP es más alta para evitar bloquear a pasajeros y personal que comparten cabinas, agencias o redes universitarias; el bloqueo por identificador se mantiene como control principal. El primer control responde 429 al exceder la cuota; el segundo registra fallos de forma atómica en PostgreSQL y bloquea los siguientes intentos hasta concluir el periodo. Un login correcto reinicia la ventana de fallos. La partición de identificador se almacena como HMAC-SHA-256 usando una clave de configuración separada para no persistir correo/DNI en claro.

Las respuestas 429 usan `ProblemDetails` y `Retry-After`. En un login rechazado se responde 401 hasta alcanzar el límite de fallos; las peticiones durante el bloqueo responden 429. El límite por IP del middleware es por proceso ASP.NET; el despliegue con varias instancias debe complementarlo en el gateway compartido para que no se multiplique la cuota por réplica. El bloqueo por identificador sí se comparte porque está en PostgreSQL.

### Cuentas Development y migración

El host aplica la migración `IdentityAccessDbContext` y luego llama al seeder después de sembrar MI02. El superadministrador es global; el administrador y agente se vinculan a IDs deterministas de los datos ya sembrados y se persisten en `user_agency_assignments`. Para que la pantalla de referencia represente selección múltiple, el usuario de prueba Administrador de Agencia queda asociado a tres agencias de la misma empresa y el Agente a una. La asignación concreta es fixture de Development, no política de negocio.

El seeder exige credenciales configuradas para los tres usuarios, aplica hash y es idempotente. No instala contraseñas por defecto ni las escribe en logs. El `DevelopmentDatabaseMigrator` debe migrar `identity_access`; producción no ejecuta el seeder. Las asignaciones de agencia usan `ICompaniesAgenciesApi`/IDs de seed, nunca consultas a tablas de MI02 desde MI01.

Las claves de desarrollo se generan bajo `.secrets/jwt/` mediante `scripts/generate-dev-jwt-keys.ps1` o `scripts/generate-dev-jwt-keys.sh`; `.secrets/` queda ignorado por Git. Compose monta la carpeta en `/run/jwt-keys:ro` y pasa `JWT_SIGNING_PRIVATE_KEY_PATH=/run/jwt-keys/private.pem` y `JWT_SIGNING_PUBLIC_KEY_PATH=/run/jwt-keys/public.pem` por variables de entorno. `.env.example` documenta solo emisor/rutas y el comando de generación, nunca material PEM. CI genera una clave temporal con `openssl`, la monta en solo lectura para el job y la descarta al finalizar.

Comando previsto para crear la migración cuando el código esté listo:

```powershell
dotnet ef migrations add InitialIdentityAccess --project src/backend/Modules/IdentityAccess/Infrastructure/TransportPlatform.Modules.IdentityAccess.Infrastructure.csproj --startup-project src/backend/Host/TransportPlatform.Host.csproj --context IdentityAccessDbContext --output-dir Persistence/Migrations
```

### Integración OpenAPI y errores

Configurar autenticación Bearer y autorización en el host, aplicar `UseAuthentication` antes de `UseAuthorization`, y registrar el esquema HTTP Bearer de OpenAPI para que aparezca **Authorize** en Swagger. Documentar los códigos de registro/login, `/me` y agencias, incluyendo `400`, `401`, `403`, `409` y `429` donde correspondan. Las excepciones de validación/conflicto se convierten en `ProblemDetails`; mensajes 401/403/429 no contienen hashes, datos de otras cuentas ni detalles internos.

La clase `TripSearchController` usa `/api/v1/trips/search`, por lo que las rutas de auth se fijan bajo `/api/v1/auth`; se reemplaza el placeholder `POST /api/auth/login` de DESIGN.md §9 por el contrato versionado de esta spec.

### Sesión y pantallas React

Implementar `AuthLayout`, `AuthCard` y `SubmitButton` en idle/loading/done. Rutas propuestas: `/pasajero/login`, `/pasajero/registro` y `/personal/login`. El registro termina en login; el login crea sesión solo en memoria y resuelve guardas/redirección usando los códigos del claim (`passenger`, `agent`, `agency_admin`, `super_admin`), mientras muestra sus etiquetas en español. La ruta base `/admin-agencia` presenta el selector de `admin-agencia/01-selector-agencia`, alimentado por `/api/v1/staff/agencies`. El shell de personal ofrece `Cerrar sesión` y el avatar del pasajero abre el menú de `pasajero/03-inicio-busqueda` (`#menu-usuario`), con cierre por Escape y click fuera.

No se monta ni se reserva espacio para `GoogleButton` en login o registro. Las llamadas usan `Authorization: Bearer` para `/me` y agencias; token y estado de sesión no se guardan en `localStorage`, `sessionStorage` ni cookies. Refrescar la página termina la sesión. Los endpoints públicos de viajes/holds siguen funcionando sin token.

Copiar al frontend los recursos de marca y las fotos locales referenciadas por las maquetas, porque el contexto de Docker del frontend no puede leer `design/stitch/`. No modificar esas fuentes de referencia. Actualizar `tailwind.config.ts` con los tokens del nuevo `DESIGN.md`: neutros verdosos (`#111B19`, `#4A5754`, `#62706C`, `#E0E6E4`, `#F6F8F7`), sombras de contacto + difusa y curvas `cubic-bezier(.23,1,.32,1)`, `cubic-bezier(.77,0,.175,1)` y `cubic-bezier(.32,.72,0,1)`, duraciones base 150/160/200 ms.

El movimiento sigue `design/stitch/DESIGN.md` §6 y los criterios de `.agents/skills/emil-design-eng` y `animate`: nada animado por rutina si no aporta feedback; sin `transition: all` ni `ease-in`; hover solo con puntero fino; entradas de una sola vez usando opacidad/transform y `ease-out`; estados rápidos con transición interrumpible; `prefers-reduced-motion` quita desplazamientos, escalas, blur, parallax, zoom y sacudidas, y conserva fundidos/cambios de estado. El menú anclado al avatar escala desde su trigger en 150 ms; los estados loading/done se cruzan en la misma celda, con `aria-busy` y `aria-live`.

La retención actual sigue identificada por `holdToken` anónimo. Un change futuro deberá asociar `Reservation` al pasajero autenticado al crear hold, definir qué pasa al vencer/cerrar sesión y migrar el contrato; este change no altera `POST/DELETE /api/v1/reservations/holds`.

### Verificación

- xUnit: hasher, emisión/validación de JWT, claims, roles, `AgencyScope`, ventanas de fallo y límite de tamaño.
- Integration con PostgreSQL/Testcontainers: registro, normalización y duplicados; login de pasajero/personal; `/me` sin token/con token; lista y detalle autorizado; 403 entre agencias/empresas; 429 por IP y por cuenta; seed/migración idempotentes.
- Jest frontend: formularios/errores, estados del botón, rutas por rol, sesión en memoria, selector, menú y logout; `prefers-reduced-motion` elimina las transformaciones de movimiento.
- Jest `tests/api`: contrato real de register/login/me, 401/403/429 y agencies; Postman incorpora las llamadas y sus comprobaciones.
- NetArchTest preserva límites de MI01 y dependencia a contratos públicos. CI ejecuta build, suites y validación OpenSpec.

## Risks / Trade-offs

- [La cuota por IP es local a cada proceso] → El middleware nativo cumple el límite en una instancia; registrar el mismo límite en el gateway compartido antes de escalar horizontalmente.
- [Las asignaciones cambian mientras un JWT sigue vigente] → Token máximo de 60 minutos y validación de la ruta contra sus claims; cambios de asignación efectivos al emitir token nuevo. La revocación inmediata requeriría una lista de revocación o tokens más cortos fuera del alcance.
- [Fallo de configuración puede impedir emitir/validar credenciales] → Fallar al arrancar de forma explícita si falta issuer/clave privada o secreto HMAC; documentar nombres y placeholders y verificarlo en integración.
- [Fotos de referencia pueden aumentar peso del bundle] → Copiar solo los recursos necesarios, optimizar sin cambiar su encuadre y validar tamaño del build.
- [Sesión en memoria se pierde al refrescar] → Es el comportamiento solicitado y evita persistir bearer tokens en almacenamiento accesible por JavaScript; no ofrecer “recordarme” como persistencia real en este change.

## Migration Plan

1. Crear entidades/índices MI01 y la migración `InitialIdentityAccess`; revisar `Up` antes de aplicarla.
2. Registrar JwtBearer, autorización, endpoints y seeder; en Development migrar identidad después de MI02 y sembrar cuentas configuradas.
3. Desplegar backend y frontend juntos; verificar alta/login, agency scope, 429 y Swagger antes de habilitar el acceso a personal.
4. Rollback de aplicación: volver a la imagen anterior y retirar configuración de autenticación del despliegue. No borrar tablas ni cuentas automáticamente; si la migración ya contiene cuentas reales, conservar el esquema y planificar una reversión de datos con backup.

## Open Questions

No quedan decisiones funcionales abiertas. La política de límite local por instancia y las respuestas de RF29 fueron definidas en este diseño/especificación.
