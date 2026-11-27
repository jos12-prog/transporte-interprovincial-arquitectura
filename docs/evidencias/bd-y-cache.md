# Evidencia de índices y caché de búsqueda

Este procedimiento debe ejecutarse contra PostgreSQL 16 con la semilla de Development aplicada. Los planes reales y el entorno se registran durante 6.1. La latencia con Redis activado/desactivado y sus conteos `X-Cache` se registran en 6.2 con k6. No se registran resultados estimados.

## Índices B-Tree de la consulta

Las migraciones MI03/MI04 crean los siguientes índices PostgreSQL B-Tree (método predeterminado de `CREATE INDEX`):

| Esquema/tabla | Columnas | Índice | Uso |
|---|---|---|---|
| `transport_scheduling.routes` | `(origin_city, destination_city)` | `IX_routes_origin_city_destination_city` (único) | Encuentra la ruta por origen y destino. |
| `transport_scheduling.trips` | `(route_id, departure_at_utc)` | `IX_trips_route_id_departure_at_utc` | Filtra y ordena salidas de una ruta por fecha. |
| `reservations.trip_seats` | `(trip_id, status)` | `IX_trip_seats_trip_id_status` | Cuenta asientos disponibles por viaje y estado. |

El índice único de la ruta es B-Tree. La consulta final compara los nombres con `ILIKE` y escape de comodines; se debe confirmar con el plan real si PostgreSQL puede aprovecharlo en este predicado. La búsqueda de MI03 y el conteo de MI04 se explican por separado porque pertenecen a esquemas y módulos distintos; MI03 no consulta directamente la tabla de MI04.

## Consulta de viajes para EXPLAIN

Ejecutar el siguiente SQL para una ruta sembrada; el intervalo representa el día local actual de Lima:

```sql
SELECT r.origin_city,
       r.destination_city,
       t.id AS trip_id,
       t.departure_at_utc,
       t.arrival_at_utc,
       t.price,
       vm.name AS vehicle_model
FROM transport_scheduling.routes AS r
JOIN transport_scheduling.trips AS t ON t.route_id = r.id
JOIN transport_scheduling.vehicles AS v ON v.id = t.vehicle_id
JOIN transport_scheduling.vehicle_models AS vm ON vm.id = v.vehicle_model_id
WHERE r.origin_city ILIKE 'Ayacucho' ESCAPE E'\\'
  AND r.destination_city ILIKE 'Lima' ESCAPE E'\\'
  AND t.departure_at_utc >= (((now() AT TIME ZONE 'America/Lima')::date)::timestamp AT TIME ZONE 'America/Lima')
  AND t.departure_at_utc < ((((now() AT TIME ZONE 'America/Lima')::date + 1)::timestamp) AT TIME ZONE 'America/Lima')
ORDER BY t.departure_at_utc;
```

Guardar la salida de `EXPLAIN (ANALYZE, BUFFERS)` para esa consulta.

## Conteo de asientos para EXPLAIN

```sql
SELECT trip_id, count(*) AS available_seats
FROM reservations.trip_seats
WHERE trip_id = (
    SELECT t.id
    FROM transport_scheduling.trips AS t
    JOIN transport_scheduling.routes AS r ON r.id = t.route_id
    WHERE r.origin_city ILIKE 'Ayacucho' ESCAPE E'\\'
      AND r.destination_city ILIKE 'Lima' ESCAPE E'\\'
    ORDER BY t.departure_at_utc
    LIMIT 1
)
  AND status = 'Available'
GROUP BY trip_id;
```

Guardar también la salida de `EXPLAIN (ANALYZE, BUFFERS)` para el conteo.

## Procedimiento antes/después

1. En una transacción, quitar temporalmente los tres índices enumerados arriba y ejecutar ambos `EXPLAIN (ANALYZE, BUFFERS)`; hacer `ROLLBACK` para restaurar los índices. No guardar cambios de esquema.
2. Ejecutar `ANALYZE` sobre las tablas involucradas y repetir ambos `EXPLAIN (ANALYZE, BUFFERS)` con los índices restaurados.
3. Registrar los dos planes, `Planning Time`, `Execution Time`, buffers, versión de PostgreSQL, configuración del contenedor y fecha de ejecución. Confirmar en `pg_indexes.indexdef` que las estructuras se muestran con `USING btree`.

## Resultados reales de EXPLAIN (6.1)

Ejecución del 2026-11-22 en PostgreSQL 16.16 dentro de Docker Compose, con la semilla Development: 70 viajes, 1.120 `trip_seats` y 5 rutas. La fase sin índices quitó temporalmente los tres índices de búsqueda dentro de una transacción y terminó con `ROLLBACK`; la fase con índices ejecutó `ANALYZE` antes de repetir los planes.

| Consulta | Fase sin índices | Fase con índices | Lectura del resultado |
|---|---|---|---|
| Búsqueda de viajes | `Seq Scan` en `routes` y `trips`; 11 buffers compartidos; planificación 0.400 ms; ejecución 0.103 ms. | `Seq Scan` en `routes` y `trips`; 11 buffers compartidos; planificación 1.253 ms; ejecución 0.232 ms. | El plan no cambió a uso de los índices de rutas/viajes. Con cinco rutas y 70 viajes, los escaneos secuenciales son razonables; además, el predicado `ILIKE` no aprovecha el B-Tree ordinario en estos planes. |
| Conteo de asientos disponibles | `Bitmap Heap Scan` en `trip_seats` con `IX_trip_seats_trip_id_seat_number`; 30 buffers compartidos; planificación 0.107 ms; ejecución 0.116 ms. | `Index Only Scan` en `trip_seats` usando `IX_trip_seats_trip_id_status`; 6 buffers compartidos, 0 heap fetches; planificación 0.296 ms; ejecución 0.229 ms. | El plan usa el índice compuesto apropiado para `trip_id` y `status`, evitando lecturas del heap en esta ejecución. |

La salida de `pg_indexes` para `indexname LIKE 'IX_%'` confirmó que los 12 índices enumerados mostraban `USING btree`; en particular, `IX_routes_origin_city_destination_city`, `IX_trips_route_id_departure_at_utc` y `IX_trip_seats_trip_id_status`. Esta consulta de catálogo confirmó el método físico declarado por PostgreSQL.

Los cuatro tiempos de ejecución son inferiores a 1 ms, pero no son comparables como evidencia de rendimiento con este volumen pequeño. La observación registrada es el cambio de plan y de buffers del conteo de asientos; el plan de búsqueda de viajes permaneció en `Seq Scan` en ambas fases. Estos resultados no demuestran el comportamiento bajo carga o con un volumen de producción.

## Latencia Redis activado/desactivado

### Ejecución real (2026-11-22)

Prueba local en Windows/PowerShell con Docker Compose y k6 v2.4.0; consulta Ayacucho–Lima para la fecha de servicio local predeterminada. El perfil sostuvo 200 VUs durante 45 s y después hizo la rampa exploratoria hasta 2.000 VUs. Los umbrales EQ-01 aplicaron solo al tramo de 200 VUs. Los resúmenes completos están en `docs/evidencias/search-load-redis-enabled.json` y `docs/evidencias/search-load-redis-disabled.json`. No se registró el hardware del host.

| Modo | Solicitudes totales | Promedio | Mediana | p95 | Máximo | `X-Cache: HIT` | `X-Cache: MISS` |
|---|---:|---:|---:|---:|---:|---:|---:|
| Redis activado | 1.115.214 | 82,79 ms | 31,32 ms | 235,59 ms | 2.370,39 ms | 1.114.116 (99,90 %) | 1.098 |
| Redis detenido (fail-open a PostgreSQL) | 308.302 | 301,13 ms | 103,28 ms | 924,01 ms | 1.225,37 ms | 0 (0 %) | 308.302 |

Resultados del tramo de aceptación EQ-01 a 200 VUs:

| Modo | Solicitudes | Promedio | Mediana | p95 | Fallos HTTP | HIT repetitivos |
|---|---:|---:|---:|---:|---:|---:|
| Redis activado | 363.136 | 24,56 ms | 23,49 ms | 37,34 ms | 0 % | 363.129 HIT / 7 MISS (99,998 %) |
| Redis detenido | 99.789 | 89,99 ms | 86,37 ms | 117,99 ms | 0 % | 0 HIT / 99.789 MISS |

EQ-01 pasó en ambas ejecuciones: el p95 de aceptación fue ≤2 s y no hubo fallos HTTP. EQ-08 pasó con Redis activado (99,998 % HIT en el tramo de aceptación, sobre el 80 % requerido). La fase exploratoria alcanzó los 2.000 VUs; su umbral no es de aceptación y no se extrapola el SLO a esa carga. Los tiempos son de esta ejecución local, no una garantía para otro hardware o volumen de datos.

## Hallazgo y corrección

Durante la prueba con Redis detenido, las búsquedas inicialmente esperaban al timeout de conexión antes de continuar con la consulta de origen. Se corrigió la degradación configurando `connectTimeout`, `syncTimeout` y `asyncTimeout` en 500 ms, un límite de 300 ms por operación de caché y un circuit breaker singleton en `RedisTripSearchCache`. Ante un fallo de Redis o timeout de operación, el breaker abre durante 30 s; durante ese intervalo se omite Redis y se consulta el origen por los contratos del módulo, que llega a PostgreSQL.

La fila **Redis detenido (fail-open a PostgreSQL)** de las tablas anteriores registra el resultado después de la corrección: en el tramo de 200 VUs hubo 0 % de fallos HTTP y p95 de 117,99 ms (≈118 ms); en la ejecución completa, el p95 fue 924,01 ms. Todas las respuestas fueron `MISS`, como se espera al no poder usar Redis.

La corrección aplica la táctica de resiliencia **Táctica 5: Retry y Circuit Breaker** de `docs/02-arquitectura-software/tacticas-arquitectonicas.md` (§ “Táctica 5”): adapta el circuit breaker a Redis para que la dependencia de caché no retenga la búsqueda cuando está caída. El breaker de caché abre ante el primer fallo/timeout de una operación y no reintenta durante los 30 s de apertura.

**Corrección 2026-11-26.** La generación de caché que agregó `add-platform-admin` (para invalidar la búsqueda cuando se editan empresas, agencias, rutas, tarifas, vehículos o viajes) se leía de Redis en cada búsqueda, así que había dos operaciones Redis por solicitud. Con 200 VUs simultáneos, un solo timeout de 300 ms abría el breaker 30 s, y el HIT del tramo de aceptación bajó a 52 % (k6, 2026-11-26). Ahora la generación se guarda en memoria 2 s: cada búsqueda vuelve a hacer una sola operación Redis, la misma instancia ve la invalidación de inmediato y otras instancias la ven en 2 s como máximo. Lo cubren `GenerationIsReadFromRedisOnceWithinLocalTtl` e `InvalidationIsVisibleImmediatelyWithoutReadingRedis` en `tests/Unit/RedisTripSearchCacheTests.cs`.

La prueba `tests/Unit/RedisTripSearchCacheTests.cs`, método `FailedGetOpensBreakerAndImmediateGetAndSetSkipDistributedCache`, verifica que el primer fallo abre el breaker y que las llamadas inmediatas siguientes omiten `IDistributedCache`. La prueba `tests/Integration/TripSearchRedisCacheTests.cs`, método `SearchFallsBackToSourceWhenRedisIsUnavailable`, cubre el fallback de la búsqueda. No se prueba el cierre del breaker al cumplirse los 30 s.
