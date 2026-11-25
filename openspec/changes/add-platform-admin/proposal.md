# Propuesta: administración de plataforma

## Why

Hoy las empresas, agencias y modelos de vehículo dependen de datos sembrados, por lo que el superadministrador no puede incorporar operadores ni mantener el catálogo global desde la plataforma. Este cambio habilita la gestión central requerida por HU31, HU32 y HU34, y por RF37, RF38 y RF40.

## What Changes

- Añadir operaciones administrativas para registrar, consultar, editar y activar o desactivar empresas y sus agencias, restringidas al rol técnico `super_admin`.
- Añadir la gestión del catálogo global de modelos de vehículo y sus planos de asientos; un modelo usado conservará inmutable su plano y uno desactivado no podrá asignarse a vehículos nuevos.
- Exponer las operaciones bajo `/api/v1/admin/...`, con validación, respuestas `ProblemDetails` y conflictos HTTP 409 cuando corresponda.
- Incorporar pantallas del superadministrador basadas en las referencias de Stitch.
- Mantener la propiedad modular: MI02 Empresas y Agencias es dueño de empresas/agencias; MI03 Transporte y Programación es dueño de modelos de vehículo.

## Capabilities

### New Capabilities

- `platform-administration`: administración del catálogo de empresas, agencias y modelos de vehículo por el superadministrador.

### Modified Capabilities

- `trip-search`: excluir de los resultados públicos los viajes de empresas o agencias desactivadas.

## Impact

- Módulos MI02 y MI03, cada uno con sus capas y esquema PostgreSQL; API REST y autorización RBAC.
- UI de superadministración y su navegación protegida por rol.
- Pruebas unitarias, de integración, frontend y API; colección Postman.
- Trazabilidad: HU31, HU32, HU34; RF37, RF38, RF40; RC02–RC06, RC12 y RC15; EQ-03 y EQ-08; ADR-01, ADR-02, ADR-03, ADR-04, ADR-05, ADR-06 y ADR-09.

Fuera de alcance: gestión de rutas, vehículos, tarifas y viajes por administradores de agencia (RF30–RF33), asignación/gestión de personal (RF34, RF39 y RF41), auditoría y su infraestructura Outbox (RF43; change futuro `add-audit-trail`), y autenticación con Google. Las capacidades de catálogo de agencia se planifican en `add-agency-catalog-admin`; la gestión de personal, en `add-staff-management`.
