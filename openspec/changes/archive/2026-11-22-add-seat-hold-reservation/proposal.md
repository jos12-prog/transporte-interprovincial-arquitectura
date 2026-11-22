# Proposal

## Why

La búsqueda ya muestra un conteo informativo, pero el pasajero todavía no puede consultar el plano vigente ni apartar sus asientos de forma transaccional. Este change entrega para Sprint 2 el mapa de asientos y la retención digital de diez minutos, con prevención de sobreventa verificable bajo concurrencia EQ-02.

## What Changes

- Añadir en MI04 `Reservation` y su relación todo-o-nada con uno o varios `TripSeat`, con estados activos/finales `HELD`, `EXPIRED` y `CANCELLED`; definir `CONFIRMED` y `RESCHEDULED` sin habilitar sus flujos en este change.
- Exponer desde MI04 el mapa actualizado `GET /api/v1/trips/{tripId}/seats`, derivado del `SeatLayout` del modelo y del estado transaccional de cada `TripSeat`, sin datos personales ni caché.
- Añadir creación de holds de 1 a 10 asientos por `POST /api/v1/reservations/holds` y liberación por `DELETE /api/v1/reservations/holds/{id}` con `X-Hold-Token`; el hold devuelve un token opaco de titular temporal, sin autenticación. Una solicitud de más de 10 asientos responde 400 `ProblemDetails`; la liberación correcta responde 204 No Content.
- Retener los asientos durante diez minutos con una transacción corta, bloqueo `SELECT ... FOR UPDATE` y salvaguarda de índice único parcial para asientos activos; rechazar el grupo completo con 409 si alguno no está libre.
- Expirar holds vencidos mediante un `BackgroundService` cada diez segundos y `FOR UPDATE SKIP LOCKED`.
- Añadir la pantalla pasajero Stitch 05 con el plano por modelo, estados, acción de retener y contador de expiración.
- Añadir pruebas de estados/expiración, integración de concurrencia (50 intentos, 1 éxito y 49 conflictos), pruebas Jest frontend/API, perfil k6 y evidencia real EQ-02.
- Mantener el conteo cacheado de búsqueda como informativo durante un máximo de 60 segundos; el mapa y la decisión de retener siempre consultan PostgreSQL.

La trazabilidad cubre HU04 y HU06; RF05, RF08, RF09 y RF11–RF13; RC07–RC09; ADR-08 y ADR-09; y EQ-02. RF07 queda cubierto parcialmente en la asociación de asientos: los datos personales requeridos por RF07/RF06 se difieren al paso 06 y a un change posterior.

## Capabilities

### New Capabilities

- `seat-hold-reservation`: mapa transaccional de asientos por viaje, retenciones digitales de diez minutos, liberación con token opaco y expiración automática segura ante concurrencia.

### Modified Capabilities

Ninguna. La capacidad `trip-search` existente permanece compatible: disponibilidad agregada informativa con TTL máximo de 60 segundos.

## Impact

- MI04 Reservations: Domain, Application, Infrastructure y Presentation; tablas de reservas y asociación de asientos en el esquema `reservations`, migración, contrato de lectura de plano con MI03 y servicio de expiración alojado en el Host.
- MI03 TransportScheduling y `Shared.Contracts`: lectura pública del plano `SeatLayout` por viaje, sin dar a MI04 acceso a sus tablas.
- API REST/OpenAPI con los endpoints indicados, liberación 204 No Content y respuestas 200/400/404/409 con `ProblemDetails` en errores.
- Frontend React del pasajero: navegación desde resultados a `design/stitch/pasajero/05-seleccion-asiento/`, plano, selección, confirmación de hold y countdown.
- Unit, Integration/Testcontainers, Jest frontend/API, k6 y `docs/evidencias/concurrencia-eq02.md`. Evaluar una etapa de k6 en el job API existente; la evidencia de una ejecución local se conserva en el documento, no se inventa.

Fuera de alcance: login/JWT, identidad persistente del titular, datos de pasajeros del paso 06, confirmación de pago, Culqi, emisión de pasajes, venta presencial y flujos de cancelación/reprogramación de reservas confirmadas.
