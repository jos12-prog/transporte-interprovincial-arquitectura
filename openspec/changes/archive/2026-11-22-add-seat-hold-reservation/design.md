# Design

## Context

Ver `proposal.md` y `specs/seat-hold-reservation/spec.md` para el alcance y contrato observable. MI04 ya posee `TripSeat`, `TripSeatStatus` (`Available`, `Held`, `Confirmed`), `ReservationsDbContext` y la consulta de conteos libres. El contexto solo mapea actualmente `trip_seats`. MI03 persiste `VehicleModel.SeatLayout` como JSONB, pero su contrato público `ITransportSchedulingApi` solo expone la búsqueda. MI04 es el propietario de los estados transaccionales; MI03 es el propietario del plano. No hay autenticación/JWT todavía.

## Goals / Non-Goals

**Goals:**

- Mantener el dominio y la base transaccional dentro de MI04, leyendo el plano de MI03 únicamente por un contrato público neutral.
- Crear un único límite transaccional que bloquee los asientos solicitados, cree la reserva y actualice su estado de forma atómica.
- Hacer verificables los estados y la expiración sin esperas reales de diez minutos.
- Conservar el mapa como lectura vigente y permitir que la búsqueda acepte el TTL informativo documentado.

**Non-Goals:**

- Asociar datos personales o una identidad permanente al hold; el `holdToken` solo identifica a su titular temporal.
- Ejecutar transiciones de pago, confirmar o reprogramar reservas, emitir pasajes, notificar por WhatsApp o vender por ventanilla.
- Invalidar claves de búsqueda entre módulos al cambiar el estado de un asiento.

## Decisions

### Propiedad y contratos entre módulos

- MI04 implementa los casos de uso, `Reservation`, la asociación de asientos, repositorios, endpoints y worker. MI04 no consulta tablas de MI03.
- Extender `ITransportSchedulingApi` de `Shared.Contracts` con una consulta por `tripId` que devuelva un DTO neutral del nombre/capacidad del modelo y las posiciones `SeatLayout`. MI03 implementa el contrato leyendo su propio esquema. La respuesta nula identifica un viaje inexistente.
- `GET /api/v1/trips/{tripId}/seats` se resuelve en Presentation de MI04: combina el plano obtenido de MI03 con los `TripSeat` de MI04 y traduce `Confirmed` a `Occupied`. El mapa consulta la fuente transaccional cada vez y no reutiliza la caché Redis de búsqueda.
- Application depende de sus entidades de Domain y de contratos; Infrastructure implementa `IReservationRepository` y persistencia; Presentation valida la petición y delega al caso de uso. No se agregan FK entre esquemas.

### Modelo y estados de reserva

- Añadir `Reservation` en `reservations.reservations` con `Id`, `CompanyId`, `AgencyId`, `TripId`, `Status`, hash del token, `CreatedAtUtc` y `ExpiresAtUtc`. Como el change excluye identidad y datos de pasajeros, no se persiste PII.
- Añadir `ReservationSeat` en `reservations.reservation_seats` como asociación entre una reserva y un `TripSeat`, guardando el estado activo necesario para la salvaguarda única. Mantener el estado de la asociación, el de `Reservation` y el de `TripSeat` sincronizados dentro de la misma transacción.
- La máquina enumera `HELD`, `EXPIRED`, `CANCELLED`, `CONFIRMED` y `RESCHEDULED`; este change solo permite transiciones `HELD → EXPIRED` y `HELD → CANCELLED`. `CONFIRMED` y `RESCHEDULED` quedan definidos para cambios posteriores, sin endpoints ni transición ejecutable aquí.
- Añadir índice único parcial por `trip_seat_id` en `reservation_seats` para filas con estado `HELD` o `CONFIRMED`, según RC09. La fila bloqueada de `TripSeat` sigue siendo la fuente de disponibilidad; el índice protege además frente a carreras o rutas de escritura futuras.

### Crear y liberar una retención

- `POST /api/v1/reservations/holds` acepta `{ tripId, tripSeatIds }` de 1 a 10 asientos. Rechaza con HTTP 400 `ProblemDetails` una colección vacía, duplicados, más de 10 IDs o una petición mal formada, antes de iniciar la transacción; las referencias inexistentes o ajenas al viaje responden 404. Ordena los IDs antes de bloquear para reducir deadlocks y abre una transacción corta.
- La transacción obtiene las filas `TripSeat` solicitadas con `SELECT ... FOR UPDATE`, confirma que todas siguen `Available`, crea una `Reservation` con expiración a `now + 10 min`, crea sus asociaciones y cambia todas las filas a `Held`. Si un asiento no está libre devuelve conflicto y revierte el grupo completo. Una violación del índice parcial se traduce a HTTP 409 con `ProblemDetails`; no se filtran errores de PostgreSQL.
- Generar `holdToken` criptográficamente aleatorio (256 bits); devolver el valor original una sola vez y almacenar únicamente su hash. `DELETE /api/v1/reservations/holds/{id}` recibe el secreto en `X-Hold-Token`, lo compara en tiempo constante y no lo escribe en logs ni URL. Un hold vigente del titular se marca `CANCELLED` y libera todos los asientos dentro de una sola transacción; después del commit responde HTTP 204 No Content, sin cuerpo. Un token ausente o incorrecto se responde como 404 para no revelar la existencia de un hold ajeno; una reserva que ya no está `HELD` devuelve 409.
- Swagger documenta 200 para mapa/creación, 204 sin contenido para liberación correcta, 400 para validación, 404 para viaje/hold no encontrado y 409 para asiento en conflicto o estado no liberable. Los errores de API usan `ProblemDetails`.

### Expiración distribuida

- Registrar un `BackgroundService` en el Host; cada diez segundos crea un scope, abre una transacción y selecciona reservas `HELD` vencidas mediante `FOR UPDATE SKIP LOCKED`. Cada lote marca la reserva y sus asociaciones como `EXPIRED` y devuelve sus `TripSeat` a `Available` antes del commit. Instancias concurrentes pueden repartir filas sin procesarlas dos veces.
- Usar `TimeProvider` para leer el tiempo de creación/vencimiento y hacer determinista el ciclo del servicio con un test double de reloj, sin depender de esperas reales ni añadir un paquete de reloj. La expiración debe ser idempotente; el siguiente polling ocurre como máximo una cadencia después de vencer.
- No se emite Outbox: este alcance no confirma pagos ni crea eventos de notificación. La confirmación queda para el flujo posterior.

### Coherencia con búsqueda y diseño de interfaz

- No invalidar la caché de MI06: `trip-search` permite que su conteo informativo se desactualice hasta el TTL máximo ya aceptado de 60 s. El mapa de asientos y la decisión de reservar siempre leen MI04/PostgreSQL. Al confirmar o vencer un hold, la búsqueda podrá mostrar temporalmente un conteo anterior, pero no permite adquirir el asiento sin la validación transaccional.
- Añadir la ruta de pasajero `/pasajero/viajes/:tripId/asientos`, enlace desde los resultados y componentes React reutilizables. Dibujar las posiciones del DTO `SeatLayout`, no copiar `code.html`; mapear estados a la leyenda aprobada en `design/stitch/DESIGN.md` y `design/stitch/pasajero/05-seleccion-asiento/`. Selección local usa estilo `Seleccionado`; respuesta HELD usa `Pendiente de pago`; `Confirmed` aparece `Ocupado`. Calcular el contador desde `expiresAtUtc`; mantener el token en estado de la sesión SPA, nunca en URL ni almacenamiento persistente, y enviarlo solo en `X-Hold-Token`.
- Mantener `@media`/mobile-first, contraste, navegación accesible y el resumen fijo inferior indicados por DESIGN.md. Un 409 refresca el mapa y presenta el mensaje accionable aprobado.

### Pruebas y CI

- xUnit usa un `TimeProvider` controlado por el test para probar transiciones válidas e inválidas y vencimiento sin dormir diez minutos; Application también comprueba el límite inclusivo de diez asientos y que once produzca error de validación sin llamar al repositorio.
- Integration con PostgreSQL Testcontainers comprueba rollback todo-o-nada, fila bloqueada e índice parcial; 50 solicitudes reales sobre el mismo asiento producen 1 respuesta 200 y 49 respuestas 409, sin asociaciones activas duplicadas. Otra prueba avanza el reloj e invoca un ciclo de expiración para verificar la liberación en PostgreSQL.
- Jest frontend comprueba plano/estados, selección, countdown y conflicto; Jest API valida contratos 200/204/400/404/409, el máximo de diez asientos, creación, conflicto y borrado autenticado por token sin cuerpo en éxito.
- Añadir un paso k6 al job API de CI, que ya levanta Docker Compose y ejecuta tests HTTP. El script descubre un viaje y asiento libre por las APIs públicas, lanza 50 VUs de una iteración sobre el mismo asiento y exporta resumen; CI conserva ese JSON como artifact. La evidencia local real se captura en `docs/evidencias/concurrencia-eq02.md` y no se genera con números estimados.

## Risks / Trade-offs

- **Dos estados activos persistidos (reserva/asociación y TripSeat) pueden divergir** → modificarlos siempre en una sola transacción; probar consistencia final de ambos y preservar el índice parcial.
- **Bloqueos de varios asientos pueden competir** → ordenar IDs antes de `FOR UPDATE`, limitar el trabajo de transacción a validar/guardar y devolver 409 ante conflicto.
- **El worker libera hasta una cadencia después de `expiresAtUtc`** → cadencia fija de 10 s; el cliente usa la hora UTC del servidor y refresca el mapa al llegar a cero.
- **El token opaco es una credencial temporal bearer** → 256 bits aleatorios, almacenar hash, transmitirlo en header y no registrar ni persistirlo en cliente.
- **El conteo de búsqueda puede tardar hasta 60 s en reflejar el hold** → declarar el conteo informativo; nunca autorizar una reserva desde el resultado cacheado.
- **El escenario k6 necesita al menos un asiento libre y fecha sembrada** → el setup descubre la fecha actual de Lima, viaje y primer asiento libre; ejecutar en stack de prueba con base reseteable.

## Migration Plan

1. Añadir tablas/índices en el esquema `reservations` y extender el contrato público de MI03. En Development, el Host aplica la migración MI04 antes de atender requests; en otros entornos se despliega la migración antes de activar el endpoint y worker.
2. Publicar aplicación y endpoints de holds; el mapa puede desplegarse con el mismo release. El worker se inicia con el Host y es seguro en varias instancias por `SKIP LOCKED`.
3. Desplegar la pantalla cuando las APIs respondan. Para rollback, retirar primero frontend, endpoint y worker; permitir que los holds activos venzan o cancelarlos antes de revertir las tablas nuevas. La búsqueda existente conserva su TTL y no requiere migración/invalidation distribuida.

## Open Questions

Ninguna bloquea la propuesta. El header `X-Hold-Token` fue elegido para proteger el secreto de URLs y logs.
