# Propuesta: Administración del catálogo de agencia

## Por qué

Hoy los viajes dependen principalmente de datos semilla, por lo que el administrador de agencia no puede mantener desde la plataforma las rutas, tarifas, vehículos y horarios de su operación. Este change habilita la administración por empresa y agencia autorizada, conservando la coherencia de precios, asientos, búsqueda y reservas existentes.

## Cambios

- Incorporar en MI03 la gestión de rutas y tarifas compartidas por empresa, vehículos pertenecientes a una agencia y viajes programados pertenecientes a una agencia.
- Resolver el precio de cada viaje al crearlo: tarifa especial para la fecha cuando exista; en su defecto, tarifa base vigente. Guardar una copia del precio en `Trip`.
- Rechazar con `409 ProblemDetails` la ausencia de tarifa vigente, la intersección de periodos de tarifas del mismo tipo en una ruta, placas duplicadas, solapamiento de horarios del mismo vehículo y cambios de modelo de un vehículo que ya tenga viajes.
- Permitir desactivar, sin borrado físico, rutas, vehículos y tarifas; los viajes existentes conservan sus datos y continúan funcionando, mientras que los registros desactivados no se pueden usar para crear viajes nuevos.
- Permitir cancelar viajes sin borrado físico: ocultarlos de la búsqueda, liberar sus holds activos de manera atómica y responder `409 ProblemDetails` cuando haya asientos vendidos/confirmados.
- Invalidar la caché de búsqueda mediante el contrato existente después del commit de cada mutación que cambie resultados públicos.
- Implementar las pantallas del administrador de agencia conforme a `design/stitch/admin-agencia/` y `design/stitch/DESIGN.md`; usar `SeatLayoutView` para el plano de vehículos. Ajustar `StaffShell` y enlazar el login pasajero con `/personal/login` según el diseño aprobado.
- Crear `docs/criterios-evaluacion.md` con los 16 criterios de la rúbrica y las evidencias existentes; los pendientes se distinguen explícitamente.

## Capacidades

### Capacidades nuevas

- `agency-catalog-administration`: catálogo operativo de rutas, tarifas, vehículos y viajes con ámbito de empresa/agencia, cancelación segura y UI administrativa.

### Capacidades modificadas

- `trip-search`: los viajes cancelados no se ofrecen en la búsqueda pública; sus cambios de catálogo invalidan la caché luego de confirmar la transacción.
- `seat-hold-reservation`: la cancelación de un viaje libera de forma atómica sus holds HELD y sus asientos; no puede cancelar el viaje si existen asientos confirmados.

## Impacto

- Módulo principal: MI03 Transporte y Programación; integración mediante contratos públicos de Shared con MI02, MI04 y MI06. Se reutiliza AgencyScope de MI01 y el contrato de invalidación de caché existente de MI06.
- Frontend: rutas privadas de `agency_admin`, pantallas del administrador de agencia, `StaffShell`, login de pasajero y componente compartido `SeatLayoutView`.
- Persistencia: esquema y migración de MI03; ampliación de MI04 únicamente mediante su contrato público para cancelación de holds y verificación transaccional de asientos confirmados. No consultar tablas de otros módulos.
- Trazabilidad: HU23–HU26; RF30–RF33; RC02–RC07; EQ-02 y EQ-03; ADR-01, ADR-02, ADR-04–ADR-09.
- Fuera de alcance: gestión de agentes y asignaciones (RF34, RF39, RF41), reportes, pagos y emisión de pasajes, reprogramación/devolución (RF15–RF17, RF28), Google login y auditoría/Outbox (RF43), que se atenderá en `add-audit-trail`.
