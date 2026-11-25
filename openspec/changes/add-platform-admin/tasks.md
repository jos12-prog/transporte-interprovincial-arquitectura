# Tareas

## 1. Dominio MI02 y MI03

- [x] 1.1 Extender `Company` y `Agency` con estado activo y los datos administrativos aprobados en `design.md`; validar reglas de dominio y cubrir cambios de estado con pruebas unitarias.
- [x] 1.2 Extender `VehicleModel` con estado activo y edición controlada del nombre/plano; hacer que el dominio rechace plano/capacidad inválidos y cubrir invariantes con pruebas unitarias.

## 2. Aplicación y contratos

- [x] 2.1 Definir contratos públicos en `Shared.Contracts` para las consultas y mutaciones administrativas de MI02 y MI03 y la consulta por lote de estado activo que necesita MI06; verificar que no introduzcan referencias de infraestructura.
- [x] 2.2 Implementar los casos de uso `UseCase` para CRUD lógico, activación/desactivación y validaciones de empresa/agencia/modelo, protegidos por rol `super_admin`; cubrir éxito, validación, inexistencia y autorización mediante pruebas unitarias.
- [x] 2.3 Implementar la regla de plano inmutable cuando existen vehículos vinculados, permitiendo renombrar/desactivar; cubrir plano congelado, modelo sin uso y conflicto con pruebas unitarias.

## 3. Infraestructura y persistencia

- [x] 3.1 Configurar migraciones aditivas de MI02 (`companies_agencies`) y MI03 (`transport_scheduling`) con sus respectivos `DesignTimeFactory`, conservando activos los registros existentes; verificar generación/aplicación de migraciones en PostgreSQL.
- [x] 3.2 Implementar repositorios EF Core y transacciones de MI02/MI03, incluyendo unicidad de RUC, ámbito empresa-agencia e invariantes concurrentes del plano; verificar con pruebas de integración Testcontainers.
- [x] 3.3 Conectar el contrato de MI06 para excluir viajes de empresas/agencias inactivas e invocar directamente el contrato existente de caché para invalidar después del commit de cada cambio de estado, sin Outbox; verificar búsqueda y caché antes/después de desactivar y reactivar.

## 4. Presentación API

- [x] 4.1 Exponer y documentar en Swagger los endpoints `/api/v1/admin/companies`, `/agencies` y `/vehicle-models`, con autorización `super_admin`, códigos HTTP acordados y `ProblemDetails`; verificar cada endpoint mediante pruebas de integración.
- [x] 4.2 Aplicar validación de payload, conflictos de unicidad, empresa/agencia inexistente y modelo/plano en uso; verificar respuestas 400/404/409 con contenido ProblemDetails en Integration y Jest API.

## 5. Frontend

- [x] 5.1 Implementar pantallas de empresas, agencias y modelos para superadministrador, con estados de carga/vacío/error, formularios y confirmación de desactivación según `design/stitch/superadmin/02-empresas/`, `03-agencias/`, `05-modelos-vehiculo/` y `DESIGN.md`; verificar build y pruebas Jest/RTL.
- [x] 5.2 Implementar el editor de plano numerado que impida cambiar geometría cuando el modelo está en uso y oculte modelos inactivos en selección para altas; verificar flujos válidos, errores 409 y estados de accesibilidad con Jest/RTL.

## 6. Pruebas de contrato y entrega

- [x] 6.1 Añadir pruebas Jest de API para operaciones de superadmin, acceso 401/403, errores 400/404/409 y exclusión pública de agencias/empresas inactivas; verificar contra la API real.
- [x] 6.2 Añadir requests y assertions para empresas, agencias, estados y modelos en `tests/postman/RutaAndina-Sprint2.postman_collection.json`; verificar colección JSON válida y ejecución de newman sin fallos.
- [ ] 6.3 Actualizar GitHub Actions si el pipeline no ejecuta las nuevas pruebas, migraciones de validación o reportes; verificar el workflow localmente o mediante CI.
- [ ] 6.4 Ejecutar `dotnet build`, `dotnet test`, `npm run build`, Jest frontend/API y `openspec validate --all --strict`; verificar que todas las ejecuciones terminan correctamente y que no hay reglas de arquitectura incumplidas.
