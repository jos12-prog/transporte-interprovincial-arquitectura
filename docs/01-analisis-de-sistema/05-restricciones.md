# Restricciones Arquitectónicas

Las restricciones arquitectónicas establecen las condiciones de negocio, operación, seguridad y tecnología que limitan las alternativas de diseño de la Plataforma Integrada de Transporte Interprovincial en Ayacucho.

Estas restricciones deben ser consideradas en las decisiones arquitectónicas, diseño de componentes, modelo de datos, seguridad, integración con servicios externos y estrategia de pruebas.

## 1. Restricciones identificadas

| ID | Restricción | Descripción |
|---|---|---|
| **RC01** | Cuatro actores principales | El sistema contempla únicamente cuatro actores humanos: **Superadmin, Administrador de Agencia, Agente/Vendedor y Pasajero**. Las empresas y agencias son entidades administradas dentro de la plataforma, no actores adicionales. |
| **RC02** | Modelo multiempresa y multiagencia | Una misma plataforma debe soportar múltiples empresas de transporte y sus respectivas agencias. Cada empresa y agencia debe mantener su información y operación independiente. |
| **RC03** | Aislamiento de información | La información debe estar aislada por empresa y agencia. Un Administrador de Agencia o Agente/Vendedor no podrá consultar ni modificar información perteneciente a otra agencia. |
| **RC04** | Base de datos compartida | La solución utilizará una única base de datos PostgreSQL para todas las empresas y agencias. El aislamiento será lógico mediante identificación de empresa y agencia, roles y autorización. |
| **RC05** | Administración jerárquica | El **Superadmin** administra la plataforma y asigna Administradores de Agencia. Cada Administrador de Agencia gestiona una o varias agencias autorizadas de una sola empresa. |
| **RC06** | Modelos de vehículos y asientos numerados | El Superadministrador administra modelos con capacidad y plano de asientos numerados; el Administrador de Agencia registra vehículos por placa y selecciona un modelo. La v1 asigna únicamente asientos numerados. |
| **RC07** | Disponibilidad compartida | Las reservas realizadas por pasajeros mediante la plataforma y las ventas realizadas presencialmente por un Agente/Vendedor deben utilizar la misma fuente transaccional de disponibilidad. |
| **RC08** | Reserva temporal | Solo una reserva del canal digital permanecerá en estado **HELD durante 10 minutos** mientras el pasajero completa el pago. Si no se confirma, expirará y los asientos se liberarán. Las ventas de agente se confirman directamente tras registrar el pago presencial. |
| **RC09** | Prevención de sobreventa | El sistema debe impedir que dos operaciones confirmen simultáneamente el mismo asiento numerado de un viaje. La base de datos será la fuente de verdad para la disponibilidad. |
| **RC10** | Pasarela de pago externa | Los pagos electrónicos se realizarán mediante **Culqi**, utilizando el patrón **Adapter** para mantener desacoplada la lógica de negocio respecto del proveedor externo. |
| **RC11** | Notificaciones externas | Las notificaciones al pasajero se realizarán mediante **WhatsApp Business Cloud API**. El fallo temporal del servicio de notificaciones no deberá comprometer una reserva o pago ya confirmado. |
| **RC12** | Caché | La solución deberá incorporar **Redis** para almacenar información de consulta frecuente y reducir la carga sobre la base de datos. La información transaccional crítica no dependerá exclusivamente de la caché. |
| **RC13** | Procesamiento asíncrono | La solución deberá incorporar **RabbitMQ** como cola durable para procesar eventos y tareas que no necesitan ejecutarse dentro de la transacción principal, como notificaciones y otros procesos secundarios. |
| **RC14** | Sin módulo de encomiendas | La primera versión del sistema no contempla gestión de encomiendas. El alcance se concentra en rutas, horarios, servicios, disponibilidad, reservas, pagos, pasajes, notificaciones y explotación de información. |
| **RC15** | Arquitectura inicial | La primera versión se implementará como un **monolito modular con API REST**, organizado bajo **Clean Architecture**, permitiendo una evolución posterior sin acoplar las reglas de negocio a la infraestructura. |

## 2. Restricciones de seguridad y aislamiento

Debido a que diferentes empresas utilizarán la misma plataforma y base de datos, el acceso deberá controlarse considerando tanto el rol como el ámbito organizacional.

El modelo de autorización será:

```text
Superadmin
    │
    ├── administra empresas y agencias
    │
    ▼
Administrador de Agencia
    │
    └── gestiona una o varias agencias autorizadas de la misma empresa
              │
              ▼
       Agente / Vendedor
              │
              └── opera únicamente en su agencia

Pasajero
    │
    └── accede únicamente a sus propias reservas y pasajes
```

Toda información operativa deberá estar asociada a una empresa y agencia cuando corresponda.

Por ejemplo:

```text
Empresa A
 ├── Agencia A1
 │    ├── Administrador A1
 │    └── Agentes A1
 │
 └── Agencia A2
      ├── Administrador A2
      └── Agentes A2

Empresa B
 └── Agencia B1
      ├── Administrador B1
      └── Agentes B1
```

Un usuario perteneciente a **Empresa A / Agencia A1** no podrá consultar ni modificar información de **Empresa B / Agencia B1**.

El token incluye `sub`, `role`, `company_id` y `agency_ids[]`. Para seleccionar una agencia autorizada, el cliente envía su identificador en `X-Agency-Id`; el backend valida el rol, la empresa y la pertenencia de la agencia en cada caso de uso.

## 3. Restricciones sobre reservas

La reserva deberá mantener estados controlados para evitar inconsistencias:

```text
DISPONIBLE
     ↓
   HELD
     ↓
PAGO PENDIENTE
     ↓
PAGO CONFIRMADO
     ↓
CONFIRMADA
     ↓
PASaje EMITIDO
```

La retención `HELD` se aplica solo a reservas digitales. Las ventas presenciales se confirman directamente al registrar el pago. Las máquinas de estado son: reserva (`HELD`, `CONFIRMED`, `CANCELLED`, `EXPIRED`, `RESCHEDULED`), pago (`PENDING`, `APPROVED`, `REJECTED`, `ERROR`, `REFUNDED`) y pasaje (`ISSUED`, `USED`, `VOIDED`).

Si transcurren los 10 minutos sin confirmación del pago:

```text
HELD
  ↓
10 minutos
  ↓
EXPIRED
  ↓
Asientos liberados
```

La retención de 10 minutos se aplica únicamente a operaciones digitales. La venta presencial registra pago en efectivo, Yape/Plin o POS y confirma directamente los asientos. La aplicación v1 es web responsive para navegador de escritorio o móvil; no incluye app móvil nativa.

## 4. Restricciones de integración

Las integraciones externas no deberán formar parte directamente de las reglas centrales del dominio.

Se utilizarán adaptadores:

```text
                  ┌───────────────┐
                  │   Aplicación  │
                  └───────┬───────┘
                          │
                 ┌────────▼────────┐
                 │      Adapter    │
                 └───────┬─────────┘
                         │
             ┌───────────┴───────────┐
             ▼                       ▼
           Culqi             WhatsApp Business
        Pago + webhook       Notificaciones asíncronas
```

El backend crea el cargo de Culqi a partir del token generado por Culqi Checkout. La respuesta del servidor y los webhooks firmados HMAC se reconcilian idempotentemente por `charge_id`; el primer resultado válido es autoritativo. Los eventos de notificación se guardan con Transactional Outbox y se despachan mediante RabbitMQ. Esto permitirá mantener la lógica de negocio independiente de los proveedores externos.

## 5. Restricciones de alcance

La primera versión contempla principalmente:

- Gestión de usuarios y roles.
- Gestión de empresas y agencias.
- Gestión de rutas.
- Gestión de horarios.
- Configuración de servicios.
- Gestión de unidades y tipos de transporte.
- Catálogo `VehicleModel` del Superadministrador con nombre, categoría, capacidad y `SeatLayout` (conductor y asientos por número, fila, columna y piso); modelos iniciales Toyota Yaris (4), Toyota Avanza (6), Minivan (7), Van (15), Bus 1 piso y Bus 2 pisos.
- Registro de `Vehicle` por placa y modelo; al crear un viaje, generación de `TripSeat` desde el plano del modelo.
- Consulta de viajes.
- Consulta compartida del mapa de asientos numerados con estados disponible, retenido/ocupado y seleccionado.
- Reservas de uno o varios asientos, asociando DNI y nombre de pasajero a cada asiento.
- Reserva temporal.
- Confirmación de compra.
- Pagos mediante Culqi.
- Emisión de pasajes.
- Actualización de disponibilidad.
- Notificaciones mediante WhatsApp.
- Reportes y explotación de información.
- Auditoría de operaciones críticas.

No se contempla en esta versión:

- Gestión de encomiendas.
- Servicio de delivery.
- Integración con múltiples pasarelas simultáneamente.
- Operación mediante microservicios independientes.
- App móvil nativa, réplica de lectura y gestión de choferes (evolución futura).
- Emisión de comprobantes SUNAT en v1; el pasaje se entrega como PDF con QR.

## 6. Restricciones con mayor impacto arquitectónico

Las restricciones que tendrán mayor influencia sobre las decisiones arquitectónicas son:

| Restricción | Impacto arquitectónico |
|---|---|
| **RC02 - Multiempresa/multiagencia** | Obliga a diseñar aislamiento lógico y control de ámbito. |
| **RC03 - Aislamiento de información** | Determina la estrategia de autenticación y autorización. |
| **RC04 - Base compartida** | Requiere un modelo de datos preparado para múltiples empresas y agencias. |
| **RC07 - Disponibilidad compartida** | Obliga a utilizar una única fuente transaccional para reservas presenciales y digitales. |
| **RC08 - Reserva temporal digital** | Requiere control de estados, expiración y liberación automática; la venta presencial se confirma al registrar el pago. |
| **RC09 - Prevención de sobreventa** | Requiere control de concurrencia y transacciones. |
| **RC10 - Culqi mediante Adapter** | Determina el desacoplamiento de la integración de pagos. |
| **RC11 - WhatsApp** | Requiere integración externa y procesamiento tolerante a fallos. |
| **RC12 - Redis** | Introduce una capa de caché para mejorar rendimiento. |
| **RC13 - RabbitMQ** | Introduce procesamiento asíncrono mediante eventos. |
| **RC15 - Monolito modular + Clean Architecture** | Define la organización inicial de módulos y dependencias. |

Estas restricciones serán utilizadas como entrada para la definición de los **drivers arquitectónicos, decisiones arquitectónicas, estilo, componentes y diseño interno** de la solución.
