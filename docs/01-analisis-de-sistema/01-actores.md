
# Identificación de Actores del Sistema

## 1. Descripción

La Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial tiene como propósito centralizar la consulta y gestión de los servicios de transporte ofrecidos por diferentes empresas y sus respectivas agencias o sucursales.

El sistema contempla cuatro actores humanos con responsabilidades diferenciadas. La empresa y la agencia constituyen entidades organizacionales del sistema y no representan roles de usuario.

La identificación de actores permite establecer las responsabilidades, permisos y límites de acceso que posteriormente serán utilizados para definir las historias de usuario, requisitos funcionales y mecanismos de autorización.

---

## 2. Actores principales

| ID | Actor | Descripción | Ámbito de actuación |
|---|---|---|---|
| ACT01 | Pasajero | Usuario que consulta servicios y realiza o gestiona sus reservas y pasajes. | Información y operaciones propias |
| ACT02 | Agente / Vendedor | Personal encargado de atender y registrar operaciones de venta y reserva en una agencia. | Agencia asignada |
| ACT03 | Administrador de Agencia | Usuario responsable de administrar la operación de una o varias agencias autorizadas de una misma empresa. | Agencias asignadas |
| ACT04 | Superadministrador | Usuario responsable de la administración general de la plataforma. | Toda la plataforma |

### ACT01 — Pasajero

**Descripción:** Persona que utiliza la plataforma para consultar servicios de transporte y realizar operaciones relacionadas con sus viajes.

**Responsabilidades:**

- Consultar rutas, horarios, tarifas y servicios disponibles.
- Consultar la disponibilidad de asientos o cupos.
- Realizar reservas de pasajes.
- Realizar pagos mediante los medios habilitados.
- Consultar sus reservas y pasajes.
- Solicitar cancelaciones o reprogramaciones de acuerdo con las condiciones establecidas.
- Recibir confirmaciones, recordatorios y notificaciones relacionadas con sus viajes.

**Restricción de acceso:** El pasajero únicamente puede consultar y gestionar sus propios datos, reservas, pagos y pasajes.

---

### ACT02 — Agente / Vendedor

**Descripción:** Personal autorizado para atender operaciones de venta y reserva de pasajes de manera presencial en una agencia.

**Responsabilidades:**

- Consultar rutas, horarios y servicios.
- Consultar la disponibilidad actual de los servicios.
- Registrar reservas y ventas para los pasajeros.
- Registrar operaciones de pago presencial.
- Emitir pasajes correspondientes a ventas confirmadas.
- Gestionar cancelaciones o reprogramaciones permitidas.
- Consultar las operaciones realizadas en su agencia.
- Atender operaciones concurrentes con otros agentes y pasajeros del canal digital.

**Restricción de acceso:** El agente solo puede realizar operaciones correspondientes a la agencia a la que se encuentra asignado.

No puede consultar ni modificar información de otras agencias o empresas.

---

### ACT03 — Administrador de Agencia

**Descripción:** Usuario responsable de administrar la operación de una o varias agencias que hayan sido autorizadas para su cuenta dentro de una misma empresa.

**Responsabilidades:**

- Consultar y administrar la información de las agencias asignadas.
- Gestionar rutas y destinos correspondientes a sus agencias.
- Registrar y administrar unidades de transporte cuando corresponda.
- Configurar horarios, tarifas y servicios programados.
- Gestionar agentes y vendedores de sus agencias.
- Supervisar reservas, ventas, cancelaciones y reprogramaciones.
- Consultar indicadores y reportes operativos de las agencias autorizadas.
- Gestionar información operativa dentro del ámbito asignado.

**Restricción de acceso:** El administrador únicamente puede acceder a información de la empresa y de las agencias que tenga expresamente autorizadas.

No puede acceder, consultar ni modificar información perteneciente a otra empresa.

Cuando tenga acceso a varias agencias, el sistema deberá permitir seleccionar la agencia sobre la cual desea trabajar, manteniendo el contexto de agencia durante la operación.

**Ejemplo de ámbito:**

```text
Empresa: Transportes Ayacucho S.A.

Administrador:
    ├── Agencia Terminal Norte ✓
    ├── Agencia Terminal Sur   ✓
    └── Agencia Huanta         ✓
```

El administrador podrá cambiar entre sus agencias autorizadas para consultar sus respectivas ventas, reservas, horarios, ocupación y demás información operativa.

---

### ACT04 — Superadministrador

**Descripción:** Usuario responsable de la administración general y configuración de la plataforma.

**Responsabilidades:**

- Registrar y administrar empresas de transporte.
- Registrar, activar o desactivar agencias.
- Asignar administradores a las agencias correspondientes.
- Gestionar usuarios y permisos administrativos.
- Supervisar el funcionamiento general de la plataforma.
- Consultar reportes globales y registros de auditoría.
- Gestionar parámetros generales del sistema.
- Administrar configuraciones generales relacionadas con notificaciones y servicios externos.

**Restricción de acceso:** Posee privilegios administrativos a nivel de plataforma, sujetos a autenticación, autorización y auditoría.

---

## 3. Entidades organizacionales

Las siguientes entidades no constituyen actores del sistema:

### Empresa de transporte

Representa a la organización que ofrece servicios de transporte mediante una o varias agencias.

Una empresa puede tener múltiples agencias o sucursales.

### Agencia o sucursal

Representa el punto de operación de una empresa desde el cual se gestionan servicios, horarios, ventas, reservas y atención a pasajeros.

Una agencia pertenece exclusivamente a una empresa.

### Relación organizacional

```text
Empresa
   │
   ├── Agencia / Sucursal
   │       ├── Administrador de Agencia
   │       └── Agentes / Vendedores
   │
   ├── Agencia / Sucursal
   │       ├── Administrador de Agencia
   │       └── Agentes / Vendedores
   │
   └── Agencia / Sucursal
           ├── Administrador de Agencia
           └── Agentes / Vendedores
```

---

## 4. Integraciones externas

Las integraciones externas no se consideran actores humanos. Son servicios con los que la plataforma intercambia información para ejecutar determinadas operaciones.

| ID | Integración | Propósito | Interacción |
|---|---|---|---|
| EXT01 | Culqi | Procesamiento de pagos electrónicos. | Solicitud de pago, confirmación y recepción de eventos mediante webhook. |
| EXT02 | WhatsApp Business Cloud API | Envío de notificaciones a pasajeros y usuarios autorizados. | Confirmaciones, recordatorios, cambios y alertas. |

### EXT01 — Culqi

La plataforma utilizará Culqi como pasarela de pago para procesar las operaciones de pago electrónico.

El sistema no considerará una reserva como pagada únicamente porque el usuario complete el proceso de pago en la interfaz. La confirmación deberá realizarse mediante la respuesta correspondiente del proveedor y sus mecanismos de notificación.

La integración se realizará mediante un adaptador para mantener desacoplado el dominio de la implementación específica del proveedor.

### EXT02 — WhatsApp Business Cloud API

La plataforma utilizará WhatsApp como canal de comunicación para enviar notificaciones relacionadas con las operaciones del sistema.

Entre las comunicaciones previstas se encuentran:

- Confirmación de reserva.
- Confirmación de pago.
- Emisión del pasaje.
- Cambios de horario.
- Cancelaciones o reprogramaciones.
- Recordatorios de viajes.
- Alertas preventivas de alta demanda asociadas a festividades o fechas especiales.

El envío de estas comunicaciones no deberá bloquear las operaciones transaccionales principales del sistema.

---

## 5. Matriz general de participación

| Funcionalidad | Pasajero | Agente / Vendedor | Administrador de Agencia | Superadministrador |
|---|:---:|:---:|:---:|:---:|
| Consultar servicios | ✓ | ✓ | ✓ | ✓ |
| Consultar disponibilidad | ✓ | ✓ | ✓ | ✓ |
| Registrar reserva | ✓ | ✓ | ✓ | ✓ |
| Registrar venta presencial | — | ✓ | ✓ | ✓ |
| Realizar pago electrónico | ✓ | — | — | — |
| Registrar pago presencial | — | ✓ | ✓ | ✓ |
| Consultar reservas propias | ✓ | — | — | — |
| Consultar reservas de la agencia | — | ✓ | ✓ | ✓ |
| Gestionar horarios | — | — | ✓ | ✓ |
| Gestionar rutas | — | — | ✓ | ✓ |
| Gestionar unidades de transporte | — | — | ✓ | ✓ |
| Gestionar agentes | — | — | ✓ | ✓ |
| Gestionar agencias | — | — | — | ✓ |
| Gestionar empresas | — | — | — | ✓ |
| Consultar reportes de agencia | — | Según permisos | ✓ | ✓ |
| Consultar reportes globales | — | — | — | ✓ |
| Gestionar parámetros generales | — | — | — | ✓ |

---

## 6. Reglas generales de acceso

- **RG01:** Todo usuario deberá autenticarse antes de acceder a funcionalidades privadas.
- **RG02:** Los permisos se determinarán mediante el rol y el ámbito organizacional asignado al usuario.
- **RG03:** Cada agencia pertenece a una única empresa.
- **RG04:** Un agente únicamente podrá operar sobre la agencia a la que se encuentre asignado.
- **RG05:** Un administrador podrá administrar una o varias agencias únicamente cuando estas pertenezcan a la empresa autorizada para su cuenta.
- **RG06:** Un administrador no podrá consultar ni modificar información de empresas ajenas a su ámbito.
- **RG07:** Un pasajero únicamente podrá consultar y modificar sus propios datos y operaciones.
- **RG08:** El Superadministrador podrá administrar las empresas, agencias y usuarios de la plataforma.
- **RG09:** Toda operación administrativa o crítica deberá mantener trazabilidad mediante registros de auditoría.
- **RG10:** Las autorizaciones deberán validarse en el backend y no depender únicamente de controles realizados en la interfaz.

---

## 7. Aislamiento de información

La plataforma utilizará una única base de datos para gestionar la información de las diferentes empresas y agencias.

La separación lógica de la información se realizará mediante el ámbito organizacional asociado a cada usuario y a las entidades operativas.

Las entidades que requieran aislamiento organizacional deberán mantener referencias a la empresa y, cuando corresponda, a la agencia.

Por ejemplo:

```text
Empresa A
    │
    ├── Agencia Norte
    │      ├── Usuarios
    │      ├── Reservas
    │      ├── Ventas
    │      └── Servicios
    │
    └── Agencia Sur
           ├── Usuarios
           ├── Reservas
           ├── Ventas
           └── Servicios

Empresa B
    │
    └── Agencia Centro
           ├── Usuarios
           ├── Reservas
           ├── Ventas
           └── Servicios
```

El backend deberá validar que el usuario tenga autorización sobre la empresa y agencia solicitadas antes de devolver o modificar información.

La existencia de una única base de datos no implica que los usuarios puedan acceder libremente a la información almacenada. El aislamiento será aplicado mediante autenticación, autorización, ámbito organizacional y controles de acceso a los datos.

---

## 8. Consideraciones arquitectónicas iniciales

La existencia de múltiples empresas y agencias establece como requisito arquitectónico la necesidad de implementar un mecanismo de autorización basado en roles y ámbito organizacional.

Las operaciones de reserva y venta deberán utilizar una fuente de información común sobre la disponibilidad, de manera que una operación realizada por un pasajero desde el canal digital y una operación realizada presencialmente por un agente no puedan confirmar simultáneamente el mismo asiento o cupo.

Asimismo, las operaciones de pago y notificaciones deberán mantenerse desacopladas de las operaciones transaccionales principales para evitar que una falla en un servicio externo afecte directamente la disponibilidad o consistencia de las reservas.

Estas necesidades serán utilizadas posteriormente para definir los drivers, tácticas y decisiones arquitectónicas.

---

## 9. Alcance de la identificación

La propuesta contempla únicamente cuatro actores humanos:

1. Pasajero.
2. Agente / Vendedor.
3. Administrador de Agencia.
4. Superadministrador.

No se contempla un rol independiente de Administrador de Empresa.

La empresa y la agencia se modelan como entidades organizacionales. Las pasarelas de pago y servicios de comunicación se consideran integraciones externas.

La incorporación de nuevos roles deberá justificarse mediante una necesidad funcional y mantener consistencia con los requisitos, autorización, arquitectura y modelo C4.