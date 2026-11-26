# Seat Hold Reservation Specification

## Purpose

Permitir que pasajeros consulten el mapa transaccional de un viaje y retengan uno o varios asientos durante diez minutos sin login, con liberación segura y sin sobreventa bajo solicitudes concurrentes.

## Requirements

### Requirement: Mapa público de asientos actualizado
La API SHALL exponer `GET /api/v1/trips/{tripId}/seats` con el plano del modelo, el nombre visible del vehículo y el estado vigente de cada asiento (`Available`, `Held` u `Occupied`), sin datos personales ni caché de búsqueda.

#### Scenario: Consultar plano y disponibilidad actuales
- **WHEN** el cliente consulta un viaje existente
- **THEN** la API devuelve HTTP 200 con el nombre visible del vehículo, los asientos y sus posiciones del `SeatLayout`, traduciendo estados vigentes a `Available`, `Held` u `Occupied`, sin incluir datos personales.

#### Scenario: Viaje inexistente
- **WHEN** el cliente consulta el mapa de un `tripId` inexistente
- **THEN** la API devuelve HTTP 404 con `ProblemDetails` y no expone detalles internos.

### Requirement: Liberación de holds al cancelar un viaje
La cancelación de un viaje SHALL ejecutarse coordinada con MI04: los holds `HELD` se cancelan y sus asientos se liberan; la operación SHALL rechazarse si existe algún asiento `CONFIRMED`.

#### Scenario: Cancelar viaje con holds activos
- **WHEN** una cancelación encuentra holds `HELD` y ningún asiento `CONFIRMED` para el viaje
- **THEN** los holds pasan a `CANCELLED`, todos sus asientos pasan a disponibles y la operación se confirma de forma atómica con el estado `Cancelled` del viaje.

#### Scenario: Cancelar viaje con asiento confirmado
- **WHEN** una cancelación encuentra al menos un asiento `CONFIRMED` del viaje
- **THEN** MI04 informa el conflicto al caso de uso de MI03, la API responde HTTP 409 con `ProblemDetails` y no se modifican el viaje, los holds ni los asientos.

#### Scenario: Cancelar viaje sin holds
- **WHEN** se cancela un viaje sin holds `HELD` ni asientos `CONFIRMED`
- **THEN** MI04 confirma que no hay reservas que liberar y MI03 puede completar la cancelación sin crear reservas ni asientos.

#### Scenario: Solicitar hold para un viaje cancelado
- **WHEN** un cliente intenta crear un hold para un viaje cuyo estado es `Cancelled`
- **THEN** la API responde HTTP 409 con `ProblemDetails` y no crea una reserva ni bloquea asientos.

### Requirement: Retención digital atómica
La API SHALL permitir crear con `POST /api/v1/reservations/holds` una retención de uno a diez `tripSeatIds` por diez minutos, devolviendo un `holdToken` opaco que identifica al titular temporal y la hora de expiración UTC.

#### Scenario: Retener asientos disponibles
- **WHEN** se solicitan uno o varios asientos existentes y libres del viaje
- **THEN** la API crea una sola reserva `HELD` para todo el grupo y responde HTTP 200 con su identificador, `holdToken` y `expiresAtUtc`.

#### Scenario: Solicitud inválida o recurso inexistente
- **WHEN** la petición está mal formada, incluye asientos repetidos o referencia un viaje/asiento inexistente
- **THEN** la API devuelve HTTP 400 para la petición inválida o HTTP 404 para el recurso inexistente, mediante `ProblemDetails`, sin crear una retención parcial.

#### Scenario: Superar el máximo de asientos por hold
- **WHEN** la petición contiene más de diez `tripSeatIds`
- **THEN** la API devuelve HTTP 400 con `ProblemDetails` y no crea una retención ni modifica ningún asiento.

#### Scenario: Algún asiento ya no está libre
- **WHEN** al menos uno de los asientos solicitados está `Held` u `Occupied`
- **THEN** la API devuelve HTTP 409 con `ProblemDetails` y no retiene ninguno de los asientos solicitados.

#### Scenario: Cincuenta solicitudes simultáneas para el mismo asiento
- **WHEN** cincuenta solicitudes concurrentes intentan retener el mismo `TripSeat` libre
- **THEN** exactamente una responde HTTP 200 y las otras cuarenta y nueve responden HTTP 409; PostgreSQL conserva como máximo una reserva activa para ese asiento.

### Requirement: Liberación autenticada por token temporal
La API SHALL permitir liberar una reserva `HELD` mediante `DELETE /api/v1/reservations/holds/{id}` y el header `X-Hold-Token`, sin requerir login ni exponer el token en la URL.

#### Scenario: Titular libera su retención
- **WHEN** la solicitud presenta un identificador de hold vigente y su `X-Hold-Token` válido
- **THEN** la API responde HTTP 204 No Content sin cuerpo, cambia la reserva a `CANCELLED` y libera todos sus asientos en la misma operación.

#### Scenario: Identificador o token no autorizado
- **WHEN** el hold no existe o el token no corresponde a su titular temporal
- **THEN** la API responde HTTP 404 con `ProblemDetails` sin revelar si el hold de otra persona existe.

### Requirement: Expiración automática de retenciones
Una retención `HELD` SHALL vencer diez minutos después de su creación y liberar todos sus asientos de forma atómica; los flujos de confirmación y reprogramación quedan fuera de este contrato.

#### Scenario: Expirar un hold vencido
- **WHEN** vence `expiresAtUtc` y se ejecuta el siguiente ciclo de expiración
- **THEN** la reserva pasa a `EXPIRED`, todos sus `TripSeat` vuelven a `Available` y otro hold puede adquirirlos.

#### Scenario: Varias instancias procesan expiraciones
- **WHEN** varias instancias examinan simultáneamente los mismos holds vencidos
- **THEN** cada hold se procesa una sola vez y ningún asiento queda asociado a más de una reserva activa `HELD` o `CONFIRMED`.
