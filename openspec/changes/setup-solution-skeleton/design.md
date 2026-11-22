# Design

## Context

See `proposal.md` for the motivation and scope. El repositorio contiene documentación y prototipos visuales, pero no hay `src/`, `tests/` ni `.github/`; el change inicia esos árboles sin reemplazar la fuente funcional `domain-baseline`. Las interfaces y nombres de módulos siguen `AGENTS.md`; la experiencia visual toma tokens y pantallas de `design/stitch/`.

## Goals / Non-Goals

**Goals:**

- Que el repositorio compile y pueda levantar localmente el host, PostgreSQL, Redis, RabbitMQ y frontend mediante Compose.
- Hacer comprobables la dirección de dependencias, la disponibilidad del host y la cobertura de pruebas en CI.
- Dejar las siete fronteras de módulo listas para incorporar comportamiento en changes posteriores.

**Non-Goals:**

- Crear entidades, reglas, casos de uso o endpoints de reservas, pagos, usuarios o administración.
- Implementar login/JWT, autorización funcional, Culqi, WhatsApp, Outbox o mensajería de negocio.
- Dar por cubiertas EQ-02 y EQ-03: la carpeta/proyecto de concurrencia es solo scaffolding; sus escenarios se implementarán cuando existan reserva y autorización.
- Crear migraciones con tablas de negocio, ambientes de producción o automatización de despliegue.

## Decisions

### 1. Solución .NET por módulos y capas

Todos los proyectos apuntan a `net8.0` y se incluyen en `src/backend/TransportPlatform.sln`. `Host` es el composition root y contiene `Program.cs`, configuración HTTP, OpenAPI y registro de dependencias. `Shared/TransportPlatform.Shared.Infrastructure` concentra solo infraestructura transversal requerida por Host (ProblemDetails, health y logging); no aloja reglas ni entidades compartidas.

Cada MI01–MI07 tendrá proyectos class library `Domain`, `Application`, `Infrastructure` y `Presentation`. La dirección permitida es:

| Proyecto | Referencias permitidas |
|---|---|
| Domain | BCL; ningún otro proyecto ni framework/librería de infraestructura |
| Application | Domain del mismo módulo y sus contratos de aplicación |
| Infrastructure | Application y Domain del mismo módulo; EF Core/Npgsql y clientes técnicos cuando se necesiten |
| Presentation | Application del mismo módulo |
| Host | Presentation e Infrastructure de módulos para composición; Shared.Infrastructure |

Los módulos no se referencian directamente entre sí. Cuando un caso de uso futuro requiera comunicación intermodular, el proveedor publicará `I<Modulo>Api` en Application y el consumidor dependerá únicamente de ese contrato público. En este esqueleto no se inventan operaciones vacías.

Se crearán contextos EF Core vacíos por módulo en Infrastructure, con nombres de esquema estables: `identity_access`, `companies_agencies`, `transport_scheduling`, `reservations`, `sales_payments`, `queries_reports` y `audit_notifications`. No tendrán `DbSet`, entidades ni migraciones hasta que otro change modele esas estructuras. La preparación cumple la regla modular de `AGENTS.md` sin anticipar el modelo de negocio. PostgreSQL 16 compartido y contextos/esquemas por módulo corresponden a RC04, ADR-04 y ADR-05.

### 2. Host, salud y API documentada

Host registra comprobaciones de conectividad de PostgreSQL, Redis y RabbitMQ y expone un único `GET /health`, sin requerir login. Responde 200 solo si los tres checks están saludables; ante cualquier fallo responde 503. El payload enumera el componente y su estado, sin connection strings, credenciales ni mensajes internos. Esto es readiness para Compose, probes y pruebas, no una API de negocio. PostgreSQL, Redis y RabbitMQ son las dependencias seleccionadas por ADR-04/09/10 y RC04/12/13.

El registro de health checks usa clientes configurados desde variables de entorno, con timeouts acotados para que una dependencia caída no bloquee el host. Compose declara `healthcheck` y `depends_on: condition: service_healthy` donde aplique. El contrato y los dos códigos de respuesta quedan descritos en OpenAPI.

Swagger UI y `swagger/v1/swagger.json` se habilitan para desarrollo; `GET /health` siempre permanece disponible para probes. El host genera XML docs y Swashbuckle los incluye en OpenAPI. ASP.NET Core ProblemDetails y el exception handler unifican errores no-health; Serilog emite logs estructurados a consola y omite secretos y datos sensibles. La API REST sigue ADR-03/RC15 y observabilidad/logging sigue ADR-15.

### 3. Esqueleto del frontend

`src/frontend` es un proyecto independiente React 18 + TypeScript + Vite. Se usará Tailwind CSS con `tailwind.config.ts` que traduce los tokens de color, tipografía, espaciado y radios de `design/stitch/DESIGN.md`; se elige Tailwind 3.x para que la configuración solicitada sea la fuente activa de tokens. Los íconos usan `lucide-react`.

React Router proporciona rutas vacías por rol: `/pasajero/*`, `/agente/*`, `/admin-agencia/*` y `/superadmin/*`. El layout de pasajero es una estructura vacía de navegación/contenido; el shell del personal comparte sidebar y barra superior. No hay resolución real de rol, guards ni login. El shell importa `logo-horizontal.svg` desde `design/stitch/_assets/brand/` mediante alias Vite `@brand`; la configuración permite ese asset de solo lectura sin duplicar ni alterar el archivo fuente.

Jest usa `ts-jest` y `jest-environment-jsdom` con React Testing Library y `jest-dom`; los comandos generan cobertura LCOV, Cobertura e HTML. No se configura ni ejecuta Vitest. La decisión de Jest aplica tanto a UI como a `tests/api` para cumplir el stack del Sprint 2.

### 4. Pruebas y reportes

La solución de tests se divide por responsabilidad:

- `tests/Unit`: xUnit para helpers/configuración aislados, sin reglas de dominio.
- `tests/Architecture`: xUnit + NetArchTest verifica Domain sin dependencias externas, Application sin Infrastructure/Presentation y referencias limitadas al módulo. Host es la excepción explícita como composition root.
- `tests/Integration`: xUnit, `Microsoft.AspNetCore.Mvc.Testing` y Testcontainers; inicia PostgreSQL, Redis y RabbitMQ para que la prueba del host observe el estado saludable completo en `/health`. Así se prueba PostgreSQL como pide Sprint 2 sin esperar HTTP 200 si las otras dependencias no están disponibles.
- `tests/Concurrency`: proyecto/README preparatorio con referencias EQ-02 y EQ-03; no implementa tests aplazados ni simula reglas inexistentes.
- `tests/api`: paquete Jest/TypeScript separado que llama por HTTP al host real usando `API_BASE_URL`; no reemplaza la integración con mocks.

`coverlet.collector` emite XML Cobertura en los proyectos .NET; ReportGenerator deriva HTML. Jest produce LCOV/Cobertura/HTML en frontend y API. El workflow sube los tres grupos de reportes aunque falle una prueba, para conservar evidencia diagnóstica.

### 5. Compose, performance y configuración del repositorio

`docker-compose.yml` declara `postgres:16`, `redis:7`, `rabbitmq:3-management`, `api` y `frontend`. API usa la imagen ASP.NET Core 8; frontend sirve el build SPA mediante Nginx con fallback de rutas. Los servicios esperan dependencias saludables. `tests/performance/smoke-health.js` configura `BASE_URL`, solicita `/health` y comprueba estado HTTP exitoso; su README explica instalar/ejecutar k6 y levantar Compose.

`.env.example` solo contiene valores de desarrollo no sensibles; `.env` se excluye en `.gitignore`. `.editorconfig` define formato compartido y `.gitattributes` fuerza `text=auto eol=lf`. Las imágenes existentes se reutilizan sin modificaciones.

### 6. CI para cada push y pull request

GitHub Actions tendrá permisos mínimos de lectura y jobs para:

1. Restaurar, compilar y probar todos los proyectos .NET; Docker del runner permite los tests Testcontainers. Publica Cobertura/HTML .NET.
2. Instalar dependencias frontend con lockfile, ejecutar `npm run build` y Jest + RTL con cobertura; publica sus reportes.
3. Levantar el stack de Compose, esperar `/health` saludable, ejecutar Jest de `tests/api` contra `API_BASE_URL` real y publicar su cobertura; baja Compose al terminar incluso si falla.
4. Ejecutar `openspec validate --all --strict`.

Las tareas de CI no necesitan secretos externos. En el stack local se usan credenciales de ejemplo tomadas de `.env`; no se versionan credenciales reales. Los tests de EQ-02/EQ-03 no forman parte de estos checks hasta que un change de negocio los implemente.

## Risks / Trade-offs

- [El health agregado será 503 si falta un contenedor auxiliar] → Compose espera dependencias saludables y las pruebas arrancan PostgreSQL, Redis y RabbitMQ; el payload permite identificar el check fallido.
- [Scaffolding de siete módulos puede crear proyectos vacíos y repetitivos] → Mantenerlos como class libraries sin tipos de negocio y probar sus límites desde NetArchTest.
- [Los tests API dependen de que Compose esté listo] → Usar healthchecks, reintentos acotados y `docker compose down` en un paso `always()`.
- [EQ-02/EQ-03 quedan pendientes aunque `AGENTS.md` los hace obligatorios para completar el producto] → No marcarlos cumplidos en esta fase y bloquear la declaración de producto terminado hasta implementarlos en changes posteriores.
- [El logo fuente está fuera de `src/frontend`] → Alias Vite apuntará al asset existente y configurará el acceso del dev server al root del repositorio; no se editará el asset.

## Migration Plan

No hay migración de datos ni reemplazo de sistemas existentes. Crear el esqueleto, levantarlo con Compose, ejecutar los checks y reportes; si se requiere rollback, retirar los proyectos y configuración introducidos por este change. El spec `service-health` solo describe el endpoint de salud incorporado.

## Open Questions

Ninguna para el esqueleto. EQ-02 y EQ-03 permanecen como trabajo posterior, no como decisiones abiertas de esta propuesta.
