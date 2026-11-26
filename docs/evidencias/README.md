# Evidencias de entrega — Sprint 2

Índice para relacionar las actividades de entrega con los documentos, pruebas y resultados guardados en el repositorio. No se encontró un PDF de Sprint 2 dentro del repositorio; las filas agrupan las actividades indicadas para esta entrega.

| Actividad | Evidencia / ubicación | Resultado o estado |
|---|---|---|
| Estilo arquitectónico: monolito modular y Clean Architecture | `docs/02-arquitectura-software/`, `src/backend/Modules/` y [`solid-y-patrones.md`](solid-y-patrones.md) | Capas por módulo y contratos públicos; límites verificados con NetArchTest en `tests/Architecture/LayeringTests.cs`. |
| Patrones y principios SOLID | [`solid-y-patrones.md`](solid-y-patrones.md) | Ejemplos con rutas y fragmentos del código actual. Adapter externo Culqi/WhatsApp se identifica como fuera de alcance y no implementado. |
| SDD / OpenSpec | `openspec/specs/*` y `openspec/changes/archive/*` | Specs principales: `domain-baseline`, `seat-hold-reservation`, `service-health`, `trip-search`. Cambios archivados: `2026-11-21-normalize-documentation-baseline`, `2026-11-22-add-seat-hold-reservation`, `2026-11-22-add-trip-search`, `2026-11-22-setup-solution-skeleton`. |
| Endpoints y documentación OpenAPI | Swagger/OpenAPI del API en `http://localhost:8080/swagger` y `src/backend/Modules/*/Presentation/`; [captura 02 — Swagger](capturas/02-swagger.png) | Endpoints incluidos: health, búsqueda pública, mapa, holds y liberación. |
| Base de datos, índices y caché | [`bd-y-cache.md`](bd-y-cache.md) | EXPLAIN real con y sin índices en PostgreSQL 16.16 (**captura histórica:** 70 viajes, 1.120 asientos, 5 rutas); incluye el `Seq Scan` de búsqueda, cambio de plan del conteo, método B-Tree y tiempos reales de Redis activado/desactivado. El seed Development actualizado genera **34 rutas, 238 viajes y 2.688 asientos**; esos resultados de rendimiento corresponden a la captura histórica y no se presentan como mediciones del volumen actual. |
| Pruebas .NET y cobertura backend | `tests/Unit/`, `tests/Architecture/`, `tests/Integration/`; [captura 03 — cobertura backend](capturas/03-cobertura-backend.png) | xUnit + coverlet, NetArchTest e integración con PostgreSQL/Testcontainers. Para `add-platform-admin`: Integration **15/15** (base recreada con `docker compose down -v`). Cobertura: **77,5 % líneas (2.123/2.738) y 71 % ramas**. Comandos reales: `dotnet test src/backend/TransportPlatform.sln -m:1 --collect:"XPlat Code Coverage" --results-directory coverage/backend` y `dotnet tool run reportgenerator -reports:"coverage/backend/**/coverage.cobertura.xml" -targetdir:coverage/backend/report -reporttypes:Html`. |
| Pruebas y cobertura frontend | `src/frontend/src/**/*.test.tsx`; [captura 04 — cobertura frontend](capturas/04-cobertura-frontend.png) | Jest + React Testing Library: **16 pruebas**; cobertura **90,43 % líneas, 86,79 % sentencias, 78,48 % ramas y 79,12 % funciones**. Comando: `npm --prefix src/frontend test -- --coverage`. |
| Pruebas HTTP de API | `tests/api/` | Jest contra API real. Para `add-platform-admin`: plataforma **8/8** e identidad **4/4**. Comando: `$env:API_BASE_URL='http://localhost:8080'; npm --prefix tests/api run test:coverage`. Resultado anterior tras incorporar reservas: 3 suites, 7 pruebas pasaron. La cobertura 0 % es esperable en este cliente de caja negra; la cobertura de C# procede de xUnit + ReportGenerator. |
| GitHub Actions / CI | [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml); [captura 05 — Actions](capturas/05-actions.png) | CI verde informado para las ramas de los changes. Los jobs cubren backend, frontend, Compose/API+k6 y `openspec validate --all --strict`. |
| Colección Postman | [`RutaAndina-Sprint2.postman_collection.json`](../../tests/postman/RutaAndina-Sprint2.postman_collection.json), [`README.md`](../../tests/postman/README.md), [`postman-sprint2.json`](postman-sprint2.json), [`postman-platform-admin.json`](postman-platform-admin.json); [captura 01 — Newman](capturas/01-newman.png) | Newman general: **9/9 requests, 19/19 assertions, 0 fallos**. Evidencia adicional de administración de plataforma: **77/77 assertions, 0 fallos**. |
| Carga de búsqueda / EQ-01 y EQ-08 | `tests/performance/search-load.js`, [`search-load-redis-enabled.json`](search-load-redis-enabled.json), [`search-load-redis-disabled.json`](search-load-redis-disabled.json), [`bd-y-cache.md`](bd-y-cache.md) | Registros reales del 2026-11-22: Redis activado/desactivado; tramo de aceptación 200 VUs con p95 37,34 ms / 117,99 ms, sin fallos; Redis activado alcanzó 99,998 % HIT en el tramo. Rampa exploratoria hasta 2.000 VUs, sin aplicar allí el umbral EQ-01. |
| Concurrencia de retención / EQ-02 | [`concurrencia-eq02.md`](concurrencia-eq02.md), [`concurrencia-eq02-summary.json`](concurrencia-eq02-summary.json), `tests/performance/seat-hold-concurrency.js` | Ejecución real: 50 VUs al mismo asiento, 1 HTTP 200, 49 HTTP 409, cero inesperadas; 51/51 checks aprobados, cero fallos HTTP, p95 32,72 ms. |
| Capturas para la presentación | [01 Newman](capturas/01-newman.png), [02 Swagger](capturas/02-swagger.png), [03 cobertura backend](capturas/03-cobertura-backend.png), [04 cobertura frontend](capturas/04-cobertura-frontend.png), [05 Actions](capturas/05-actions.png) | Capturas enlazadas desde sus actividades correspondientes. |

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

npx newman run tests/postman/RutaAndina-Sprint2.postman_collection.json --env-var "agentPassword=$env:IDENTITY_SEED_AGENT_PASSWORD" --env-var "agencyAdminPassword=$env:IDENTITY_SEED_AGENCY_ADMIN_PASSWORD" --reporters cli,json --reporter-json-export docs/evidencias/postman-agency-catalog.json
docker compose down
```

Si hay un volumen local de una versión anterior del esquema, recrearlo antes del arranque con `docker compose down -v`; esto elimina el volumen local de PostgreSQL. Verificación reportada: CI #22 verde en `feat/add-agency-catalog-admin` (commit `0ebadda`), Integration **17/17**, Jest API plataforma/catálogo **9/9**, Jest identidad **4/4** y Newman local **58 requests, 92/92 assertions, 0 fallos**.

### Criterios de la rúbrica relacionados

| Criterio | Evidencia de este change |
|---|---|
| [1. Problema y requerimientos](../criterios-evaluacion.md#1-problema-y-requerimientos) | Catálogo funcional de rutas, tarifas, vehículos y viajes conforme a RF30–RF33, con límites de empresa/agencia. |
| [4. Desarrollo basado en SDD](../criterios-evaluacion.md#4-desarrollo-basado-en-sdd) | [Proposal, design, tasks y delta specs](../../openspec/changes/add-agency-catalog-admin/); validación OpenSpec. El change sigue activo y aún debe archivarse. |
| [5. Pruebas unitarias e integración](../criterios-evaluacion.md#5-pruebas-unitarias-e-integracion) | xUnit existente y Integration/Testcontainers **17/17**, incluida la empresa externa al seed y los flujos del catálogo. |
| [6. Pruebas End to End](../criterios-evaluacion.md#6-pruebas-end-to-end) | Jest API contra la API real y Newman **58 requests, 92/92 assertions, 0 fallos**. |
| [7. Interfaz y UX](../criterios-evaluacion.md#7-interfaz-y-ux) | Capturas 06–12: empresas/modelos de superadmin y pantallas de rutas, vehículos, viajes y tarifas de agencia. |

## Administración de plataforma — criterio de rúbrica

**Criterio asociado:** implementación funcional de los catálogos de superadministración con control de acceso, validación de respuestas y verificación automatizada. La evidencia cubre empresas, agencias y modelos; la exclusión pública de registros inactivos; y los contratos HTTP con respuestas de éxito y error. Se acredita con Integration **15/15**, Jest API (**8/8** administración de plataforma y **4/4** identidad) y Newman (**77/77 assertions**, 0 fallos), junto con la colección [`RutaAndina-Sprint2.postman_collection.json`](../../tests/postman/RutaAndina-Sprint2.postman_collection.json) y el resultado [`postman-platform-admin.json`](postman-platform-admin.json).

No se encontró el PDF ni un archivo con los criterios oficiales de la rúbrica de Sprint 2 en el repositorio. La relación anterior describe el criterio evidenciado sin atribuirle una etiqueta oficial.

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
