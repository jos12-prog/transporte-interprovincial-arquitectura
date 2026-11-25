# Proposal

## Why

MI01 ya existe como esqueleto, pero la plataforma todavía no puede identificar a sus cuatro roles ni aplicar en backend el aislamiento de empresa y agencia descrito por ADR-06 y EQ-03. Sin registro, login y una sesión validada por el servidor, las rutas de personal y el acceso asignado a agencias no pueden pasar de pantallas de referencia a flujos utilizables.

## What Changes

- Crear la capacidad de identidad y acceso de MI01 para registrar pasajeros, autenticar pasajeros y personal, emitir JWT de 60 minutos, consultar `/auth/me` y proteger la información con roles y ámbito organizacional.
- Implementar `AgencyScope` reutilizable y las consultas de RF29 `GET /api/v1/staff/agencies` y `GET /api/v1/staff/agencies/{agencyId}`; solo exponen agencias autorizadas por `agency_ids[]` del token.
- Usar códigos técnicos estables en el claim `role` (`passenger`, `agent`, `agency_admin`, `super_admin`); los nombres de rol en español se reservan para etiquetas de UI y documentación.
- Aplicar ambos límites de login: veinte solicitudes por minuto por IP para tolerar IP compartidas y bloqueo por cinco fallos por identificador dentro de cinco minutos como control principal, con HTTP 429 y `ProblemDetails`.
- Añadir generación local de claves RSA con scripts PowerShell/Bash en `.secrets/` ignorado por Git, montaje de solo lectura en Compose y generación efímera con OpenSSL en CI; ninguna clave se guarda en el repositorio.
- Agregar cuentas de demostración de Development idempotentes y vinculadas a las empresas/agencias sembradas; las contraseñas llegan por configuración/variables de entorno y se guardan con hash.
- Añadir las pantallas y sesión en memoria para login/registro, protección de rutas por rol, selector de agencia, menú de cuenta del pasajero y cierre de sesión del personal. Ocultar Google, que pertenece a un change posterior.
- Actualizar los tokens Tailwind para los neutros de marca, sombras en capas, curvas y duraciones de DESIGN.md §2, §4 y §6; conservar búsqueda, mapa y holds públicos. Documentar que asociar el hold al pasajero autenticado queda para un change posterior.
- Añadir controles Bearer a Swagger, pruebas .NET, Jest frontend y Jest HTTP; integrar requests de identidad en Postman y actualizar CI si las nuevas verificaciones lo requieren.

## Capabilities

### New Capabilities

- `identity-access`: registro y autenticación de los cuatro roles, JWT, perfil autenticado, ámbito de agencias, endpoints de RF29, limitación de login y experiencia de acceso.

### Modified Capabilities

Ninguna. `domain-baseline` ya establece RBAC, claims y aislamiento entre tenants; esta capacidad especifica el flujo de identidad y sus contratos HTTP sin cambiar esas reglas existentes.

## Impact

- Módulo MI01 (`IdentityAccess`) y composición del host .NET 8; esquema `identity_access`, migración EF Core y configuración local de cuentas de Development. La clave de firma y contraseñas no se guardan en el repositorio.
- APIs nuevas: `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, `GET /api/v1/auth/me`, `GET /api/v1/staff/agencies` y `GET /api/v1/staff/agencies/{agencyId}`. Los endpoints de búsqueda, mapa y holds continúan públicos.
- Frontend React: AuthLayout, AuthCard, SubmitButton, login de pasajero/personal, registro, guardas de rol, selector de agencia y acciones de sesión.
- CI y pruebas en `tests/Unit`, `tests/Integration`, `tests/api` y `src/frontend`.
- Trazabilidad: HU22, HU27, HU33, HU35 y HU39; RF29, RF34 y RF39; RC01–RC05 y RC15; DA04, EQ-03 y ADR-06; Táctica 7 Rate Limiting (AC04/AC03). RF34 y RF39 se cubren solo en cuanto a identidades, roles y asignaciones de Development; CRUD de agentes y asignación administrativa siguen fuera del alcance. Los documentos no definen una HU/RF específica para registro/login de pasajero; ese flujo se origina en este alcance.
- Fuera de alcance: Google, CRUD general de empresas/agencias/vehículos/usuarios, recuperación de contraseña, refresh tokens y asociación de holds al pasajero.
