\# AGENTS.md — Reglas para Codex



\## Flujo obligatorio (SDD con OpenSpec)

\- Nunca escribir código sin un change de OpenSpec aprobado: propose → revisión humana → apply → archive.

\- Fuente de verdad: `openspec/specs/` (domain-baseline) y `docs/`. Si algo no está especificado, preguntar; no inventar reglas de negocio.

\- Implementar solo lo que indica `tasks.md` del change activo.



\## Estructura del repositorio

\- `src/backend/` — solución .NET 8: `Host/`, `Modules/<Modulo>/{Domain,Application,Infrastructure,Presentation}`, `Shared/`.

\- `src/frontend/` — React 18 + TypeScript + Vite + Tailwind.

\- `tests/` — `Unit/`, `Integration/` (Testcontainers con PostgreSQL), `Concurrency/`.

\- `design/stitch/` — pantallas de referencia (`screen.png` + `code.html`) y `DESIGN.md`.

\- `docker-compose.yml` — PostgreSQL, Redis, RabbitMQ, API y frontend.



\## Arquitectura (no negociable)

\- Monolito modular, 7 módulos MI01–MI07. Un esquema PostgreSQL y un DbContext por módulo.

\- Entre módulos solo contratos públicos (`I<Modulo>Api`); prohibido consultar tablas de otro módulo.

\- Domain no referencia EF Core, ASP.NET ni librerías externas. Los controladores no contienen reglas de negocio.

\- Toda entidad operativa lleva `CompanyId` y, si corresponde, `AgencyId`. Global Query Filters + validación de ámbito en el caso de uso.

\- JWT con `sub`, `role`, `company\_id`, `agency\_ids\[]`. La agencia activa llega en el header `X-Agency-Id` y se valida en backend.



\## Nombres canónicos

\- `IPaymentGateway` → `CulqiPaymentAdapter`; `INotificationGateway` → `WhatsAppNotificationAdapter`.

\- `IReservationRepository` → `ReservationRepository`; `ISeatAvailabilityService`; `ICancellationPolicy`.

\- Casos de uso con sufijo `UseCase` (`CreateReservationUseCase`, `ConfirmReservationUseCase`, …). Métodos de dominio en inglés.

\- Prohibidos: `IPaymentPort`, `INotificationPort`, `INotificationService`, estrategias de cupos.



\## Reglas críticas de negocio

\- Solo asientos numerados: `VehicleModel` (plano) → `Vehicle` (placa) → `Trip` → `TripSeat`.

\- Retener asientos: transacción corta con `SELECT ... FOR UPDATE` sobre `TripSeat` + índice único parcial `WHERE status IN ('HELD','CONFIRMED')`.

\- HELD dura 10 minutos (solo canal web). La expiración usa un BackgroundService cada 10 s con `FOR UPDATE SKIP LOCKED`.

\- Venta en ventanilla: directo a CONFIRMED con pago presencial registrado.

\- Pagos Culqi idempotentes por `charge\_id` único; el webhook se verifica con HMAC.

\- Notificaciones solo por Transactional Outbox + RabbitMQ; nunca llamadas síncronas a WhatsApp en un caso de uso transaccional.

\- Redis solo para búsqueda y catálogos (TTL 60 s). Nunca cachear el mapa de asientos ni usar Redis para decidir una reserva.

\- Toda operación crítica registra auditoría (usuario, rol, empresa, agencia, operación, fecha, recurso, resultado).



\## Calidad

\- Cada caso de uso con pruebas unitarias; cada endpoint con prueba de integración.

\- Pruebas obligatorias: concurrencia sobre el mismo asiento (EQ-02) y acceso cruzado entre agencias (EQ-03).

\- Antes de terminar: `dotnet build`, `dotnet test`, `npm run build` y `openspec validate --all --strict`.

\- Secretos solo en variables de entorno (`.env` fuera de git).



\## Frontend

\- Seguir `design/stitch/DESIGN.md` y la pantalla correspondiente en `design/stitch/<rol>/<NN-pantalla>/`.

\- `code.html` es referencia: convertirlo a componentes React reutilizables, no copiarlo literal.

