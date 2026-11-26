# Diseño: Administración del catálogo de agencia

## Contexto

Ver `proposal.md` para la motivación y las capacidades afectadas. MI03 ya posee `Route`, `Vehicle`, `Trip` y modelos; hoy `Route` no tiene empresa, `Vehicle` no tiene nombre visible ni estado y `Trip` no tiene estado. MI03 persiste en `transport_scheduling`; MI04 es propietario de `TripSeat` y `Reservation`. La búsqueda obtiene origen/destino de `Route` y el modelo mediante `Vehicle`, mientras `Trip.Price` ya es una copia persistida. El contrato de invalidación de búsquedas `IQueriesReportsCacheControl` y `AgencyScope` existen. La UI ya tiene `SeatLayoutView`, `StaffShell` y login por rol, pero el shell coloca actualmente el cierre de sesión en la barra superior.

El seeder actual comparte las mismas cinco rutas entre Andes Express y Wari Bus, aunque los viajes pertenecen a ambas empresas. La migración de MI03 será solo de esquema; el seeder de Development creará desde el inicio rutas y tarifas base propias para cada empresa. No hay datos reales y se recreará la base local.

### Decisión de datos de desarrollo regionales

El seed de Development representa una operación con origen en Ayacucho hacia sus provincias: Huanta, San Miguel (La Mar), Cangallo, Vilcas Huamán, Puquio (Lucanas), Coracora (Parinacochas), Huancapi (Víctor Fajardo) y Querobamba (Sucre), con rutas de ida y vuelta por empresa. La única ruta fuera de la región es Ayacucho → Lima. Las tarifas y duraciones son aproximadas, redondas y didácticas; el código del seeder las documenta. Andes Express y Wari Bus mantienen agencias independientes y cada viaje pertenece a la agencia de origen.

### Datos de catálogo pendientes de exposición

La API de MI03 aún no entrega paradas intermedias, distancia, duración estimada, ocupación del viaje ni nombre de una tarifa especial. La interfaz omite esos campos cuando no hay datos y no los infiere. Quedan pendientes para un change futuro que amplíe los contratos y endpoints correspondientes.

## Objetivos y exclusiones

**Objetivos:** cubrir los requisitos de las tres delta specs; mantener los límites entre MI03 y MI04; aplicar el ámbito desde el token y `X-Agency-Id`; dejar los artefactos necesarios para que implementación y pruebas puedan ejecutarse por etapas.

**No objetivos:** asignación de personal, reportes, ventas, pagos, pasajes, reprogramación, devoluciones, login Google, auditoría y Outbox.

## Decisiones

**Decisión del usuario (2026-11-26): acciones visibles por fila en lugar del menú .more de Stitch, por intuitividad.** Las acciones conservan confirmación para desactivar/cancelar y usan botones compactos con íconos; los badges muestran también un ícono de estado.

### Propiedad y ámbito de los datos

- `Route` y `Fare` llevan `CompanyId` y son compartidos por agencias de la empresa. Toda escritura deriva `CompanyId` del token; no acepta ese valor desde el cuerpo de la solicitud. Para estas operaciones también se exige una `X-Agency-Id` activa y asignada, como selección del contexto operativo, validada por la policy reutilizable `AgencyScope`.
- `Vehicle` y `Trip` llevan `CompanyId` y `AgencyId`; la agencia se toma del ámbito validado, y el caso de uso comprueba que las referencias de ruta/modelo pertenecen a la empresa y que el vehículo pertenece a la agencia activa.
- Reutilizar los contratos públicos de MI02 para confirmar empresa/agencia activa y no acceder a sus tablas desde MI03. El Admin API de MI03 conserva sus endpoints bajo `/api/v1/admin/...`, protegidos por `agency_admin`.
- Se mantiene la propiedad por empresa para rutas/tarifas acordada; duplicarlas por agencia sería inconsistente con que sean compartidas y produciría catálogos divergentes.

### Entidades MI03, persistencia y migración

- En `transport_scheduling`, añadir `CompanyId` y `IsActive` a `Route`; índice único `(CompanyId, OriginCity, DestinationCity)`. Añadir `DisplayName` e `IsActive` a `Vehicle`; conservar la placa normalizada única por empresa, como en la configuración existente. Añadir `Fare` con `CompanyId`, `RouteId`, `Type` (`Base`/`Special`), periodo inclusivo `ValidFrom`/`ValidTo`, `Price`, moneda PEN y estado activo. Añadir a `Trip` `Status` (`Scheduled`/`Cancelled`) y los datos de presentación del viaje como snapshots creados al programar (origen, destino, nombre visible del vehículo y nombre del modelo), junto con el precio ya copiado.
- Los periodos de cada tipo de tarifa en una ruta son inclusivos y no se pueden cruzar, incluso si una de las tarifas está desactivada. Serializar la validación y escritura por `RouteId` mediante bloqueo de fila para que dos solicitudes concurrentes no creen periodos solapados. Al resolver el viaje, usar la tarifa especial vigente para la fecha local de servicio de Lima; si no hay una especial, usar la tarifa base vigente.
- El chequeo y creación de viajes bloquea la fila del vehículo antes de buscar un solapamiento y guardar, serializando altas concurrentes. Los intervalos de servicio son semiabiertos `[DepartureAtUtc, ArrivalAtUtc)`: viajes contiguos pueden compartir el vehículo sin conflicto.
- La migración de MI03 será solo de esquema: columnas, tabla de tarifas, índices y restricciones. No copiará, duplicará, reasignará ni transformará rutas, viajes, tarifas o snapshots existentes. Development crea rutas deterministas por empresa y ruta y su tarifa base desde el inicio; Andes Express y Wari Bus tendrán sus propios registros compartidos por sus agencias. Los viajes semilla copian el precio de esa tarifa y guardan sus snapshots al crearse.
- No borrar físicamente rutas, vehículos, tarifas ni viajes. Los recursos inactivos no son elegibles para viajes nuevos; las referencias y snapshots de viajes existentes se conservan. Cambiar el modelo de un vehículo con viajes asociados devuelve `409`.
- Los viajes guardan snapshots de los datos públicos de ruta/vehículo/modelo y precio al programarse; las ediciones futuras del catálogo no reescriben viajes existentes. Esto conserva los datos históricos sin impedir que el catálogo se actualice. El borrado físico con cascada se descarta porque invalidaría reservas, mapas y referencias ya creadas.
- Se usa bloqueo pesimista de la fila de ruta/vehículo para serializar creación/edición de tarifas y programación del mismo vehículo; se prefiere a una restricción GiST con extensión porque reutiliza el patrón PostgreSQL ya adoptado y evita otra extensión de base.

### Contratos entre módulos y consistencia de asientos

- Mantener `TripSeat` y `Reservation` bajo propiedad de MI04. Extender `Shared.Contracts` con comandos públicos de MI04 para provisionar los asientos de un viaje desde su `SeatLayout`, comprobar elegibilidad/estado del viaje, y cancelar holds del viaje devolviendo si hay algún asiento `CONFIRMED`. MI03 no consulta ni escribe tablas de MI04.
- La composición del monolito coordina los comandos de MI03 y MI04 en una única transacción PostgreSQL local sobre conexión/transacción compartida, detrás de interfaces de aplicación y contratos públicos. Al programar: validar referencias y tarifa, guardar `Trip`, pedir a MI04 una fila por cada posición del plano y confirmar ambas escrituras juntas. Si la operación falla, no queda viaje ni grupo parcial de `TripSeat`.
- Al cancelar: bloquear el viaje en MI03, coordinar con MI04 la inspección de asientos y cancelación de reservas HELD, y confirmar juntos el estado `Cancelled`, las reservas `CANCELLED` y los asientos liberados. Si MI04 informa asientos confirmados, revertir toda la operación. La creación/liberación de holds toma el bloqueo/estado de viaje dentro del mismo límite transaccional para no admitir holds sobre un viaje ya cancelado.
- La cancelación es idempotente. Una vez cancelado el viaje, MI04 rechaza nuevas retenciones con `409`. Los endpoints de administración expresan conflictos como `409 ProblemDetails`.
- Se elige coordinación local porque ambos módulos viven en el mismo monolito y PostgreSQL; un flujo eventual/Saga u Outbox introduciría estados parciales y contradice la exigencia de atomicidad de la reserva.

### Búsqueda y caché

- Actualizar la lectura de MI03 para filtrar `Trip.Status == Scheduled` y devolver snapshots con nombre visible de vehículo y nombre de modelo. El endpoint de mapa incluye el nombre visible del vehículo desde el contrato de MI03; MI04 mantiene estados y geometría de asientos.
- Tras confirmar cada cambio que pueda modificar resultados públicos, MI03 invoca `IQueriesReportsCacheControl.InvalidateTripSearchAsync`. No invalidar si la transacción falla y no introducir Outbox para esta invalidación. Si Redis no está disponible, el TTL de búsqueda de hasta 60 segundos sigue siendo el límite de consistencia aprobado.
- Se reutiliza la invalidación directa existente, no una nueva clave de caché ni Outbox: la búsqueda es una proyección derivada y los datos operativos confirmados son la fuente de verdad.

### API y validación

- Exponer recursos administrativos para rutas, vehículos, tarifas de ruta y viajes, incluyendo consulta, alta, edición y activación/desactivación; la desactivación reemplaza al borrado físico. Programar un viaje responde `409` cuando no hay tarifa aplicable, con `ProblemDetails.detail` exactamente `La ruta no tiene tarifa vigente para esa fecha`, cuando el vehículo se solapa con otro viaje, o cuando una placa está duplicada. Validación de campos usa `400`; referencia fuera del ámbito usa `403`; recurso ausente usa `404`.
- Documentar en Swagger `200/201/204` según operación y los errores `400/403/404/409`. Los contratos de búsqueda y mapa añaden el nombre visible del vehículo sin exponer datos personales.

### Interfaz

- Crear pantallas bajo `design/stitch/admin-agencia/03-rutas/`, `04-vehiculos/`, `05-viajes-horarios/` y `06-tarifas/`, con `StaffShell`, rutas protegidas con `RequireRole` y estados de carga, vacío, validación, éxito y conflicto. El selector de vehículo ofrece solo modelos activos; la vista del vehículo integra `SeatLayoutView` y su nombre visible.
- Alinear `StaffShell` con `design/stitch/DESIGN.md`: tarjeta de usuario con nombre, rol y cierre de sesión al pie de sidebar; barra superior solo con título e insignia de rol. En login pasajero añadir el enlace de personal a `/personal/login`.

### Auditoría diferida

No se implementarán auditoría ni Outbox en este change. Esto deja una brecha conocida respecto de `AGENTS.md`, que exige auditar las operaciones críticas. El change futuro `add-audit-trail` cubrirá las operaciones críticas incorporadas por `add-platform-admin` y `add-agency-catalog-admin` mediante la infraestructura MI07/Outbox definida allí.

## Riesgos y compensaciones

- [La coordinación transaccional entre dos DbContext puede romper atomicidad si no comparten conexión/transacción] → Implementar el coordinador en infraestructura/Host, mantener la transacción local PostgreSQL explícita y probar rollback con Testcontainers; las aplicaciones solo ven contratos.
- [Una base local conserva la semilla antigua, cuyas rutas no tienen empresa] → Recrear el volumen local (`docker compose down -v`) antes de iniciar la versión con el nuevo esquema; la migración no transforma datos. El entorno del proyecto contiene solo datos semilla de desarrollo, sin datos reales.
- [Un administrador pierde el acceso si se desactiva su agencia durante una operación] → Validar estado y ámbito en el backend dentro del caso de uso; no confiar en la UI o el claim aislado.
- [Una búsqueda cacheada antigua reaparece por una lectura concurrente después de invalidar] → Invalidar después del commit y mantener TTL máximo de 60 segundos como límite temporal.
- [Las operaciones críticas quedan sin auditoría en esta entrega] → Documentar la brecha y cerrar el pendiente en `add-audit-trail`, que debe cubrir también las operaciones de `add-platform-admin`.

## Plan de migración

1. Recrear la base local y el volumen de desarrollo con `docker compose down -v`.
2. Aplicar la migración de esquema MI03 (columnas, tablas, índices y restricciones nuevas) y dejar intactos los registros de datos: no se duplican ni reasignan rutas.
3. Iniciar Development para que el seeder cree rutas y tarifas base independientes por empresa y los viajes/seats relacionados; verificar ambas empresas y sus rutas/tarifas con Testcontainers y revisar búsqueda/mapa.
4. Actualizar los contratos de MI02/MI04/MI06 en el mismo release del monolito. MI04 no recibe una nueva tabla propietaria de MI03.
5. Rollback: retirar endpoints/UI y volver al artefacto anterior. No revertir la migración de forma destructiva después de que el personal haya creado/actualizado catálogo; la reversión requiere backup y migración compensatoria revisada.

## Preguntas abiertas

Ninguna. Las reglas de tarifa, borrado lógico y postergación de auditoría fueron acordadas.
