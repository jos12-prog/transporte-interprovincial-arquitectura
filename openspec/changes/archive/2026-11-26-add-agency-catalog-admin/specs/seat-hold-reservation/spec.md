# Spec Delta

## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Mapa público de asientos actualizado
La API SHALL exponer `GET /api/v1/trips/{tripId}/seats` con el plano del modelo, el nombre visible del vehículo y el estado vigente de cada asiento (`Available`, `Held` u `Occupied`), sin datos personales ni caché de búsqueda.

#### Scenario: Consultar plano y disponibilidad actuales
- **WHEN** el cliente consulta un viaje existente
- **THEN** la API devuelve HTTP 200 con el nombre visible del vehículo, los asientos y sus posiciones del `SeatLayout`, traduciendo estados vigentes a `Available`, `Held` u `Occupied`, sin incluir datos personales.

#### Scenario: Viaje inexistente
- **WHEN** el cliente consulta un `tripId` inexistente
- **THEN** la API devuelve HTTP 404 con `ProblemDetails` y no expone detalles internos.
