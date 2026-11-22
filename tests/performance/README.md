# Pruebas de rendimiento con k6

Ejecuta los comandos desde la raíz del repositorio. Requieren Docker Compose y k6 instalados. `SEARCH_DATE` por omisión es la fecha actual de `America/Lima`; la semilla Development incluye viajes para esa fecha. Puedes fijar `SEARCH_DATE` en formato `yyyy-MM-dd` si ambas ejecuciones deben usar una fecha concreta de la semilla. Origen y destino por omisión: Ayacucho–Lima.

## Búsqueda con y sin Redis

El perfil `search-load.js` ejecuta primero la aceptación EQ-01 a 200 VUs durante 45 segundos. Sus umbrales son p95 ≤ 2 s, menos de 1 % de errores HTTP y al menos 80 % de `X-Cache: HIT` con Redis activado. Luego sube progresivamente hasta 2.000 VUs y mantiene ese nivel 15 segundos como observación exploratoria; no aplica ahí el umbral EQ-01. Los contadores `search_cache_hit` y `search_cache_miss` registran cada encabezado observado, y `search_cache_hit_rate` permite comprobar EQ-08. Cada comando exporta su resumen JSON para conservar los resultados.

```powershell
docker compose up -d
docker compose ps
$env:API_BASE_URL = 'http://localhost:8080'
$env:SEARCH_ORIGIN = 'Ayacucho'
$env:SEARCH_DESTINATION = 'Lima'
$env:REDIS_MODE = 'enabled'
k6 run --summary-export docs/evidencias/search-load-redis-enabled.json tests/performance/search-load.js
docker compose stop redis
$env:REDIS_MODE = 'disabled'
k6 run --summary-export docs/evidencias/search-load-redis-disabled.json tests/performance/search-load.js
docker compose start redis
```

La ejecución desactivada detiene solo el contenedor Redis; la API continúa sirviendo la búsqueda y su cache-aside degrada a PostgreSQL. El health check global puede quedar `unhealthy` mientras Redis está detenido. Al reiniciarlo, espera a que vuelva a `healthy` antes de usar otras funciones del stack. Para que la comparación sea útil, conserva origen, destino, fecha, máquina y perfil iguales en ambos comandos. La prueba puede tardar varios minutos y el tramo de 2.000 VUs es exploratorio; ejecútalo en un equipo que pueda sostenerlo.

Registra en `docs/evidencias/bd-y-cache.md` la fecha, versión de k6, hardware, conteos de `search_cache_hit`/`search_cache_miss`, promedio, mediana y p95 para ambos modos. No copies los tiempos de una máquina como resultado general.

## Smoke test de salud

Desde la raíz del repositorio, inicia el stack y confirma que los servicios están saludables:

```powershell
docker compose up --build -d
docker compose ps
$env:BASE_URL = 'http://localhost:8080'
k6 run tests/performance/smoke-health.js
docker compose down
```

El smoke test solicita `GET /health` una vez y falla si la API no devuelve HTTP 200.
