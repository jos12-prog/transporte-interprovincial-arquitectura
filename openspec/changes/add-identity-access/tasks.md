# Tasks

## 1. Domain — MI01

- [x] 1.1 Reemplazar los placeholders de MI01 Domain con `User`, códigos de rol estables (`passenger`, `agent`, `agency_admin`, `super_admin`) y asignaciones de empresa/agencia como modelo puro; verificar invariantes y rechazar códigos desconocidos con xUnit y que Domain no referencie EF Core ni ASP.NET.

## 2. Application — MI01

- [x] 2.1 Definir contratos de repositorio/hasher/token/limitador y los casos `RegisterPassengerUseCase`, `LoginUseCase`, `GetCurrentUserUseCase` y `AgencyScope`; verificar que Application dependa solo de Domain/contratos y compile.
- [x] 2.2 Agregar pruebas xUnit para validación, duplicados como conflictos, login correo/DNI, códigos de claim por rol, expiración JWT de 60 min y decisiones de `AgencyScope` (permitida, otra agencia y otra empresa); ejecutar la suite Unit.

## 3. Infrastructure — MI01 y Host

- [x] 3.1 Configurar `IdentityAccessDbContext` con tablas `users`, `user_agency_assignments` y `login_attempt_windows`, índices únicos sobre DNI/correo normalizados y claves para asignaciones; crear la migración con `dotnet ef migrations add InitialIdentityAccess --project src/backend/Modules/IdentityAccess/Infrastructure/TransportPlatform.Modules.IdentityAccess.Infrastructure.csproj --startup-project src/backend/Host/TransportPlatform.Host.csproj --context IdentityAccessDbContext --output-dir Persistence/Migrations` y revisar que use el esquema `identity_access`.
- [x] 3.2 Implementar repositorio EF Core y hash con `IPasswordHasher<TUser>` o equivalente de ASP.NET Core; verificar por xUnit que el hash no sea la contraseña y que se compruebe correctamente.
- [x] 3.3 Implementar emisión/validación JWT con firma asimétrica y configuración externa (issuer/clave), claim `role` con códigos estables y rechazo de configuración ausente; verificar token válido, expiración, códigos y asignaciones claims por rol con xUnit.
- [x] 3.4 Implementar almacenamiento atómico de fallos por identificador HMAC, expiración de ventanas y reinicio en login correcto; implementar semilla Development idempotente con contraseñas solo desde configuración, y migrar `IdentityAccessDbContext` en `DevelopmentDatabaseMigrator` después de MI02; verificar reejecución y ausencia de secretos en logs con tests de integración (requiere Docker para Testcontainers).
- [x] 3.5 Crear `scripts/generate-dev-jwt-keys.ps1` y `.sh` para generar un par RSA solo en `.secrets/jwt/`; ignorar `.secrets/` en `.gitignore`, documentar issuer/rutas sin PEM en `.env.example`, y configurar `docker-compose.yml` para montar las claves en solo lectura y pasar rutas por entorno; verificar que ambos scripts generen un par válido y que no escriban claves fuera de `.secrets/`.

## 4. Presentation — API y autorización

- [x] 4.1 Registrar autenticación/autorización, middleware en orden, contratos `/api/v1/auth/register`, `/login` y `/me`, `ProblemDetails` y esquema Bearer/Authorize de Swagger; verificar códigos/documentación consultando `swagger.json` en pruebas de integración.
- [x] 4.2 Publicar `GET /api/v1/staff/agencies` y `GET /api/v1/staff/agencies/{agencyId}` con policy/handler reutilizable `AgencyScope` y lectura solo por `ICompaniesAgenciesApi`; agregar tests para 200, 401, 403, ausencia de datos cruzados y ejecutar Integration con PostgreSQL/Testcontainers (requiere Docker).
- [x] 4.3 Aplicar Rate Limiting nativo de ASP.NET Core (20 solicitudes/minuto/IP), registro de cinco fallos por identificador en cinco minutos, `Retry-After` y `ProblemDetails` 429; verificar 20 solicitudes permitidas, la 21.ª rechazada y el bloqueo por cuenta con tests sin esperas de ventana (requiere Docker para el almacenamiento PostgreSQL de intentos).

## 5. Frontend — React

- [x] 5.1 Copiar al frontend solo los recursos locales requeridos de marca/fotos y construir `AuthLayout`, `AuthCard` y `SubmitButton` idle/loading/done para login pasajero, registro y login personal conforme a DESIGN.md §9; no montar Google; verificar formularios, validación, errores, estados accesibles y `prefers-reduced-motion` con Jest.
- [x] 5.2 Implementar sesión en memoria, redirección y guardas por códigos técnicos de rol (etiquetas visibles en español) en `/agente`, `/admin-agencia` y `/superadmin`, selector conectado a `/staff/agencies`, menú `#menu-usuario` y logout del pasajero/personal; verificar acceso permitido/denegado para los cuatro códigos, cierre de sesión y menú con Jest.
- [x] 5.3 Actualizar `tailwind.config.ts` a neutros de marca, sombras en capas, curvas/duraciones de DESIGN.md §2/§4/§6 y alinear búsqueda/resultados/asientos; ajustar y ejecutar Jest frontend para regresiones visuales/comportamentales y validar `npm run build`.

## 6. Pruebas HTTP, colección y CI

- [x] 6.1 Completar Integration con Testcontainers para registro, correo/DNI duplicados, login pasajero/personal y códigos de rol, `/me`, 401/403 entre agencias, 20 solicitudes/min/IP, 5 fallos/5 min/identificador, migración y semilla; ejecutar la suite Integration completa (requiere Docker).
- [x] 6.2 Añadir pruebas Jest de `tests/api` para register/login/me, códigos de rol, agencias 200/401/403, 20 solicitudes/min/IP y 429 por cuenta con ProblemDetails; agregar esos requests a la colección Postman con variables de ejecución y tests por request; verificar Jest contra API real y Newman con la colección (requiere Docker).
- [ ] 6.3 Actualizar GitHub Actions para generar una clave RSA efímera con `openssl`, pasarla a Compose por montaje de solo lectura, ejecutar migraciones/backend/frontend/API tests y publicar artefactos; verificar que el workflow no suba PEM ni conserve claves y que valide correctamente.

## 7. Verificación final

- [ ] 7.1 Ejecutar `dotnet build`, `dotnet test`, `npm --prefix src/frontend run build`, `npm --prefix src/frontend test -- --coverage`, Jest de `tests/api` contra Compose, Newman, `openspec validate --all --strict` y comprobar que búsqueda, mapa y holds siguen accesibles sin token; registrar resultados y dejar pendientes solo las verificaciones que requieran Docker ausente.
