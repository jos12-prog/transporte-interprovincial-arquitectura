# Evidencias de entrega — Sprint 2

Índice para relacionar las actividades de entrega con los documentos, pruebas y resultados guardados en el repositorio. No se encontró un PDF de Sprint 2 dentro del repositorio; las filas agrupan las actividades indicadas para esta entrega.

| Actividad | Evidencia / ubicación | Resultado o estado |
|---|---|---|
| Estilo arquitectónico: monolito modular y Clean Architecture | `docs/02-arquitectura-software/`, `src/backend/Modules/` y [`solid-y-patrones.md`](solid-y-patrones.md) | Capas por módulo y contratos públicos; límites verificados con NetArchTest en `tests/Architecture/LayeringTests.cs`. |
| Patrones y principios SOLID | [`solid-y-patrones.md`](solid-y-patrones.md) | Ejemplos con rutas y fragmentos del código actual. Adapter externo Culqi/WhatsApp se identifica como fuera de alcance y no implementado. |
| SDD / OpenSpec | `openspec/specs/*` y `openspec/changes/archive/*` | Specs principales: `domain-baseline`, `seat-hold-reservation`, `service-health`, `trip-search`. Cambios archivados: `2026-11-21-normalize-documentation-baseline`, `2026-11-22-add-seat-hold-reservation`, `2026-11-22-add-trip-search`, `2026-11-22-setup-solution-skeleton`. |
| Endpoints y documentación OpenAPI | Swagger/OpenAPI del API en `http://localhost:8080/swagger` y `src/backend/Modules/*/Presentation/`; [captura 02 — Swagger](capturas/02-swagger.png) | Endpoints incluidos: health, búsqueda pública, mapa, holds y liberación. |
| Base de datos, índices y caché | [`bd-y-cache.md`](bd-y-cache.md) | EXPLAIN real con y sin índices en PostgreSQL 16.16 (70 viajes, 1.120 asientos, 5 rutas); incluye el `Seq Scan` de búsqueda, cambio de plan del conteo, método B-Tree y tiempos reales de Redis activado/desactivado. |
| Pruebas .NET y cobertura backend | `tests/Unit/`, `tests/Architecture/`, `tests/Integration/`; [captura 03 — cobertura backend](capturas/03-cobertura-backend.png) | xUnit + coverlet, NetArchTest e integración con PostgreSQL/Testcontainers. Cobertura: **77,5 % líneas (2.123/2.738) y 71 % ramas**. Comandos reales: `dotnet test src/backend/TransportPlatform.sln -m:1 --collect:"XPlat Code Coverage" --results-directory coverage/backend` y `dotnet tool run reportgenerator -reports:"coverage/backend/**/coverage.cobertura.xml" -targetdir:coverage/backend/report -reporttypes:Html`. |
| Pruebas y cobertura frontend | `src/frontend/src/**/*.test.tsx`; [captura 04 — cobertura frontend](capturas/04-cobertura-frontend.png) | Jest + React Testing Library: **16 pruebas**; cobertura **90,43 % líneas, 86,79 % sentencias, 78,48 % ramas y 79,12 % funciones**. Comando: `npm --prefix src/frontend test -- --coverage`. |
| Pruebas HTTP de API | `tests/api/` | Jest contra API real. Comando: `$env:API_BASE_URL='http://localhost:8080'; npm --prefix tests/api run test:coverage`. Resultado comunicado tras incorporar reservas: 3 suites, 7 pruebas pasaron. La cobertura 0 % es esperable en este cliente de caja negra; la cobertura de C# procede de xUnit + ReportGenerator. |
| GitHub Actions / CI | [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml); [captura 05 — Actions](capturas/05-actions.png) | CI verde informado para las ramas de los changes. Los jobs cubren backend, frontend, Compose/API+k6 y `openspec validate --all --strict`. |
| Colección Postman | [`RutaAndina-Sprint2.postman_collection.json`](../../tests/postman/RutaAndina-Sprint2.postman_collection.json), [`README.md`](../../tests/postman/README.md), [`postman-sprint2.json`](postman-sprint2.json); [captura 01 — Newman](capturas/01-newman.png) | Newman: **9/9 requests, 19/19 assertions, 0 fallos**. |
| Carga de búsqueda / EQ-01 y EQ-08 | `tests/performance/search-load.js`, [`search-load-redis-enabled.json`](search-load-redis-enabled.json), [`search-load-redis-disabled.json`](search-load-redis-disabled.json), [`bd-y-cache.md`](bd-y-cache.md) | Registros reales del 2026-11-22: Redis activado/desactivado; tramo de aceptación 200 VUs con p95 37,34 ms / 117,99 ms, sin fallos; Redis activado alcanzó 99,998 % HIT en el tramo. Rampa exploratoria hasta 2.000 VUs, sin aplicar allí el umbral EQ-01. |
| Concurrencia de retención / EQ-02 | [`concurrencia-eq02.md`](concurrencia-eq02.md), [`concurrencia-eq02-summary.json`](concurrencia-eq02-summary.json), `tests/performance/seat-hold-concurrency.js` | Ejecución real: 50 VUs al mismo asiento, 1 HTTP 200, 49 HTTP 409, cero inesperadas; 51/51 checks aprobados, cero fallos HTTP, p95 32,72 ms. |
| Capturas para la presentación | [01 Newman](capturas/01-newman.png), [02 Swagger](capturas/02-swagger.png), [03 cobertura backend](capturas/03-cobertura-backend.png), [04 cobertura frontend](capturas/04-cobertura-frontend.png), [05 Actions](capturas/05-actions.png) | Capturas enlazadas desde sus actividades correspondientes. |

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
