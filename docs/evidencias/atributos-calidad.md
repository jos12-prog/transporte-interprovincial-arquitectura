# Evaluación de atributos de calidad — RutaAndina (Sprint 2)

Este documento resume cómo se evaluaron los atributos de calidad definidos en el Sprint 1 (`docs/01-analisis-de-sistema/08-escenarios-atributos-calidad.md`, escenarios EQ-01 a EQ-10) y qué se obtuvo. Todas las cifras son de ejecuciones reales en local (Windows + Docker Compose) del **2026-11-26**, salvo EQ-02, medido el 2026-11-22. Son mediciones de esta máquina, no garantías para otro hardware.

## 1. Atributos que evaluamos

| Atributo | Escenario | Qué evaluaremos | Métrica | Herramienta |
|---|---|---|---|---|
| Rendimiento | EQ-01 | Tiempo de respuesta de la búsqueda de viajes con 200 usuarios concurrentes | p95 y % de errores HTTP | k6 |
| Capacidad / escalabilidad de carga | EQ-01 (rampa), EQ-09 | Comportamiento al subir progresivamente hasta 2.000 usuarios virtuales | Solicitudes por segundo, p95 y % de errores | k6 |
| Concurrencia | EQ-02 | Varios usuarios reteniendo el mismo asiento a la vez | Retenciones exitosas por asiento (máx. 1) y respuestas 409 | k6 + PostgreSQL |
| Seguridad y aislamiento | EQ-03 | Que un usuario no opere ni vea datos de otra empresa o agencia | % de intentos no autorizados rechazados (401/403) | xUnit + Testcontainers, Postman |
| Disponibilidad | EQ-04 | Que la búsqueda siga funcionando si Redis se cae | % de errores con Redis detenido | k6 (Redis apagado) |
| Reserva temporal | EQ-05 | Retención de 10 min y liberación automática del asiento | Duración del hold y tiempo máximo de liberación | xUnit + Testcontainers |
| Caché | EQ-08 | Que las búsquedas repetidas se atiendan desde Redis | % de respuestas `X-Cache: HIT` | k6 |
| Fiabilidad | — | Respuestas correctas de los endpoints | % de solicitudes y aserciones exitosas | Postman/Newman, k6 |
| Funcionalidad | — | Flujos de la API: búsqueda, asientos, holds, login, administración y catálogo | Casos aprobados y fallidos | Postman/Newman, Jest API |
| Seguridad básica | — | Rechazo de accesos inválidos y abuso de login | Respuestas 401, 409 y 429 esperadas | Postman/Newman |
| Mantenibilidad | — | Organización por capas y cobertura de pruebas | Reglas de arquitectura; cobertura de líneas, ramas, funciones | NetArchTest, xUnit + coverlet, Jest |
| Pagos, notificaciones, auditoría | EQ-06, EQ-07, EQ-10 | Consistencia de pagos, envío asíncrono y trazabilidad | Ver Sprint 1 | Sprint 3 |

## 2. Criterios de aceptación

Los objetivos vienen de los escenarios del Sprint 1. Donde la referencia del curso es más exigente, se registra también.

| Métrica | Objetivo | Origen |
|---|---|---|
| p95 de la búsqueda con 200 VUs | ≤ 2 s (referencia del curso: < 500 ms) | EQ-01 |
| Errores HTTP en la prueba de carga | < 1 % (referencia: ≥ 95 % de solicitudes exitosas y 0 errores 5xx) | EQ-01 |
| Reservas activas por asiento | Máximo 1 en el 100 % de las pruebas | EQ-02 |
| Intentos no autorizados rechazados | 100 % | EQ-03 |
| Búsqueda con Redis caído | Sigue respondiendo sin errores | EQ-04 |
| Duración y liberación del hold | 10 min; liberación ≤ 30 s después de vencer | EQ-05 |
| HIT de caché en búsquedas repetidas | ≥ 80 % | EQ-08 |
| Casos funcionales de API (Postman) | 100 % aprobados | Curso |
| Cobertura de líneas | ≥ 80 % | Curso |

## 3. Herramientas

- **k6** (`tests/performance/`): `search-load.js` corre 45 s con 200 VUs (tramo de aceptación con umbrales) y luego sube hasta 2.000 VUs (tramo exploratorio). `seat-hold-concurrency.js` lanza 50 VUs contra el mismo asiento. Se exporta el resumen JSON y un reporte HTML con gráficos (`K6_WEB_DASHBOARD`).
- **Postman / Newman** (`tests/postman/`): 58 solicitudes con aserciones de estado, cuerpo y cabeceras, incluidos los casos de error.
- **Jest**: pruebas del frontend (React Testing Library) y pruebas de caja negra contra la API real (`tests/api/`).
- **xUnit + Testcontainers** (`tests/Unit`, `tests/Integration`): reglas de negocio y pruebas contra PostgreSQL y Redis reales en contenedores. **NetArchTest** (`tests/Architecture`) comprueba las capas.
- **GitHub Actions** (`.github/workflows/ci.yml`): ejecuta backend, frontend, API con Newman y k6, y `openspec validate` en cada push.

## 4. Resultados: objetivo frente a lo obtenido

| Escenario / métrica | Objetivo | Obtenido | Estado | Evidencia |
|---|---|---|---|---|
| **EQ-01** p95 con 200 VUs (Redis activo) | ≤ 2 s | **103,49 ms**; 149.502 solicitudes | ✅ Cumple (también < 500 ms) | [14a](capturas/14a-k6-umbrales-redis-activado.png), [JSON](search-load-redis-enabled-2026-11-26.json), [HTML](k6-busqueda-redis-activado.html) |
| **EQ-01** errores HTTP con 200 VUs | < 1 % | **0 %** | ✅ Cumple | [14a](capturas/14a-k6-umbrales-redis-activado.png) |
| **Capacidad** rampa hasta 2.000 VUs (Redis activo) | Sin umbral (exploratorio) | **764.317** solicitudes, **5.271 req/s**, p95 **337 ms**, **0 %** de errores | ✅ Sin errores; p95 < 500 ms | [13](capturas/13-k6-dashboard-redis-activado.png), [14b](capturas/14b-k6-resultados-redis-activado.png) |
| **EQ-08** HIT de caché (200 VUs) | ≥ 80 % | **99,86 %** (149.302 de 149.502) | ✅ Cumple | [14a](capturas/14a-k6-umbrales-redis-activado.png), [bd-y-cache.md](bd-y-cache.md) |
| **EQ-04** búsqueda con Redis detenido, 200 VUs | Sigue respondiendo | p95 **157,57 ms**, **0 %** de errores, 100 % MISS (va a PostgreSQL) | ✅ Cumple | [15a](capturas/15a-k6-umbrales-redis-desactivado.png), [JSON](search-load-redis-disabled-2026-11-26.json), [HTML](k6-busqueda-redis-desactivado.html) |
| **EQ-04** Redis detenido, rampa hasta 2.000 VUs | Sin umbral | 230.050 solicitudes, **1.586 req/s**, p95 **1,28 s**, **0 %** de errores | ⚠️ Sin errores, pero p95 > 500 ms | [15b](capturas/15b-k6-resultados-redis-desactivado.png) |
| **EQ-04** disponibilidad mensual 99,5 % | 99,5 % | No medible en local | ⏳ Requiere despliegue (Sprint 3) | — |
| **EQ-02** mismo asiento, 50 VUs simultáneos | Máx. 1 reserva activa | **1** HTTP 200 y **49** HTTP 409; 0 respuestas inesperadas; 51/51 checks | ✅ Cumple | [concurrencia-eq02.md](concurrencia-eq02.md), [JSON](concurrencia-eq02-summary.json) |
| **EQ-03** aislamiento entre empresas y agencias | 100 % rechazados | 100 % de los casos probados rechazados | ✅ Cumple en los casos probados | `AgencyAdminCanScheduleWithFareAndCancelWhileAgentIsForbidden`, `SaveRouteRejectsUnauthorizedAgencyScope`, `CatalogQueriesValidateScopeBeforeReturningCompanyAndAgencyData`; [03b](capturas/03b-pruebas-backend.png) |
| **EQ-05** hold de 10 min y liberación automática | 10 min; ≤ 30 s | Hold de **10 min**; el worker revisa cada **10 s**; liberación verificada, también con dos workers a la vez | ✅ Cumple | `ReservationCreatesHeldSeatsAndExpiresTenMinutesAfterCreation`, `ExpirationWorkerReleasesExpiredSeat`, `ConcurrentExpirationWorkersExpireAndReleaseEachHoldOnce` |
| **EQ-09** duplicar instancias del backend | Mantener EQ-01 | La rampa a 2.000 VUs corrió con **1 instancia** | ⏳ Parcial: falta comparar 1 vs 2 réplicas | Sprint 3 |
| **Fiabilidad / funcionalidad** Postman | 100 % aprobados | **58/58** solicitudes, **92/92** aserciones, 0 fallas, 21 ms de promedio | ✅ Cumple | [01](capturas/01-newman.png), [JSON](postman-agency-catalog.json) |
| **Seguridad básica** casos de error | Respuesta esperada | 401 sin sesión y con login incorrecto, 409 por vehículo ocupado y 429 tras superar el límite de intentos por IP | ✅ Cumple | [01](capturas/01-newman.png) |
| **Mantenibilidad** cobertura del backend | ≥ 80 % líneas | **81,3 %** líneas (5.021/6.169), **69,6 %** ramas; 85 unitarias, 2 de arquitectura y 17 de integración aprobadas | ✅ Líneas; ramas por debajo | [03](capturas/03-cobertura-backend.png), [03b](capturas/03b-pruebas-backend.png) |
| **Mantenibilidad** cobertura del frontend | ≥ 80 % líneas | **83,11 %** líneas, 75,22 % sentencias, 63,99 % ramas, 68,44 % funciones; **72/72** pruebas | ✅ Líneas; ramas por debajo | [04](capturas/04-cobertura-frontend.png), [04b](capturas/04b-cobertura-frontend-resumen.png) |
| **Integración continua** | Pipeline en verde | CI #24 de `main` en verde; el #21 falló y se corrigió en el #22 | ✅ Cumple | [05](capturas/05-actions.png) |
| **EQ-06** pagos, **EQ-07** WhatsApp, **EQ-10** auditoría | Ver Sprint 1 | No implementados en este sprint | ⏳ Sprint 3 | — |

### Comparación con y sin caché (misma máquina, mismo perfil)

| Métrica | Con Redis | Sin Redis | Mejora con Redis |
|---|---:|---:|---:|
| p95 con 200 VUs | 103,49 ms | 157,57 ms | 1,5× más rápido |
| p95 de toda la prueba (hasta 2.000 VUs) | 337 ms | 1,28 s | 3,8× más rápido |
| Solicitudes por segundo | 5.271 | 1.586 | 3,3× más capacidad |
| Errores HTTP | 0 % | 0 % | — |

## 5. Hallazgo durante la validación

En la primera ejecución del 2026-11-26, k6 marcó **EQ-08 en rojo**: 52 % de HIT con 200 VUs, cuando el 22/11 había dado 99,998 %. La causa estaba en `RedisTripSearchCache`. La "generación" de caché que agregó `add-platform-admin` se leía de Redis en cada búsqueda, así que cada búsqueda hacía dos operaciones en Redis. Con 200 usuarios a la vez bastaba un timeout de 300 ms para abrir el circuit breaker 30 s, y durante ese tiempo todas las búsquedas iban a PostgreSQL.

La corrección guarda la generación en memoria 2 s, y la API vuelve a hacer una sola operación en Redis por búsqueda. Dos pruebas unitarias nuevas la cubren. Al repetir k6 se obtuvo **99,86 % de HIT, p95 de 103 ms y 0 errores hasta 2.000 VUs**. El detalle está en [bd-y-cache.md](bd-y-cache.md#hallazgo-y-corrección).

Además, el circuit breaker evitó errores: aun con la caché desactivada, el tramo de 200 VUs terminó con 0 % de fallos.

## 6. Pendiente para el Sprint 3

- **EQ-09:** repetir la rampa con 1 y 2 réplicas de la API detrás de un balanceador y comparar.
- **EQ-04:** medir la disponibilidad real con el sistema desplegado.
- **EQ-06, EQ-07 y EQ-10:** implementar pagos con Culqi, notificaciones por WhatsApp con RabbitMQ y auditoría, y validarlos.
- **Cobertura de ramas** (69,6 % en backend y 64 % en frontend): subirla en los módulos de administración.

## 7. Cómo reproducir

Los comandos están en [README.md](README.md) y en `tests/performance/README.md`. Para la carga, antes de la corrida con Redis se recomienda calentar la API con unas búsquedas y esperar 35 s, para que el circuit breaker no arranque abierto.
