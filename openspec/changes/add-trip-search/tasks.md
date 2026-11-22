# Tasks

## 1. Dominio

- [x] 1.1 Añadir en MI02 `Company` y `Agency` con `CompanyId`, datos públicos y `Agency` ligada a una empresa; en MI03 `Route`, `VehicleModel`, `SeatLayout`, `Vehicle` y `Trip` con identificadores de empresa/agencia y tiempos UTC; en MI04 `TripSeat` como propietario de disponibilidad. Mantener Domain sin referencias a EF/ASP.NET/paquetes externos; verificar con pruebas xUnit de creación, invariantes estructurales y `dotnet test tests/Unit`.
- [x] 1.2 Representar en `SeatLayout` asientos numerados y sus ubicaciones por fila/columna/piso cuando corresponda; cubrir Yaris 4, Avanza 6, Minivan 7, Van 15, Bus 1 piso 44 y Bus 2 pisos 52. Probar cantidades, unicidad de números y distribución de piso con xUnit, sin implementar mapa visual ni reglas de reserva.

## 2. Aplicación

- [x] 2.1 Crear `Shared.Contracts` neutral con DTO y `ICompaniesAgenciesApi`, `ITransportSchedulingApi` e `IReservationsApi`; referenciarlo desde las Application pertinentes sin referencias directas entre módulos. Mantener `ISeatAvailabilityService` interno de MI04, exponer conteos agregados por lote y hacer que MI03 los consulte vía `IReservationsApi`; verificar con pruebas de contratos/adaptadores y `dotnet test tests/Architecture`.
- [x] 2.2 Implementar en MI06 `SearchTripsUseCase` y validación de origen, destino, fecha y filtros RF02 (empresa, agencia, horario y tarifa); devolver datos de itinerario más `availableSeats` por consultas batch. Añadir xUnit para criterios válidos, normalización, filtros, resultado vacío y errores de validación; verificar `dotnet test tests/Unit`.

## 3. Infraestructura

- [x] 3.1 Implementar mapeos y migraciones EF Core independientes en los esquemas `companies_agencies`, `transport_scheduling` y `reservations`, sin FK ni navegación EF entre esquemas; hacer que el Host aplique migraciones automáticamente solo en `Development`. Verificar aplicabilidad/reversibilidad con PostgreSQL Testcontainers y que entornos no-Development no migren al arrancar. El comando `dotnet ef` de cada módulo lo ejecutará el usuario.
- [x] 3.2 Sembrar de forma determinista e idempotente varias empresas/agencias y rutas/viajes para Ayacucho, Lima, Huanta, Andahuaylas y Cusco durante los siguientes siete días; crear los `TripSeat` numerados a partir de los planos de modelo e IDs estables. En Development, ejecutar la semilla automáticamente después de las migraciones al arrancar el Host. Probar conteo, referencias, reejecución y arranque en Development con Testcontainers PostgreSQL.
- [x] 3.3 Añadir/confirmar índices B-Tree justificados por la consulta de búsqueda para origen/destino, ruta/fecha de viaje y viaje/estado de asiento; crear `docs/evidencias/bd-y-cache.md` con las consultas y el procedimiento reproducible de `EXPLAIN ANALYZE` antes/después. Verificar migraciones e índices con PostgreSQL Testcontainers. Capturar los planes reales y la medición de latencia con Redis activado/desactivado en 6.1, una vez que existan el endpoint y la caché; no inventar resultados.
- [x] 3.4 Implementar cache-aside Redis en MI06 con clave que incluya criterios normalizados y versión, TTL máximo 60 s y `X-Cache: HIT|MISS`; solo cachear la respuesta agregada de búsqueda y degradar a lectura de origen si Redis falla. Verificar en Integration con Testcontainers primera respuesta MISS, repetición HIT, expiración, bypass por fallo Redis y ausencia de lecturas del mapa.

## 4. Presentación y API

- [x] 4.1 Exponer `GET /api/v1/trips/search` en MI06, anónimo y de solo lectura; responder con empresa/agencia, horarios, modelo, precio y asientos libres, `ProblemDetails` para entrada inválida/errores sanitizados y esquemas/códigos 200/400/500 en Swagger. Verificar con Jest de API contra el Host real respuesta, validación, campos, lista vacía y encabezado `X-Cache`.

## 5. Frontend

- [x] 5.1 Implementar pantallas de búsqueda/resultados según `design/stitch/pasajero/03-inicio-busqueda/`, `04-resultados/` y `design/stitch/DESIGN.md`; consumir `/api/v1/trips/search` real con ruta/fecha/filtros y mostrar empresa, agencia, salidas, modelo, tarifa y conteo informativo. Cubrir skeleton, errores y estado vacío con Jest + React Testing Library; verificar `npm run build` y Jest con cobertura.

## 6. Pruebas de integración y rendimiento

- [x] 6.1 Añadir pruebas de integración de búsqueda extremo a extremo con PostgreSQL y Redis Testcontainers, incluyendo seed, filtros, conteo agregado y escenarios de cache HIT/MISS; añadir prueba concurrente de búsquedas de solo lectura sobre el mismo viaje verificando que no modifican `TripSeat` ni cambian el conteo almacenado. Capturar y registrar en `docs/evidencias/bd-y-cache.md` los planes reales de `EXPLAIN ANALYZE` antes/después de los índices. No presentar esta prueba como EQ-02 (reserva concurrente), que sigue fuera de alcance; verificar toda la suite de Integration.
- [x] 6.2 Añadir `tests/performance/search-load.js` con rampa progresiva hasta 2.000 VUs, tramo de aceptación EQ-01 desde al menos 200 VUs (p95 ≤2 s, errores técnicos <1 %) y medición EQ-08 (≥80 % de respuestas repetitivas HIT); guardar `--summary-export` en `docs/evidencias/`. Comparar en esta tarea la latencia del endpoint con Redis activado/desactivado y registrar promedio, mediana, p95 y conteos `X-Cache` en `docs/evidencias/bd-y-cache.md`. Verificar que k6 reconoce el perfil y que el comando documentado produce el summary.

## 7. Verificación final

- [x] 7.1 Ejecutar `dotnet build`, `dotnet test`, `npm run build`, Jest de frontend y API, Integration con Testcontainers, `docker compose up`/`down`, k6 y `openspec validate --all --strict`; comprobar OpenAPI, seeds, cache y que no se añadieron login/JWT, mapa, reservas/retenciones, pagos ni CRUD administrativo. Verificación final reportada satisfactoria: build sin errores, suite .NET, build y Jest del frontend, Jest API contra Docker Compose y validación OpenSpec estricta completados.
