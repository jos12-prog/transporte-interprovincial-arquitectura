# Evidencias de entrega — Sprint 2

Índice que relaciona las 8 actividades y los 5 entregables del Sprint 2 con los documentos, pruebas y resultados del repositorio. Las cifras de la columna "Resultado" se midieron el **2026-11-26**, salvo donde se indica otra fecha. El resumen de atributos de calidad (objetivo frente a lo obtenido) está en [**atributos-calidad.md**](atributos-calidad.md).

| Actividad del Sprint 2 | Evidencia / ubicación | Resultado |
|---|---|---|
| 1. Estilo arquitectónico: monolito modular | `docs/02-arquitectura-software/`, `src/backend/Modules/`, [`solid-y-patrones.md`](solid-y-patrones.md) | Módulos con contratos públicos y esquema propio en PostgreSQL. NetArchTest (`tests/Architecture/LayeringTests.cs`) verifica los límites: 2/2 en verde. |
| 2. Patrón arquitectónico: Clean Architecture | `src/backend/Modules/*/{Domain,Application,Infrastructure,Presentation}` | Capas por módulo; la regla de dependencias la comprueba NetArchTest. |
| 3. Especificaciones SDD (OpenSpec) | `openspec/specs/*`, `openspec/changes/archive/*` | 7 specs principales: `domain-baseline`, `service-health`, `trip-search`, `seat-hold-reservation`, `identity-access`, `platform-administration`, `agency-catalog-administration`. 7 changes archivados, cada uno con proposal, design, tasks y criterios de aceptación (escenarios WHEN/THEN). `openspec validate --all --strict` en verde. |
| 4. Endpoints REST con OpenAPI/Swagger | `http://localhost:8080/swagger`; [captura 02](capturas/02-swagger.png), [02b](capturas/02b-swagger.png), [02c](capturas/02c-swagger.png) | 41 endpoints en 8 grupos: búsqueda, asientos y holds, autenticación, agencias del personal, administración de plataforma, catálogo de agencia y health. Los endpoints protegidos usan JWT (candado en Swagger). |
| 5. Base de datos y caché | [`bd-y-cache.md`](bd-y-cache.md) | EXPLAIN real con y sin índices (captura histórica del 22/11: 70 viajes, 1.120 asientos), índices B-Tree, Redis cache-aside con circuit breaker y comparación con Redis activado y desactivado. Incluye el hallazgo y la corrección del 26/11. |
| 6. SOLID y patrones de diseño | [`solid-y-patrones.md`](solid-y-patrones.md) | Ejemplos con rutas del código actual. |
| 7. Pruebas automatizadas y CI/CD | Frontend: `src/frontend/src/**/*.test.tsx`; API: `tests/api/`; backend: `tests/Unit`, `tests/Integration`; [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) | Jest frontend **72/72** (10 suites), cobertura **83,11 % líneas**, 75,22 % sentencias, 63,99 % ramas, 68,44 % funciones ([04](capturas/04-cobertura-frontend.png), [04b](capturas/04b-cobertura-frontend-resumen.png)). xUnit: 85 unitarias, 2 de arquitectura y 17 de integración, cobertura **81,3 % líneas** y 69,6 % ramas ([03](capturas/03-cobertura-backend.png), [03b](capturas/03b-pruebas-backend.png)). GitHub Actions: CI #24 de `main` en verde ([05](capturas/05-actions.png)). |
| 8. Atributos de calidad (Postman, k6, Jest) | [**atributos-calidad.md**](atributos-calidad.md), `tests/postman/`, `tests/performance/` | Newman **58/58** solicitudes y **92/92** aserciones ([01](capturas/01-newman.png)). k6 de 200 a 2.000 VUs: con Redis, p95 103 ms con 200 VUs, **99,86 % HIT** y **0 errores en 764.317 solicitudes** ([13](capturas/13-k6-dashboard-redis-activado.png), [14a](capturas/14a-k6-umbrales-redis-activado.png), [14b](capturas/14b-k6-resultados-redis-activado.png)); sin Redis, p95 158 ms y 0 errores ([15a](capturas/15a-k6-umbrales-redis-desactivado.png), [15b](capturas/15b-k6-resultados-redis-desactivado.png)). Concurrencia EQ-02 (22/11): 1 éxito y 49 respuestas 409 ([concurrencia-eq02.md](concurrencia-eq02.md)). |

### Entregables

| Entregable | Dónde está |
|---|---|
| Código fuente organizado según la arquitectura | `src/backend/Modules/`, `src/frontend/src/` |
| Primera versión funcional | `docker compose up --build -d`; capturas de UI 06 a 12 |
| Documentación de API con OpenAPI/Swagger | `/swagger`; capturas 02, 02b y 02c |
| Pruebas con Jest y reporte de cobertura | Capturas 03, 03b, 04 y 04b; `npm --prefix src/frontend test -- --coverage` |
| Evidencias de validación de atributos de calidad | [atributos-calidad.md](atributos-calidad.md); reportes k6 en JSON y HTML (`search-load-redis-*-2026-11-26.json`, `k6-busqueda-redis-*.html`); [postman-agency-catalog.json](postman-agency-catalog.json) |

### Archivos de resultados

- **k6, 2026-11-26:** [`search-load-redis-enabled-2026-11-26.json`](search-load-redis-enabled-2026-11-26.json), [`search-load-redis-disabled-2026-11-26.json`](search-load-redis-disabled-2026-11-26.json), y los reportes con gráficos [`k6-busqueda-redis-activado.html`](k6-busqueda-redis-activado.html) y [`k6-busqueda-redis-desactivado.html`](k6-busqueda-redis-desactivado.html) (se abren en el navegador).
- **k6, 2026-11-22 (histórico):** [`search-load-redis-enabled.json`](search-load-redis-enabled.json) y [`search-load-redis-disabled.json`](search-load-redis-disabled.json).
- **Newman:** [`postman-agency-catalog.json`](postman-agency-catalog.json) (26/11), [`postman-platform-admin.json`](postman-platform-admin.json) y [`postman-sprint2.json`](postman-sprint2.json) (históricos).

### Cómo repetir la carga con k6

```powershell
docker compose up --build -d
$env:API_BASE_URL='http://localhost:8080'; $env:SEARCH_ORIGIN='Ayacucho'; $env:SEARCH_DESTINATION='Lima'
$env:K6_WEB_DASHBOARD='true'; $env:REDIS_MODE='enabled'; $env:K6_WEB_DASHBOARD_EXPORT='docs/evidencias/k6-busqueda-redis-activado.html'
1..30 | ForEach-Object { Invoke-WebRequest "http://localhost:8080/api/v1/trips/search?origin=Ayacucho&destination=Lima&date=2026-11-26" -UseBasicParsing | Out-Null }
Start-Sleep -Seconds 35
k6 run --summary-export docs/evidencias/search-load-redis-enabled-2026-11-26.json tests/performance/search-load.js
docker compose stop redis
$env:REDIS_MODE='disabled'; $env:K6_WEB_DASHBOARD_EXPORT='docs/evidencias/k6-busqueda-redis-desactivado.html'
k6 run --summary-export docs/evidencias/search-load-redis-disabled-2026-11-26.json tests/performance/search-load.js
docker compose start redis
```

Cambia la fecha de la búsqueda por el día de la prueba. Mientras corre, el panel en vivo está en `http://localhost:5665`.

## add-agency-catalog-admin

### Qué se verificó

- MI03 y MI04 sobre una base nueva: migración de esquema, seed regional por empresa, programación y cancelación transaccional, reservas/asientos, caché y conflictos de ámbito.
- Contratos HTTP del catálogo, búsqueda y mapa: Jest API de plataforma/catálogo **9/9** (5 suites) e identidad **4/4**.
- Newman contra la API en Docker Compose: **58 requests, 92/92 assertions, 0 fallos**; reporte [postman-agency-catalog.json](postman-agency-catalog.json).
- Seed Development: **34 rutas, 238 viajes y 2.688 `trip_seats`**. Las rutas y tarifas se separan por empresa; se incluyen las provincias de Ayacucho y Ayacucho–Lima.
- Capturas de superadmin: [06 — Empresas](capturas/06-superadmin-empresas.png), [07 — Modelos](capturas/07-superadmin-modelos.png).
- Capturas de agencia: [08 — Vehículos](capturas/08-agencia-vehiculos.png), [09 — Registrar vehículo](capturas/09-agencia-registrar-vehiculo.png), [10 — Viajes](capturas/10-agencia-viajes.png), [11 — Tarifas](capturas/11-agencia-tarifas.png), [12 — Rutas](capturas/12-agencia-rutas.png).

### Comandos de verificación

Desde PowerShell en la raíz. El test Integration crea su PostgreSQL de Testcontainers; los comandos de Jest API y Newman apuntan al API de Compose.

```powershell
$env:MSBUILDDISABLENODEREUSE='1'
$env:DOTNET_CLI_USE_MSBUILD_SERVER='0'
dotnet test tests/Integration/TransportPlatform.Integration.Tests.csproj -m:1 -nodeReuse:false

docker compose up --build -d
docker compose ps
Invoke-RestMethod http://localhost:8080/health

$env:API_BASE_URL='http://localhost:8080'
npm --prefix tests/api run test:coverage

npx newman run tests/postman/RutaAndina-Sprint2.postman_collection.json --env-var "agentPassword=$env:IDENTITY_SEED_AGENT_PASSWORD" --env-var "agencyAdminPassword=$env:IDENTITY_SEED_AGENCY_ADMIN_PASSWORD" --env-var "superAdminPassword=$env:IDENTITY_SEED_SUPER_ADMIN_PASSWORD" --reporters cli,json --reporter-json-export docs/evidencias/postman-agency-catalog.json
docker compose down
```

Si hay un volumen local de una versión anterior del esquema, recrearlo antes del arranque con `docker compose down -v`; esto elimina el volumen local de PostgreSQL. Verificación reportada: CI #22 verde en `feat/add-agency-catalog-admin` (commit `0ebadda`), Integration **17/17**, Jest API plataforma/catálogo **9/9**, Jest identidad **4/4** y Newman local **58 requests, 92/92 assertions, 0 fallos**.

### Criterios de la rúbrica relacionados

| Criterio | Evidencia de este change |
|---|---|
| [1. Problema y requerimientos](../criterios-evaluacion.md#1-problema-y-requerimientos) | Catálogo funcional de rutas, tarifas, vehículos y viajes conforme a RF30–RF33, con límites de empresa/agencia. |
| [4. Desarrollo basado en SDD](../criterios-evaluacion.md#4-desarrollo-basado-en-sdd) | [Proposal, design, tasks y delta specs](../../openspec/changes/archive/2026-11-26-add-agency-catalog-admin/); archivado el 2026-11-26 con las specs sincronizadas y `openspec validate --all --strict` en verde. |
| [5. Pruebas unitarias e integración](../criterios-evaluacion.md#5-pruebas-unitarias-e-integracion) | xUnit existente y Integration/Testcontainers **17/17**, incluida la empresa externa al seed y los flujos del catálogo. |
| [6. Pruebas End to End](../criterios-evaluacion.md#6-pruebas-end-to-end) | Jest API contra la API real y Newman **58 requests, 92/92 assertions, 0 fallos**. |
| [7. Interfaz y UX](../criterios-evaluacion.md#7-interfaz-y-ux) | Capturas 06–12: empresas/modelos de superadmin y pantallas de rutas, vehículos, viajes y tarifas de agencia. |

## Administración de plataforma — criterio de rúbrica

**Criterio asociado:** implementación funcional de los catálogos de superadministración con control de acceso, validación de respuestas y verificación automatizada. La evidencia cubre empresas, agencias y modelos; la exclusión pública de registros inactivos; y los contratos HTTP con respuestas de éxito y error. Se acredita con Integration **15/15**, Jest API (**8/8** administración de plataforma y **4/4** identidad) y Newman (**77/77 assertions**, 0 fallos), junto con la colección [`RutaAndina-Sprint2.postman_collection.json`](../../tests/postman/RutaAndina-Sprint2.postman_collection.json) y el resultado [`postman-platform-admin.json`](postman-platform-admin.json).


## Newman contra Docker Compose

Ejecutar en este orden desde PowerShell, en la raíz del repositorio:

```powershell
docker compose up --build -d
docker compose ps
Invoke-RestMethod http://localhost:8080/health
npx newman run tests/postman/RutaAndina-Sprint2.postman_collection.json --reporters cli,json --reporter-json-export docs/evidencias/postman-sprint2.json
docker compose down
```

Espera a que `docker compose ps` muestre los servicios healthy y que `/health` muestre `postgresql`, `redis` y `rabbitmq` saludables antes de Newman. El test Newman queda registrado en `docs/evidencias/postman-sprint2.json`.
