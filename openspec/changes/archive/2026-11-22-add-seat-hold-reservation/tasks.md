# Tasks

## 1. Domain

- [x] 1.1 Añadir `Reservation`, `ReservationSeat` y `ReservationStatus` en MI04 con `CompanyId`, `AgencyId`, `TripId`, estados definidos (`HELD`, `EXPIRED`, `CANCELLED`, `CONFIRMED`, `RESCHEDULED`) y solo transiciones `HELD → EXPIRED|CANCELLED` habilitadas. Probar invariantes y transiciones permitidas/prohibidas con xUnit; Domain no debe referenciar EF Core ni ASP.NET.

## 2. Application

- [x] 2.1 Extender `Shared.Contracts`/`ITransportSchedulingApi` con un DTO neutral y consulta del `SeatLayout` por `tripId`; probar DTO/contrato y que la respuesta de viaje inexistente se represente sin consultar tablas de MI03 desde MI04.
- [x] 2.2 Añadir contratos MI04 `IReservationRepository` y casos de uso de mapa, crear hold, liberar hold y expirar; validar IDs y grupo todo-o-nada, máximo 10 asientos (11 produce 400 antes de llamar al repositorio), generar token opaco y usar `TimeProvider` para `CreatedAtUtc`/`ExpiresAtUtc`. Cubrir con xUnit 1 y 10 asientos aceptados, 0 y 11 rechazados, no encontrado, conflicto, token y vencimiento con un reloj controlado sin esperas reales.

## 3. Infrastructure

- [x] 3.1 Añadir mapeos EF de `reservations` y `reservation_seats` en el esquema MI04; persistir el hash del token y fechas UTC, crear el índice único parcial por `trip_seat_id` con estado `HELD` o `CONFIRMED`, sin FK entre esquemas, y generar la migración. Verificar aplicación y modelo del índice con PostgreSQL Testcontainers.
- [x] 3.2 Implementar `IReservationRepository`: leer/bloquear los `TripSeat` pedidos mediante `SELECT ... FOR UPDATE` en orden estable, y en una transacción crear/cancelar la reserva, actualizar todas sus asociaciones y estados de asiento; traducir la violación del índice parcial a conflicto recuperable. Probar rollback todo-o-nada y liberación en Integration con Testcontainers.
- [x] 3.3 Implementar el worker `BackgroundService` cada 10 s con scope/`DbContext` propio y `FOR UPDATE SKIP LOCKED`; expirar y liberar atómicamente sin doble procesamiento en varias instancias. Probar `HELD → EXPIRED`, liberación y concurrencia de expiradores con PostgreSQL Testcontainers y tiempo controlado.

## 4. Presentation

- [x] 4.1 Exponer en MI04 `GET /api/v1/trips/{tripId}/seats`, `POST /api/v1/reservations/holds` y `DELETE /api/v1/reservations/holds/{id}` con `X-Hold-Token`; traducir `TripSeat` a `Available|Held|Occupied`, documentar 200/204/400/404/409 en Swagger, con DELETE correcto 204 sin cuerpo y errores `ProblemDetails` donde aplica. Verificar status, payload, ausencia de PII y header con pruebas HTTP.

## 5. Frontend

- [x] 5.1 Añadir navegación de resultados a `/pasajero/viajes/:tripId/asientos` y construir pantalla basada en `design/stitch/pasajero/05-seleccion-asiento/` y `design/stitch/DESIGN.md`, dibujando `SeatLayout` dinámico para los modelos sembrados, leyenda, selección, resumen, retención y countdown desde `expiresAtUtc`; conservar `holdToken` solo en memoria y enviarlo en `X-Hold-Token` al liberar. Verificar `npm run build`.
- [x] 5.2 Cubrir en Jest + React Testing Library el mapa y sus modelos, estados libre/seleccionado/pendiente/ocupado, selección múltiple, countdown, éxito, 409 con refresco y accesibilidad/responsive. Verificar `npm run test -- --coverage`.

## 6. Tests, carga y CI

- [x] 6.1 Añadir Integration con Testcontainers que lance 50 solicitudes HTTP concurrentes por el mismo `TripSeat` y verifique exactamente 1 HTTP 200, 49 HTTP 409 y una sola reserva activa; cubrir hold multi-asiento todo-o-nada, máximo de 10 (11 da 400 sin mutación), token incorrecto, expiración que libera y ausencia de reservas activas duplicadas. Verificar liberación correcta como 204 sin cuerpo y ejecutar toda la suite Integration.
- [x] 6.2 Añadir Jest de `tests/api` contra API real para GET mapa, hold 200 (1–10 asientos), límite de 11 con 400 `ProblemDetails`, recurso ausente 404, conflicto 409 y DELETE con/sin `X-Hold-Token` (204 sin cuerpo al liberar); verificar las pruebas con Docker Compose y reporte.
- [x] 6.3 Crear `tests/performance/seat-hold-concurrency.js` con 50 VUs y una solicitud por VU sobre el mismo viaje/asiento, descubriendo un asiento libre por los endpoints públicos; exigir exactamente 1 éxito y 49 conflictos. Documentar ejecución en `tests/performance/README.md`, guardar `--summary-export` en `docs/evidencias/` y registrar en `docs/evidencias/concurrencia-eq02.md` solo resultados reales, ambiente y comando ejecutado.
- [x] 6.4 Actualizar el job API de `.github/workflows/ci.yml` para instalar k6, ejecutar el escenario de 50 VUs contra el stack Compose ya iniciado y subir el summary JSON como artifact; confirmar que `docker compose down` se ejecuta incluso si k6 falla.

## 7. Verificación final

- [x] 7.1 Ejecutar `dotnet build`, `dotnet test` de la solución, `npm run build`, Jest de frontend y API, Docker Compose/Testcontainers, k6 y `openspec validate --all --strict`; verificar Swagger 200/400/404/409, mapa sin caché, retención/expiración, resultado 1+49 de EQ-02 y evidencia real generada. Marcar solo con salidas satisfactorias.
