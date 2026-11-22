# Spec Delta

## Purpose

Define la línea base funcional de la plataforma interprovincial para que sus documentos arquitectónicos describan el mismo comportamiento de reservas, pagos, pasajes, inventario de asientos, acceso multiempresa e integraciones.

## ADDED Requirements

### Requirement: Separación de drivers y decisiones
La documentación SHALL identificar los drivers como `DA01–DA14` y las decisiones arquitectónicas como `ADR-01–ADR-15`, preservando la trazabilidad entre ambos catálogos.

#### Scenario: Consultar una referencia de arquitectura
- **WHEN** un documento cite un driver o una decisión
- **THEN** el prefijo y el identificador SHALL señalar inequívocamente a su catálogo y SHALL corresponder a la definición canónica.

### Requirement: Acceso administrativo por empresa y agencias autorizadas
Un Administrador de Agencia SHALL poder operar sobre una o varias agencias autorizadas de una sola empresa; el backend SHALL validar el rol, la empresa y la agencia activa para cada operación.

#### Scenario: Administrador cambia de agencia autorizada
- **WHEN** un administrador selecciona una agencia perteneciente a su empresa y a su conjunto autorizado
- **THEN** podrá operar en esa agencia mientras el backend rechaza agencias fuera de su ámbito.

### Requirement: Canal de atención web responsive
La plataforma SHALL ofrecer una aplicación web responsive para navegadores de escritorio y móviles; la versión inicial SHALL NOT requerir una aplicación móvil nativa.

#### Scenario: Pasajero usa navegador móvil
- **WHEN** un pasajero abre la aplicación web desde un navegador móvil
- **THEN** podrá completar los flujos disponibles en la interfaz responsive sin instalar una app nativa.

### Requirement: Inventario de vehículos con asientos numerados
La plataforma SHALL modelar vehículos mediante modelos con capacidad y plano de asientos numerados, y SHALL NOT tratar cupos abiertos como modalidad de asignación en la versión inicial.

#### Scenario: Crear un viaje desde un vehículo
- **WHEN** se crea un viaje asociado a un vehículo registrado desde un modelo
- **THEN** el viaje tendrá asientos individuales derivados del plano del modelo, incluido piso, fila y columna cuando corresponda.

### Requirement: Selección y reserva de asientos por pasajero
Pasajeros y agentes SHALL consultar el mismo mapa de asientos; una reserva SHALL permitir seleccionar uno o varios asientos y asociar a cada asiento los datos del pasajero requeridos.

#### Scenario: Consultar y reservar varios asientos
- **WHEN** un pasajero o agente consulta un viaje y selecciona asientos disponibles
- **THEN** verá los estados disponible, retenido/ocupado y seleccionado, y el sistema asociará cada asiento reservado con sus datos de pasajero.

### Requirement: Ciclo de vida de reservas, pagos y pasajes
El sistema SHALL mantener separados los estados de reserva (`HELD`, `CONFIRMED`, `CANCELLED`, `EXPIRED`, `RESCHEDULED`), pago (`PENDING`, `APPROVED`, `REJECTED`, `ERROR`, `REFUNDED`) y pasaje (`ISSUED`, `USED`, `VOIDED`).

#### Scenario: Cambio de estado en cada agregado
- **WHEN** una reserva, pago o pasaje cambia durante su ciclo de vida
- **THEN** el cambio SHALL usar únicamente un estado válido de su propia máquina y SHALL conservar coherencia entre los tres registros.

### Requirement: Retención digital y venta presencial
Una reserva iniciada por el canal digital SHALL retener asientos en `HELD` durante 10 minutos; una venta de agente con pago presencial SHALL confirmar directamente los asientos y registrar el medio de pago.

#### Scenario: Expira una retención digital
- **WHEN** no se valida el pago dentro de los 10 minutos de una reserva digital
- **THEN** la reserva pasa a `EXPIRED` y los asientos se liberan.

#### Scenario: Agente registra una venta presencial
- **WHEN** un agente registra una venta y un pago presencial mediante efectivo, Yape/Plin o POS
- **THEN** los asientos se confirman sin crear una retención digital y el sistema registra el medio de pago sin exigir cierre de caja.

### Requirement: Confirmación idempotente de pagos Culqi
El pago electrónico SHALL usar el token obtenido en Culqi Checkout, crear el cargo desde el backend y reconciliar la respuesta del servidor con el webhook firmado; la primera notificación válida SHALL ser autoritativa y el procesamiento SHALL ser idempotente por `charge_id`.

#### Scenario: Respuesta y webhook repetidos
- **WHEN** la respuesta del cargo y uno o más webhooks válidos llegan en cualquier orden para el mismo `charge_id`
- **THEN** el pago y la reserva tendrán un único resultado aplicado y no se duplicarán confirmaciones, pasajes ni efectos secundarios.

#### Scenario: Pago aprobado después de expirar la reserva
- **WHEN** se confirma un cargo para una reserva expirada
- **THEN** el sistema confirma la reserva solo si los asientos aún están libres; de lo contrario, registra el pago como `REFUNDED`.

### Requirement: Notificaciones desacopladas de transacciones
La confirmación de reserva o pago SHALL persistir el evento de notificación junto con la transacción principal y SHALL enviar la comunicación de forma asíncrona mediante la cola.

#### Scenario: Proveedor de mensajería no disponible
- **WHEN** WhatsApp no está disponible al confirmar una operación
- **THEN** la operación principal permanece confirmada y la notificación queda pendiente de reintento sin duplicar el evento.

### Requirement: Disponibilidad segura bajo concurrencia
El sistema SHALL impedir que un asiento de un viaje figure simultáneamente en más de una reserva activa `HELD` o `CONFIRMED`, incluso con solicitudes concurrentes o varias instancias de aplicación.

#### Scenario: Dos operaciones reclaman el mismo asiento
- **WHEN** dos pasajeros o un pasajero y un agente intentan reservar el mismo asiento simultáneamente
- **THEN** como máximo una operación obtiene el asiento y las restantes reciben un resultado de indisponibilidad sin sobreventa.

#### Scenario: Varias instancias expiran retenciones
- **WHEN** varias instancias procesan retenciones vencidas en paralelo
- **THEN** cada asiento retenido se libera una sola vez y no se altera una reserva que se haya confirmado válidamente.

### Requirement: Aislamiento por tenant y propiedad de datos modular
La plataforma SHALL aislar los datos por empresa y agencia, SHALL impedir el acceso cruzado no autorizado y SHALL respetar los límites de propiedad de datos de sus módulos.

#### Scenario: Usuario solicita datos de otro tenant
- **WHEN** un usuario consulta o modifica recursos de una empresa o agencia no autorizada
- **THEN** el backend rechaza la operación y no revela los datos solicitados.

### Requirement: Objetivos de calidad unificados
El objetivo de rendimiento SHALL evaluar al menos 200 usuarios concurrentes con un percentil 95 de respuesta de búsqueda y disponibilidad no superior a 2 segundos; la usabilidad SHALL cubrir flujos diferenciados por rol y la selección visual de asientos.

#### Scenario: Evaluar rendimiento de búsqueda
- **WHEN** la prueba de carga ejecuta búsquedas y consultas de disponibilidad con al menos 200 usuarios concurrentes
- **THEN** el 95 % de las respuestas cumple el límite de 2 segundos definido para AC01 y EQ-01.

### Requirement: Búsqueda, catálogos y alertas de demanda
La caché SHALL limitarse a búsquedas de viajes y catálogos con expiración de 60 segundos; mapa de asientos y operaciones de reserva SHALL consultar la fuente transaccional. Las alertas internas de alta demanda SHALL informar al Administrador de Agencia y las alertas a pasajeros SHALL requerir consentimiento previo.

#### Scenario: Consultar mapa o reservar
- **WHEN** el usuario consulta asientos o inicia una reserva
- **THEN** el sistema obtiene el estado vigente desde PostgreSQL y no confía en una copia cacheada de asientos.

#### Scenario: Enviar alerta de alta demanda
- **WHEN** la ocupación supera el 70 % y existe un evento de demanda aplicable
- **THEN** el sistema genera una alerta interna para el Administrador de Agencia y solo notifica a pasajeros que hayan aceptado ese tipo de comunicación.

### Requirement: Límites de alcance y validaciones pendientes
La documentación SHALL marcar como evolución futura los choferes, las réplicas de lectura y la app móvil nativa; SHALL identificar las políticas de cancelación/reprogramación como pendientes de validación; y SHALL excluir de la versión inicial la facturación SUNAT, usando pasajes PDF con QR.

#### Scenario: Revisar alcance de la versión inicial
- **WHEN** se describe una capacidad fuera del alcance v1 o una regla todavía no validada
- **THEN** la documentación la etiqueta como evolución futura o pendiente, y no la presenta como funcionalidad acordada de v1.

### Requirement: Contratos canónicos
La documentación SHALL usar `IPaymentGateway`, `INotificationGateway`, `IReservationRepository` e `ISeatAvailabilityService` como interfaces de Aplicación, con sus implementaciones correspondientes en Infraestructura.

#### Scenario: Describir integración externa o persistencia
- **WHEN** un documento especifica acceso a pagos, notificaciones, persistencia o disponibilidad de asientos
- **THEN** identifica el contrato canónico de Aplicación y su implementación de Infraestructura, sin sustituir la interfaz por el nombre de la implementación.
