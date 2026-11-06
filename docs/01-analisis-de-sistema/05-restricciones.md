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
| **RC05** | Administración jerárquica | El **Superadmin** administra la plataforma y asigna Administradores de Agencia. Cada Administrador de Agencia gestiona únicamente la información de la agencia que tiene asignada. |
| **RC06** | Transporte multimodal | La solución no estará limitada a buses. Cada empresa podrá configurar el tipo de unidad utilizado, incluyendo buses, combis, taxis u otros vehículos, según su operación. |
| **RC07** | Disponibilidad compartida | Las reservas realizadas por pasajeros mediante la plataforma y las ventas realizadas presencialmente por un Agente/Vendedor deben utilizar la misma fuente transaccional de disponibilidad. |
| **RC08** | Reserva temporal | Una reserva permanecerá temporalmente en estado **HELD durante 10 minutos** mientras el pasajero completa el pago. Si el pago no se confirma dentro de ese periodo, la reserva expirará y el asiento o cupo será liberado. |
| **RC09** | Prevención de sobreventa | El sistema debe impedir que dos operaciones confirmen simultáneamente el mismo asiento o cupo disponible. La base de datos será la fuente de verdad para la disponibilidad. |
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
    └── gestiona únicamente su agencia
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

Si transcurren los 10 minutos sin confirmación del pago:

```text
HELD
  ↓
10 minutos
  ↓
EXPIRED
  ↓
Asiento / cupo liberado
```

La misma regla deberá aplicarse independientemente de que la operación se origine desde el canal web, móvil o desde un Agente/Vendedor.

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
        Pagos externos          Notificaciones
```

Esto permitirá mantener la lógica de negocio independiente de los proveedores externos.

## 5. Restricciones de alcance

La primera versión contempla principalmente:

- Gestión de usuarios y roles.
- Gestión de empresas y agencias.
- Gestión de rutas.
- Gestión de horarios.
- Configuración de servicios.
- Gestión de unidades y tipos de transporte.
- Consulta de viajes.
- Consulta de disponibilidad.
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

## 6. Restricciones con mayor impacto arquitectónico

Las restricciones que tendrán mayor influencia sobre las decisiones arquitectónicas son:

| Restricción | Impacto arquitectónico |
|---|---|
| **RC02 - Multiempresa/multiagencia** | Obliga a diseñar aislamiento lógico y control de ámbito. |
| **RC03 - Aislamiento de información** | Determina la estrategia de autenticación y autorización. |
| **RC04 - Base compartida** | Requiere un modelo de datos preparado para múltiples empresas y agencias. |
| **RC07 - Disponibilidad compartida** | Obliga a utilizar una única fuente transaccional para reservas presenciales y digitales. |
| **RC08 - Reserva temporal** | Requiere control de estados, expiración y liberación automática. |
| **RC09 - Prevención de sobreventa** | Requiere control de concurrencia y transacciones. |
| **RC10 - Culqi mediante Adapter** | Determina el desacoplamiento de la integración de pagos. |
| **RC11 - WhatsApp** | Requiere integración externa y procesamiento tolerante a fallos. |
| **RC12 - Redis** | Introduce una capa de caché para mejorar rendimiento. |
| **RC13 - RabbitMQ** | Introduce procesamiento asíncrono mediante eventos. |
| **RC15 - Monolito modular + Clean Architecture** | Define la organización inicial de módulos y dependencias. |

Estas restricciones serán utilizadas como entrada para la definición de los **drivers arquitectónicos, decisiones arquitectónicas, estilo, componentes y diseño interno** de la solución.