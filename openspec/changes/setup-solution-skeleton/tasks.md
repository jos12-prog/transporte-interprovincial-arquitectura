# Tasks

## 1. Domain

- [x] 1.1 Crear `src/backend/TransportPlatform.sln` y los siete proyectos Domain (`MI01`–`MI07`) en `net8.0`, sin referencias de proyecto ni paquetes externos; verificar `dotnet build src/backend/TransportPlatform.sln`.

## 2. Application

- [x] 2.1 Crear los siete proyectos Application y referenciar únicamente el Domain de su propio módulo; verificar que la solución restaure y compile sin referencias cruzadas entre módulos.

## 3. Infrastructure

- [x] 3.1 Crear Infrastructure por módulo con referencia a Domain/Application del mismo módulo y un DbContext vacío por módulo configurado con su esquema asignado; verificar que EF Core/Npgsql compile y que Domain no herede dependencias de persistencia.
- [x] 3.2 Crear `Shared.Infrastructure` para health, ProblemDetails y logging; registrar comprobaciones acotadas de PostgreSQL, Redis y RabbitMQ usando configuración por variables de entorno y sin incluir secretos en respuestas o logs; verificar compilación.

## 4. Presentation y Host

- [x] 4.1 Crear los siete proyectos Presentation con referencias solo a Application del mismo módulo y sin lógica de negocio; verificar compilación de la solución.
- [x] 4.2 Crear el Host ASP.NET Core 8 como composition root, registrando módulos e infraestructura, `GET /health` con 200/503 conforme a la spec, ProblemDetails, Serilog y Swagger/OpenAPI con XML docs; verificar respuestas de salud y que OpenAPI describa `/health`.
- [x] 4.3 Completar `.gitignore` existente para secretos, binarios intermedios y coberturas, y crear `.env.example`, `.editorconfig` y `.gitattributes` con `eol=lf`; verificar que `.env` se ignore y `.env.example` no contenga secretos.

## 5. Frontend

- [x] 5.1 Crear `src/frontend` con React 18, TypeScript, Vite y Tailwind CSS compatible con `tailwind.config.ts`; trasladar los tokens de `design/stitch/DESIGN.md` a la configuración y añadir `lucide-react`; verificar `npm ci` y `npm run build`.
- [x] 5.2 Crear rutas vacías para pasajero, agente, administrador de agencia y superadministrador, layout de pasajero y shell compartido del personal con logo de `design/stitch/_assets/brand/`; verificar renderizado de cada ruta sin login ni guards.
- [x] 5.3 Configurar Jest + React Testing Library (sin Vitest), cobertura HTML/LCOV/Cobertura y pruebas mínimas de rutas/layout; verificar que `npm test -- --coverage` pase y genere reportes.

## 6. Tests y entorno local

- [ ] 6.1 Crear `docker-compose.yml` con `postgres:16`, `redis:7`, `rabbitmq:3-management`, `api` y `frontend`, healthchecks, puertos y configuración desde `.env`; verificar `docker compose config` y que el stack alcance saludable en `/health`.
- [ ] 6.2 Crear `tests/Unit` con xUnit y coverlet para helpers de infraestructura/configuración sin reglas de negocio; configurar ReportGenerator y verificar con `dotnet test` salidas Cobertura y HTML.
- [x] 6.3 Crear `tests/Architecture` con xUnit y NetArchTest para asegurar Domain sin dependencias de infraestructura, Application sin Infrastructure/Presentation y ausencia de referencias directas entre módulos; verificar que las reglas pasan sobre los ensamblados construidos.
- [ ] 6.4 Crear `tests/Integration` con xUnit, `Microsoft.AspNetCore.Mvc.Testing` y Testcontainers para PostgreSQL, Redis y RabbitMQ; probar `/health` saludable y degradado y verificar códigos 200/503 sin filtrar secretos.
- [ ] 6.5 Crear `tests/api` como paquete Jest + TypeScript que use `API_BASE_URL` para llamar por HTTP al Host real en `/health`; verificar los checks de componentes y reportes Cobertura/HTML con los servicios levantados.
- [ ] 6.6 Crear `tests/Concurrency/README.md` que registre EQ-02 y EQ-03 como obligatorios pero diferidos hasta que existan reglas de reserva y autorización; verificar que no se afirme que esos escenarios están probados.
- [ ] 6.7 Crear `tests/performance/smoke-health.js` para k6 y un README con instrucciones para levantar Compose, configurar `BASE_URL` y ejecutar el smoke test; verificar que una API saludable pase y una respuesta no exitosa falle.

## 7. CI e integración final

- [ ] 7.1 Crear `.github/workflows/ci.yml` para push y pull request: restaurar, compilar y probar .NET; ejecutar Jest de frontend; levantar Compose y ejecutar Jest de API contra HTTP real; subir Cobertura/HTML de backend, frontend y API incluso ante fallos; ejecutar `openspec validate --all --strict`; verificar que el workflow analice y use esos eventos y jobs.
- [ ] 7.2 Verificar el conjunto integrado con `dotnet build`, `dotnet test`, `npm run build`, pruebas Jest de frontend y API, `docker compose up`/`down` y `openspec validate --all --strict`; confirmar que el change no introduce login/JWT, reglas de negocio, Culqi ni WhatsApp.
