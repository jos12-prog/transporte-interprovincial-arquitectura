# Proposal

## Why

El repositorio ya define el stack y la arquitectura, pero todavía no tiene solución ejecutable, proyectos ni pipeline; esto impide validar el trabajo del Sprint 2 de manera repetible. Se establecerá una base compilable y verificable sin reglas de negocio, autenticación ni integraciones de pago o notificación.

## What Changes

- Crear el esqueleto de backend .NET 8 bajo `src/backend`: Host ASP.NET Core Web API, Shared y los módulos MI01–MI07, cada uno con Domain, Application, Infrastructure y Presentation, respetando Clean Architecture.
- Incorporar `GET /health`, que informa el estado de PostgreSQL, Redis y RabbitMQ; configurar Serilog, ProblemDetails y Swagger/OpenAPI con comentarios XML.
- Añadir proyectos .NET para Unit, Architecture e Integration con xUnit y coverlet; validar límites de dependencias con NetArchTest y probar `/health` usando Testcontainers.
- Crear pruebas de API en `tests/api` con Jest y TypeScript contra el host HTTP real; incorporar reporte de cobertura.
- Crear `src/frontend` con React 18, TypeScript, Vite, Tailwind, Jest y React Testing Library (sin Vitest), tokens de `design/stitch/DESIGN.md`, `lucide-react`, rutas vacías por rol, layout de pasajero y shell del personal con el logo existente.
- Añadir estructura de `tests/Concurrency` como preparación, sin pruebas de negocio: EQ-02 (concurrencia) y EQ-03 (aislamiento entre agencias) se difieren hasta que existan reglas de reserva y autorización; continuarán siendo obligatorias antes de completar el producto.
- Añadir k6 smoke test y guía de ejecución, `docker-compose.yml`, `.env.example`, completar el `.gitignore` existente, crear `.editorconfig` y `.gitattributes` con `eol=lf`, y workflow CI para push/PR, builds, pruebas, artefactos de cobertura y `openspec validate --all --strict`.
- Mantener fuera de alcance login/JWT, entidades y reglas de negocio, Culqi y WhatsApp.

## Capabilities

### New Capabilities

- `service-health`: endpoint de salud compuesto para PostgreSQL, Redis y RabbitMQ, documentado en OpenAPI y utilizable por probes e integración.

### Modified Capabilities

Ninguna. `domain-baseline` no cambia: esta propuesta no implementa comportamiento de negocio ni modifica sus requisitos.

## Impact

- **Módulos:** estructura inicial de MI01 Identidad y Acceso, MI02 Empresas y Agencias, MI03 Transporte y Programación, MI04 Reservas y Pasajes, MI05 Ventas y Pagos, MI06 Consultas y Reportes y MI07 Auditoría y Notificaciones; sin casos de uso de negocio.
- **API:** nuevo `GET /health` y contrato OpenAPI para health. No se agregan endpoints de dominio.
- **Pruebas y herramientas:** solución .NET 8, xUnit, NetArchTest, coverlet, Testcontainers, Jest, TypeScript, React Testing Library, k6, Docker Compose y GitHub Actions.
- **Fundamento documental:** RC15 y ADR-01/02/03 justifican monolito modular, Clean Architecture y API REST; RC04 y ADR-04/05 justifican PostgreSQL compartido y aislamiento modular; RC12 y ADR-09 justifican Redis; RC13 y ADR-10 justifican RabbitMQ; ADR-15 justifica logging/observabilidad. Las versiones .NET 8 y React 18 y el stack frontend están fijados además por `AGENTS.md` y `docs/02-arquitectura-software/tecnologias-y-despliegue.md`.
- **Calidad y límites:** la verificación se limita a health y arquitectura del esqueleto. No cubre HU/RF de negocio ni afirma cumplir aún EQ-02/EQ-03; estas pruebas quedan pendientes para changes posteriores de reservas y autorización.
