# Historias de Usuario

## 1. Introducción

Las historias de usuario describen las principales necesidades de los actores que interactúan con la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial en Ayacucho.

Cada historia se expresa desde la perspectiva del actor que utilizará el sistema y establece la funcionalidad requerida junto con el beneficio esperado.

La solución contempla cuatro actores humanos: **Pasajero, Agente/Vendedor, Administrador de Agencia y Superadministrador**.

Las empresas de transporte y las agencias constituyen entidades organizacionales del sistema, mientras que Culqi y WhatsApp Business Cloud API corresponden a integraciones externas.

---

## 2. Historias de usuario del Pasajero

| ID | Historia de usuario |
|---|---|
| **HU01** | Como pasajero, quiero buscar viajes indicando origen, destino y fecha, para encontrar alternativas de transporte disponibles. |
| **HU02** | Como pasajero, quiero consultar y comparar los servicios de diferentes empresas y agencias, para elegir la alternativa que mejor se adapte a mis necesidades. |
| **HU03** | Como pasajero, quiero consultar el detalle de un viaje, incluyendo horario, tarifa, empresa, agencia y disponibilidad, para decidir si deseo adquirirlo. |
| **HU04** | Como pasajero, quiero consultar la disponibilidad actualizada de asientos o cupos, para conocer las opciones disponibles antes de realizar una reserva. |
| **HU05** | Como pasajero, quiero seleccionar un asiento o cupo disponible, para iniciar el proceso de reserva de mi viaje. |
| **HU06** | Como pasajero, quiero reservar temporalmente un asiento o cupo durante un periodo limitado, para disponer de tiempo suficiente para completar el pago. |
| **HU07** | Como pasajero, quiero realizar el pago electrónico de mi reserva, para confirmar la adquisición del pasaje. |
| **HU08** | Como pasajero, quiero recibir la confirmación de mi reserva y pago, para tener constancia de que mi pasaje fue confirmado correctamente. |
| **HU09** | Como pasajero, quiero consultar mis reservas y pasajes, para conocer el estado de mis operaciones y próximos viajes. |
| **HU10** | Como pasajero, quiero cancelar una reserva o pasaje cuando las condiciones del servicio lo permitan, para gestionar adecuadamente mi viaje. |
| **HU11** | Como pasajero, quiero solicitar la reprogramación de mi viaje cuando esté permitido, para modificar mi fecha u horario de viaje según las condiciones establecidas. |
| **HU12** | Como pasajero, quiero recibir notificaciones sobre cambios de horario, cancelaciones o modificaciones de mi viaje, para estar informado oportunamente. |
| **HU13** | Como pasajero, quiero recibir recordatorios de mi viaje, para evitar olvidos y llegar con anticipación a la agencia correspondiente. |
| **HU14** | Como pasajero, quiero recibir alertas preventivas cuando exista una alta demanda relacionada con una festividad o fecha especial, para planificar mi viaje con anticipación. |

---

## 3. Historias de usuario del Agente / Vendedor

| ID | Historia de usuario |
|---|---|
| **HU15** | Como agente/vendedor, quiero consultar los servicios, horarios y disponibilidad de mi agencia, para atender las solicitudes de los pasajeros. |
| **HU16** | Como agente/vendedor, quiero registrar una reserva para un pasajero desde la agencia, para atender operaciones realizadas presencialmente. |
| **HU17** | Como agente/vendedor, quiero registrar una venta presencial y su forma de pago, para confirmar la adquisición de un pasaje directamente en la agencia. |
| **HU18** | Como agente/vendedor, quiero emitir el pasaje de una venta confirmada, para entregar al pasajero su comprobante de viaje. |
| **HU19** | Como agente/vendedor, quiero consultar las operaciones realizadas en mi agencia, para verificar las reservas y ventas efectuadas durante mi atención. |
| **HU20** | Como agente/vendedor, quiero cancelar o reprogramar una operación cuando tenga autorización para hacerlo, para atender las solicitudes permitidas de los pasajeros. |
| **HU21** | Como agente/vendedor, quiero que las ventas presenciales y las reservas digitales utilicen la misma disponibilidad, para evitar la asignación duplicada de un asiento o cupo. |

---

## 4. Historias de usuario del Administrador de Agencia

| ID | Historia de usuario |
|---|---|
| **HU22** | Como administrador de agencia, quiero consultar las agencias que tengo autorizadas, para seleccionar sobre cuál de ellas realizar operaciones administrativas. |
| **HU23** | Como administrador de agencia, quiero gestionar las rutas y destinos de mis agencias autorizadas, para mantener actualizada la oferta de servicios. |
| **HU24** | Como administrador de agencia, quiero gestionar los horarios y servicios programados, para mantener actualizada la programación de viajes. |
| **HU25** | Como administrador de agencia, quiero gestionar las tarifas de los servicios, para mantener actualizados los precios ofrecidos a los pasajeros. |
| **HU26** | Como administrador de agencia, quiero registrar y gestionar las unidades de transporte cuando corresponda, para asociarlas a los servicios que las requieran. |
| **HU27** | Como administrador de agencia, quiero gestionar los agentes/vendedores de mis agencias autorizadas, para controlar quién puede realizar operaciones presenciales. |
| **HU28** | Como administrador de agencia, quiero consultar las reservas, ventas, ocupación y operaciones de mis agencias autorizadas, para supervisar su funcionamiento. |
| **HU29** | Como administrador de agencia, quiero consultar reportes de ventas, demanda, ocupación y rutas utilizadas, para apoyar la toma de decisiones de la agencia. |
| **HU30** | Como administrador de agencia, quiero recibir alertas sobre modificaciones o incidencias que afecten los servicios de mis agencias, para tomar las acciones correspondientes. |

---

## 5. Historias de usuario del Superadministrador

| ID | Historia de usuario |
|---|---|
| **HU31** | Como superadministrador, quiero registrar y gestionar empresas de transporte, para incorporar nuevos operadores a la plataforma. |
| **HU32** | Como superadministrador, quiero registrar y gestionar agencias o sucursales asociadas a una empresa, para organizar la operación de cada operador. |
| **HU33** | Como superadministrador, quiero asignar administradores a las agencias correspondientes, para controlar quién administra cada ámbito operativo. |
| **HU34** | Como superadministrador, quiero activar o desactivar empresas y agencias, para controlar su participación en la plataforma. |
| **HU35** | Como superadministrador, quiero gestionar los permisos y ámbitos de acceso de los usuarios administrativos, para proteger la información de las empresas y agencias. |
| **HU36** | Como superadministrador, quiero consultar indicadores globales de ventas, demanda, ocupación y rutas, para supervisar el comportamiento general de la plataforma. |
| **HU37** | Como superadministrador, quiero consultar los registros de auditoría, para conocer las operaciones administrativas y críticas realizadas en la plataforma. |
| **HU38** | Como superadministrador, quiero configurar eventos, festividades y parámetros de alta demanda, para generar alertas preventivas a los pasajeros cuando corresponda. |

---

## 6. Historias de usuario transversales

Las siguientes historias representan comportamientos que involucran diferentes actores y componentes de la plataforma.

| ID | Historia de usuario |
|---|---|
| **HU39** | Como usuario autorizado, quiero que el sistema limite mi acceso según mi rol, empresa y agencia asignada, para impedir el acceso a información que no me corresponde. |
| **HU40** | Como usuario de la plataforma, quiero que las operaciones críticas queden registradas, para garantizar trazabilidad y auditoría. |
| **HU41** | Como pasajero, quiero que una reserva temporal expire automáticamente cuando no se confirme dentro del tiempo establecido, para liberar el asiento o cupo para otros pasajeros. |
| **HU42** | Como pasajero, quiero que mi reserva se confirme únicamente cuando el sistema valide correctamente el resultado del pago, para evitar inconsistencias entre reservas y pagos. |
| **HU43** | Como usuario, quiero recibir las notificaciones de manera independiente de la operación principal, para que una falla en el servicio de mensajería no afecte mi reserva o compra. |
| **HU44** | Como pasajero o agente, quiero que el sistema mantenga una disponibilidad consistente cuando existan operaciones simultáneas, para evitar la sobreventa o asignación duplicada. |

---

## 7. Reglas principales asociadas a las historias

### Reserva temporal

Las reservas realizadas por el canal digital tendrán una vigencia inicial de **10 minutos** para completar el proceso de pago.

El flujo será:

```text
Seleccionar asiento/cupo
        ↓
Reserva temporal
        ↓
Estado: HELD
        ↓
10 minutos
   ↙          ↘
Pago       Sin pago
  ↓             ↓
CONFIRMED     EXPIRED
                ↓
       Liberar asiento/cupo
```

El tiempo de 10 minutos constituye un parámetro inicial de diseño y podrá ser ajustado posteriormente mediante configuración y pruebas de carga.

### Venta presencial

Las operaciones realizadas por un agente no utilizarán una disponibilidad independiente.

El agente consultará y modificará la **misma disponibilidad transaccional** utilizada por el canal digital.

```text
Pasajero Web ──────┐
                   ├──> Servicio de disponibilidad
Agente/Vendedor ───┘
                         ↓
                    Base de datos
                         ↓
                  Estado consistente
```

De esta manera, una venta presencial y una reserva digital no podrán confirmar simultáneamente el mismo asiento o cupo.

### Pago electrónico

La confirmación de una reserva no dependerá únicamente de la respuesta visual mostrada al pasajero.

El sistema deberá validar el resultado del proveedor de pago mediante los mecanismos de integración definidos, incluyendo la recepción de eventos cuando corresponda.

### Notificaciones

Las notificaciones se procesarán de manera desacoplada de las operaciones principales.

Entre ellas se consideran:

- Confirmación de reserva.
- Confirmación de pago.
- Emisión de pasaje.
- Recordatorio del viaje.
- Cambio de horario.
- Cancelación.
- Reprogramación.
- Alertas preventivas por alta demanda.

### Alta demanda

El sistema podrá identificar situaciones de alta demanda utilizando información como:

- fecha del viaje;
- ocupación del servicio;
- festividades o eventos registrados;
- comportamiento de la demanda.

Como parámetro inicial, se podrá considerar un umbral del **70 % de ocupación**, sujeto a validación mediante pruebas y datos históricos.

Por ejemplo:

```text
Capacidad: 40 pasajeros
Ocupación: 30 pasajeros
Ocupación: 75 %

        ↓

ALTA DEMANDA

        ↓

Generar alerta preventiva
```

---

## 8. Consideración de aislamiento de información

Las historias de usuario administrativas deberán respetar el ámbito organizacional de cada usuario.

Una misma base de datos podrá almacenar información de múltiples empresas y agencias, pero cada usuario solamente podrá consultar o modificar la información correspondiente a su ámbito autorizado.

El modelo conceptual será:

```text
Empresa
   │
   ├── Agencia Norte
   │      ├── Administrador
   │      └── Agentes
   │
   └── Agencia Sur
          ├── Administrador
          └── Agentes

Empresa B
   │
   └── Agencia Centro
          ├── Administrador
          └── Agentes
```

Un Administrador de Agencia podrá trabajar con una o varias agencias autorizadas de su empresa, mientras que un Agente/Vendedor estará asociado a una agencia específica.

---

## 9. Resumen de actores e historias

| Actor | Historias |
|---|---|
| **Pasajero** | HU01–HU14 |
| **Agente/Vendedor** | HU15–HU21 |
| **Administrador de Agencia** | HU22–HU30 |
| **Superadministrador** | HU31–HU38 |
| **Transversales** | HU39–HU44 |

Las historias relacionadas con **encomiendas han sido eliminadas**, debido a que dicho módulo no forma parte del alcance definitivo del proyecto.

Las historias se utilizarán como base para la identificación y trazabilidad de los requisitos funcionales.