# Design

## Context

La propuesta y `specs/domain-baseline/spec.md` definen el motivo y el contrato conductual. El inventario de entrada abarca README y documentos de análisis, arquitectura, diseño interno y C4; no hay especificaciones OpenSpec previas ni código que cambiar en esta fase.

## Goals / Non-Goals

**Goals:**

- Mantener una única fuente documental coherente para IDs, actores, alcance, estados, inventario de asientos, aislamiento, pagos y calidad.
- Separar los requisitos observables de las decisiones técnicas que los implementan.
- Aplicar nombres y referencias canónicas de forma cruzada en todos los documentos enumerados.

**Non-Goals:**

- Implementar los componentes o modificar código, API o base de datos.
- Añadir funcionalidades ajenas a las decisiones de entrada.
- Resolver ahora políticas comerciales que están explícitamente pendientes de validar.

## Decisions

### 1. Catálogos de IDs sin colisión

`DA01–DA14` queda reservado exclusivamente para drivers. Las decisiones existentes conservan su orden y contenido bajo IDs `ADR-01–ADR-15`; toda referencia previa del tipo `DA-01` que aludía a una decisión se convierte en `ADR-01`. En el README se informan por separado los dos conteos.

| Antes | Después | Significado canónico |
|---|---|---|
| `DA01–DA14` (drivers) | `DA01–DA14` | Drivers; IDs y orden sin cambio |
| `DA-01–DA-15` (decisiones) | `ADR-01–ADR-15` | Decisiones arquitectónicas; mismo orden y decisión |
| DA01 driver | DA01 | Concurrencia de usuarios |
| DA02 driver | DA02 | Prevención de sobreventa |
| DA03 driver | DA03 | Consistencia de reservas, pagos, disponibilidad y pasajes |
| DA04 driver | DA04 | Aislamiento multiempresa/agencia |
| DA05 driver | DA05 | Escalabilidad |
| DA06 driver | DA06 | Rendimiento bajo demanda |
| DA07 driver | DA07 | Disponibilidad compartida web/presencial |
| DA08 driver | DA08 | Retención temporal |
| DA09 driver | DA09 | Integración de pagos |
| DA10 driver | DA10 | Notificaciones asíncronas |
| DA11 driver | DA11 | Consultas frecuentes y caché |
| DA12 driver | DA12 | Trazabilidad y auditoría |
| DA13 driver | DA13 | Integraciones externas |
| DA14 driver | DA14 | Mantenibilidad y evolución |

Las tablas de trazabilidad se rehacen como relaciones driver→ADR: DA01→ADR-01/07/14; DA02→ADR-07; DA03→ADR-04/07/08; DA04→ADR-05/06; DA05→ADR-01/09/10/14; DA06→ADR-09/14; DA07→ADR-04/07; DA08→ADR-08; DA09→ADR-11; DA10→ADR-10/12; DA11→ADR-09; DA12→ADR-13/15; DA13→ADR-03/11/12; DA14→ADR-01/02. Las referencias deben verificarse por significado en contexto y no hacerse mediante reemplazo ciego.

### 2. Acceso multiagencia y web responsive

El Administrador de Agencia administra una o varias agencias dentro de una sola empresa. La documentación de autenticación fija claims `sub`, `role`, `company_id`, `agency_ids[]`; el cliente envía la agencia seleccionada en `X-Agency-Id` y el backend valida autorización y pertenencia a la empresa en el caso de uso. La experiencia de v1 es una sola aplicación React web responsive ejecutada en navegador de escritorio o móvil; no se implica app móvil nativa.

### 3. Inventario de asientos numerados

La modalidad de cupos abiertos se elimina. La documentación define `VehicleModel` como catálogo administrado por Superadministrador (nombre, categoría, capacidad y `SeatLayout` con conductor y asientos numerados, fila, columna y piso); los modelos iniciales son Toyota Yaris (4), Toyota Avanza (6), Minivan (7), Van (15), Bus 1 piso y Bus 2 pisos. El Administrador registra `Vehicle` por placa y elige modelo por nombre. La creación del viaje genera `TripSeat` desde el plano. Una reserva puede asociar varios asientos con pasajero (DNI y nombre). Pasajero y agente ven el mismo mapa y sus estados disponible, ocupado/retenido y seleccionado.

### 4. Estados, reservas y canal de venta

Se documentan tres máquinas separadas: reserva `HELD`, `CONFIRMED`, `CANCELLED`, `EXPIRED`, `RESCHEDULED`; pago `PENDING`, `APPROVED`, `REJECTED`, `ERROR`, `REFUNDED`; pasaje `ISSUED`, `USED`, `VOIDED`. Solo las reservas digitales usan `HELD` por 10 minutos. El agente registra el pago presencial (efectivo, Yape/Plin o POS) y confirma directamente; no se agrega cierre de caja. Reglas de cancelación/reprogramación quedan pendientes de validar.

### 5. Pagos, idempotencia y despacho

El frontend obtiene un token con Culqi Checkout; el backend crea el cargo y valida la respuesta servidor a servidor. Webhooks firmados HMAC reconcilian el resultado; gana el primer evento válido y el `charge_id` único evita duplicación. Si llega aprobación después de expirar la reserva, se confirma solo si siguen libres todos los asientos; si no, el estado del pago pasa a `REFUNDED`. Los eventos de notificación usan Transactional Outbox + RabbitMQ y nunca una llamada síncrona del caso de uso transaccional.

### 6. Persistencia, concurrencia y caché

Se conserva una base PostgreSQL. Los límites de propiedad son por módulo (un esquema y un `DbContext` por módulo; interacción intermodular mediante contratos públicos) y el aislamiento tenant usa `company_id`, `agency_id`, Global Query Filters y validación en el caso de uso. La asignación bloquea `TripSeat` con `SELECT FOR UPDATE`; un índice único parcial cubre `trip_seat_id` mientras el estado sea `HELD` o `CONFIRMED`. Un `BackgroundService` cada 10 segundos expira retenciones con `FOR UPDATE SKIP LOCKED`, permitiendo múltiples instancias. Redis almacena solo búsquedas de viajes y catálogos con TTL de 60 s; mapa exacto de asientos y reservas consultan PostgreSQL. La réplica de lectura queda fuera de v1.

### 7. Nombres canónicos antes→después

| Antes / variantes | Después |
|---|---|
| `IPaymentPort` | `IPaymentGateway` (interfaz, Aplicación) implementada por `CulqiPaymentAdapter` (Infraestructura) |
| `INotificationService`, `INotificationPort` | `INotificationGateway` (interfaz) implementada por `WhatsAppNotificationAdapter` |
| `WhatsAppAdapter` | `WhatsAppNotificationAdapter` |
| `PostgresReservationRepository` | `IReservationRepository` (interfaz) implementada por `ReservationRepository` (EF Core) |
| `IAvailabilityService`, `IAvailabilityStrategy` | `ISeatAvailabilityService` (interfaz) |
| `QuotaSeatStrategy`, `CapacityQuotaStrategy` | Eliminados |
| `CreateReservation`, `ConfirmReservation`, nombres de caso sin sufijo | `CreateReservationUseCase`, `ConfirmReservationUseCase`, `CancelReservationUseCase`, `RescheduleReservationUseCase`, `IssueTicketUseCase`, `ProcessPaymentUseCase` |
| Métodos de dominio `Crear`, `MarcarConfirmada`, `Cancelar` | `Create`, `Confirm`, `Cancel`, `Expire`, `Reschedule` |

`IPaymentGateway`, `INotificationGateway`, `IReservationRepository` e `ISeatAvailabilityService` se definen en Aplicación; sus adaptadores y repositorios concretos pertenecen a Infraestructura. **Toda integración externa y persistencia se accede mediante interfaz en Aplicación e implementación en Infraestructura.**

Strategy (PD03) deja de escoger modalidad de disponibilidad y pasa a `ICancellationPolicy` para políticas de cancelación/reprogramación según anticipación. Sus reglas quedan pendientes de validar. Se elimina la duplicación de RG08 en principios de diseño.

### 8. Alcance y reglas pendientes

Choferes, réplica de lectura y aplicación móvil nativa se etiquetan como evolución futura o se retiran de las descripciones de v1. El Superadministrador no registra ventas presenciales. HU30 se traza al nuevo RF49, alerta interna al Administrador de Agencia. Requisitos funcionales incluirá una sección de validación pendiente para las políticas de cancelación/reprogramación, comprobante SUNAT (fuera de alcance v1; pasaje PDF con QR) y consentimiento WhatsApp: alertas a pasajeros solo con opt-in y cuando la ocupación supere 70 %.

### 9. Objetivos y tácticas de calidad

AC01 y EQ-01 usarán 200 usuarios concurrentes con P95 ≤ 2 s. La táctica 12 será “Interfaz por rol y sistema de diseño consistente”: navegación según rol, mapa visual de asientos y guía UI indicada en `design/stitch/DESIGN.md`; se agrega AC10 al diagrama y matriz. No se crean artefactos UI en esta propuesta.

## Risks / Trade-offs

- [La renumeración de las decisiones puede dejar referencias antiguas o confundir DA de driver con ADR] → Actualizar documento por documento, buscar el patrón obsoleto en todo el corpus y validar las matrices por significado.
- [Los nuevos estados afectan varios diagramas y ejemplos] → Aplicar las tres máquinas como vocabulario canónico y revisar tablas, prosa, pseudocódigo y diagramas juntos.
- [El modelo multi-asiento puede contradecir ejemplos de una sola plaza] → Actualizar los ejemplos de dominio y secuencia para que una reserva agrupe asientos y pasajeros.
- [Políticas de cancelación no tienen reglas comerciales confirmadas] → Documentar el contrato Strategy como pendiente de validación, sin inventar plazos o penalidades.
- [Las decisiones de esta propuesta todavía no están implementadas] → Marcar las tareas como actualización documental y no afirmar que los componentes ya existen en ejecución.

## Migration Plan

1. Actualizar primero catálogos base y requisitos (`01-analisis-de-sistema`) y fijar su glosario.
2. Alinear después decisiones, diseño interno, tácticas, tecnologías y diagramas C4 con esos requisitos.
3. Actualizar README al final con conteos y alcance reconciliados.
4. Verificar referencias, vocabulario prohibido, máquinas de estado y trazabilidad completa. No hay migración de código o datos en esta fase.

## Open Questions

- Las políticas comerciales concretas de cancelación y reprogramación permanecen pendientes de validación por decisión de entrada; esta propuesta solo fija el contrato `ICancellationPolicy`.
