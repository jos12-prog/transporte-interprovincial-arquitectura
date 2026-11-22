# Tasks

## 1. Dominio

- [ ] 1.1 Añadir en MI02 `Company` y `Agency` con `CompanyId`, datos públicos y `Agency` ligada a una empresa; en MI03 `Route`, `VehicleModel`, `SeatLayout`, `Vehicle` y `Trip` con identificadores de empresa/agencia y tiempos UTC; en MI04 `TripSeat` como propietario de disponibilidad. Mantener Domain sin referencias a EF/ASP.NET/paquetes externos; verificar con pruebas xUnit de creación, invariantes estructurales y `dotnet test tests/Unit`.
- [ ] 1.2 Representar en `SeatLayout` asientos numerados y sus ubicaciones por fila/columna/piso cuando corresponda; cubrir Yaris 4, Avanza 6, Minivan 7, Van 15, Bus 1 piso 44 y Bus 2 pisos 52. Probar cantidades, unicidad de números y distribución de piso con xUnit, sin implementar mapa visual ni reglas de reserva.

## 2. Aplicación

- [ ] 2.1 Definir contratos de Aplicación para los datos públicos de MI02/MI03 y `IReservationsApi` de MI04; mantener `ISeatAvailabilityService` interno de MI04. Implementar en MI03 la consulta de conteos agregados de MI04 por el contrato público, sin referencias a sus tablas o entidades; verificar con pruebas de contratos/adaptadores y análisis de dependencias.
- [ ] 2.2 Implementar en MI06 `SearchTripsUseCase` y validación de origen, destino, fecha y filtros RF02 (empresa, agencia, horario y tarifa); devolver datos de itinerario más `availableSeats` por consultas batch. Añadir xUnit para criterios válidos, normalización, filtros, resultado vacío y errores de validación; verificar `dotnet test tests/Unit`.

## 3. Infraestructura

- [ ] 3.1 Implementar mapeos y migraciones EF Core independientes en los esquemas `companies_agencies`, `transport_scheduling` y `reservations`, sin FK ni navegación EF entre esquemas. Verificar migraciones aplicables y reversibles con PostgreSQL de Testcontainers y consultas de aislamiento entre esquemas.
- [ ] 3.2 Sembrar de forma determinista e idempotente varias empresas/agencias y rutas/viajes para Ayacucho, Lima, Huanta, Andahuaylas y Cusco durante los siguientes siete días; crear los `TripSeat` numerados a partir de los planos de modelo e IDs estables. Probar conteo, referencias y reejecución de semillas con Testcontainers PostgreSQL.
- [ ] 3.3 Añadir índices B-Tree justificados por el SQL final para origen/destino, ruta/fecha de viaje y viaje/estado de asiento; completar `docs/evidencias/bd-y-cache.md` con consultas y `EXPLAIN ANALYZE` antes/después y medición de latencia con Redis desactivado/activado. Registrar resultados del entorno real, no valores estimados; verificar que el documento permita repetir las mediciones.
- [ ] 3.4 Implementar cache-aside Redis en MI06 con clave que incluya criterios normalizados y versión, TTL máximo 60 s y `X-Cache: HIT|MISS`; solo cachear la respuesta agregada de búsqueda y degradar a lectura de origen si Redis falla. Verificar en Integration con Testcontainers primera respuesta MISS, repetición HIT, expiración, bypass por fallo Redis y ausencia de lecturas del mapa.

## 4. Presentación y API

- [ ] 4.1 Exponer `GET /api/v1/trips/search` en MI06, anónimo y de solo lectura; responder con empresa/agencia, horarios, modelo, precio y asientos libres, `ProblemDetails` para entrada inválida/errores sanitizados y esquemas/códigos 200/400/500 en Swagger. Verificar con Jest de API contra el Host real respuesta, validación, campos, lista vacía y encabezado `X-Cache`.

## 5. Frontend

- [ ] 5.1 Implementar pantallas de búsqueda/resultados según `design/stitch/pasajero/03-inicio-busqueda/`, `04-resultados/` y `design/stitch/DESIGN.md`; consumir `/api/v1/trips/search` real con ruta/fecha/filtros y mostrar empresa, agencia, salidas, modelo, tarifa y conteo informativo. Cubrir skeleton, errores y estado vacío con Jest + React Testing Library; verificar `npm run build` y Jest con cobertura.

## 6. Pruebas de integración y rendimiento

- [ ] 6.1 Añadir pruebas de integración de búsqueda extremo a extremo con PostgreSQL y Redis Testcontainers, incluyendo seed, filtros, conteo agregado y escenarios de cache HIT/MISS; añadir prueba concurrente de búsquedas de solo lectura sobre el mismo viaje verificando que no modifican `TripSeat` ni cambian el conteo almacenado. No presentar esta prueba como EQ-02 (reserva concurrente), que sigue fuera de alcance; verificar toda la suite de Integration.
- [ ] 6.2 Añadir `tests/performance/search-load.js` con rampa progresiva hasta 2.000 VUs, tramo de aceptación EQ-01 desde al menos 200 VUs (p95 ≤2 s, errores técnicos <1 %) y medición EQ-08 (≥80 % de respuestas repetitivas HIT); guardar `--summary-export` en `docs/evidencias/`. Verificar que k6 reconoce el perfil y que el comando documentado produce el summary.

## 7. Verificación final

- [ ] 7.1 Ejecutar `dotnet build`, `dotnet test`, `npm run build`, Jest de frontend y API, Integration con Testcontainers, `docker compose up`/`down`, k6 y `openspec validate --all --strict`; comprobar OpenAPI, seeds, cache y que no se añadieron login/JWT, mapa, reservas/retenciones, pagos ni CRUD administrativo. Marcar esta tarea solo con resultados satisfactorios y evidencias generadas.
