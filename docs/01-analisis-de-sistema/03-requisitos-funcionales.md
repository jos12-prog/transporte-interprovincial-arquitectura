# Requisitos Funcionales

## 1. Introducción

Los requisitos funcionales definen las capacidades que debe proporcionar la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial en Ayacucho.

Estos requisitos se derivan de las historias de usuario definidas para los actores del sistema y constituyen la base para las siguientes etapas de análisis, arquitectura, diseño e implementación.

La plataforma deberá permitir la operación de múltiples empresas y agencias manteniendo el aislamiento de la información y los permisos correspondientes.

---

## 2. Requisitos funcionales

### 2.1 Consulta y búsqueda de viajes

| ID | Requisito funcional |
|---|---|
| **RF01** | El sistema debe permitir buscar viajes indicando origen, destino y fecha. |
| **RF02** | El sistema debe permitir filtrar los resultados de búsqueda por empresa, agencia, horario y tarifa. |
| **RF03** | El sistema debe permitir mostrar y comparar los servicios disponibles de diferentes empresas y agencias. |
| **RF04** | El sistema debe permitir consultar el detalle de un servicio, incluyendo empresa, agencia, origen, destino, horario, tarifa y condiciones del viaje. |
| **RF05** | El sistema debe permitir consultar la disponibilidad actualizada de asientos o cupos de cada servicio. |
| **RF06** | El sistema debe permitir seleccionar un asiento o cupo disponible antes de iniciar una reserva. |

### 2.2 Reservas y disponibilidad

| ID | Requisito funcional |
|---|---|
| **RF07** | El sistema debe permitir registrar una reserva temporal asociada al pasajero, servicio y asiento o cupo seleccionado. |
| **RF08** | El sistema debe bloquear temporalmente el asiento o cupo reservado para impedir que sea asignado a otra operación mientras la reserva se encuentre vigente. |
| **RF09** | El sistema debe establecer una vigencia inicial de **10 minutos** para las reservas temporales realizadas por el canal digital. |
| **RF10** | El sistema debe confirmar una reserva únicamente cuando el resultado del pago haya sido validado correctamente. |
| **RF11** | El sistema debe actualizar la disponibilidad del servicio después de una reserva confirmada, cancelada o expirada. |
| **RF12** | El sistema debe impedir que dos operaciones concurrentes confirmen simultáneamente el mismo asiento o cupo. |
| **RF13** | El sistema debe registrar el estado de cada reserva, incluyendo como mínimo los estados temporal, confirmada, cancelada y expirada. |

### 2.3 Pasajes y operaciones del pasajero

| ID | Requisito funcional |
|---|---|
| **RF14** | El sistema debe permitir al pasajero consultar sus reservas y pasajes registrados. |
| **RF15** | El sistema debe permitir cancelar una reserva o pasaje cuando las condiciones establecidas para el servicio lo permitan. |
| **RF16** | El sistema debe permitir gestionar la reprogramación de una reserva o pasaje cuando las condiciones del servicio lo permitan. |
| **RF17** | El sistema debe registrar el historial de cancelaciones y reprogramaciones realizadas sobre una reserva o pasaje. |

### 2.4 Pagos

| ID | Requisito funcional |
|---|---|
| **RF18** | El sistema debe permitir iniciar el proceso de pago electrónico asociado a una reserva. |
| **RF19** | El sistema debe validar el resultado del pago antes de confirmar definitivamente la reserva. |
| **RF20** | El sistema debe registrar el resultado y estado de cada operación de pago asociada a una reserva o venta. |
| **RF21** | El sistema debe impedir que una reserva sea confirmada cuando el pago no haya sido validado correctamente. |

### 2.5 Operación presencial

| ID | Requisito funcional |
|---|---|
| **RF22** | El sistema debe permitir al Agente/Vendedor consultar los servicios, horarios y disponibilidad de su agencia. |
| **RF23** | El sistema debe permitir al Agente/Vendedor registrar reservas para pasajeros atendidos presencialmente. |
| **RF24** | El sistema debe permitir al Agente/Vendedor registrar ventas presenciales de pasajes y su forma de pago. |
| **RF25** | El sistema debe permitir emitir o registrar el pasaje correspondiente a una venta confirmada. |
| **RF26** | El sistema debe utilizar la misma disponibilidad de asientos o cupos para las operaciones digitales y presenciales. |
| **RF27** | El sistema debe permitir al Agente/Vendedor consultar las operaciones realizadas en su agencia. |
| **RF28** | El sistema debe permitir al Agente/Vendedor cancelar o reprogramar operaciones únicamente cuando cuente con autorización para realizar dichas acciones. |

### 2.6 Gestión de agencias

| ID | Requisito funcional |
|---|---|
| **RF29** | El sistema debe permitir al Administrador de Agencia consultar las agencias que tiene asignadas. |
| **RF30** | El sistema debe permitir al Administrador de Agencia gestionar las rutas y destinos correspondientes a sus agencias autorizadas. |
| **RF31** | El sistema debe permitir al Administrador de Agencia gestionar los horarios y servicios programados. |
| **RF32** | El sistema debe permitir al Administrador de Agencia gestionar las tarifas de los servicios autorizados. |
| **RF33** | El sistema debe permitir al Administrador de Agencia registrar y gestionar las unidades de transporte asociadas a sus servicios. |
| **RF34** | El sistema debe permitir al Administrador de Agencia gestionar los Agentes/Vendedores asociados a sus agencias autorizadas. |
| **RF35** | El sistema debe permitir al Administrador de Agencia consultar reservas, ventas, ocupación y operaciones de sus agencias. |
| **RF36** | El sistema debe generar reportes de ventas, demanda, ocupación y rutas para el Administrador de Agencia. |

### 2.7 Administración global

| ID | Requisito funcional |
|---|---|
| **RF37** | El sistema debe permitir al Superadministrador registrar y gestionar empresas de transporte. |
| **RF38** | El sistema debe permitir al Superadministrador registrar y gestionar agencias o sucursales asociadas a las empresas. |
| **RF39** | El sistema debe permitir al Superadministrador asignar Administradores de Agencia según la empresa y agencia correspondiente. |
| **RF40** | El sistema debe permitir al Superadministrador activar o desactivar empresas y agencias. |
| **RF41** | El sistema debe permitir al Superadministrador configurar los permisos y ámbitos de acceso de los usuarios administrativos. |
| **RF42** | El sistema debe permitir al Superadministrador consultar indicadores globales de ventas, demanda, ocupación y rutas. |
| **RF43** | El sistema debe permitir al Superadministrador consultar los registros de auditoría de las operaciones críticas realizadas en la plataforma. |

### 2.8 Alta demanda y alertas

| ID | Requisito funcional |
|---|---|
| **RF44** | El sistema debe permitir registrar festividades, eventos y fechas especiales que puedan generar incrementos de demanda. |
| **RF45** | El sistema debe identificar servicios que alcancen el umbral de alta demanda configurado. |
| **RF46** | El sistema debe generar alertas preventivas para los pasajeros cuando un servicio o periodo alcance las condiciones de alta demanda establecidas. |

### 2.9 Notificaciones

| ID | Requisito funcional |
|---|---|
| **RF47** | El sistema debe permitir enviar notificaciones relacionadas con confirmaciones, pagos, recordatorios, cancelaciones, reprogramaciones y modificaciones de servicios. |
| **RF48** | El sistema debe procesar las notificaciones de forma independiente de las operaciones principales, de manera que una falla en el servicio de notificaciones no impida completar una reserva, venta o pago confirmado. |

---

## 3. Requisitos transversales de operación

Además de los requisitos anteriores, las operaciones deberán cumplir las siguientes condiciones funcionales:

### 3.1 Control de acceso

El sistema debe restringir las funcionalidades de acuerdo con el rol del usuario y su ámbito organizacional.

Un usuario no deberá acceder a información de empresas o agencias que no estén dentro de su ámbito autorizado.

### 3.2 Aislamiento multiempresa y multiagencia

La plataforma deberá permitir almacenar información de múltiples empresas y agencias en una misma solución, manteniendo separación lógica entre sus operaciones.

El Administrador de Agencia solo podrá gestionar las agencias que tenga asignadas.

El Agente/Vendedor solo podrá realizar operaciones dentro de la agencia a la que se encuentre asociado.

### 3.3 Consistencia de disponibilidad

La disponibilidad de asientos o cupos deberá mantenerse consistente entre los diferentes canales de operación.

El siguiente escenario no deberá ser posible:

```text
Pasajero web ──────┐
                   ├──> mismo servicio
Agente/Vendedor ───┘
                   ↓
        mismo asiento/cupo
                   ↓
          SOLO UNA CONFIRMACIÓN
```

### 3.4 Expiración de reservas

Cuando una reserva temporal supere los 10 minutos sin confirmación de pago:

```text
HELD
 ↓
10 minutos
 ↓
EXPIRED
 ↓
Liberar asiento/cupo
 ↓
Disponible nuevamente
```

### 3.5 Trazabilidad

Las operaciones críticas deberán conservar información suficiente para identificar:

- usuario responsable;
- empresa;
- agencia;
- operación realizada;
- fecha y hora;
- estado anterior;
- estado posterior, cuando corresponda.

---

## 4. Relación entre historias de usuario y requisitos funcionales

| Historia | Requisitos relacionados |
|---|---|
| HU01 | RF01 |
| HU02 | RF02, RF03 |
| HU03 | RF04 |
| HU04 | RF05 |
| HU05 | RF06 |
| HU06 | RF07, RF08, RF09 |
| HU07 | RF18, RF19, RF20, RF21 |
| HU08 | RF10, RF20 |
| HU09 | RF14 |
| HU10 | RF15, RF17 |
| HU11 | RF16, RF17 |
| HU12 | RF47 |
| HU13 | RF47 |
| HU14 | RF44, RF45, RF46 |
| HU15 | RF22 |
| HU16 | RF23 |
| HU17 | RF24 |
| HU18 | RF25 |
| HU19 | RF27 |
| HU20 | RF28 |
| HU21 | RF26, RF12 |
| HU22 | RF29 |
| HU23 | RF30 |
| HU24 | RF31 |
| HU25 | RF32 |
| HU26 | RF33 |
| HU27 | RF34 |
| HU28 | RF35 |
| HU29 | RF36 |
| HU30 | RF47 |
| HU31 | RF37 |
| HU32 | RF38 |
| HU33 | RF39 |
| HU34 | RF40 |
| HU35 | RF41 |
| HU36 | RF42 |
| HU37 | RF43 |
| HU38 | RF44, RF45, RF46 |
| HU39 | RF41 |
| HU40 | RF43 |
| HU41 | RF09, RF11 |
| HU42 | RF10, RF19, RF21 |
| HU43 | RF48 |
| HU44 | RF12, RF26 |

---

## 5. Resumen

La plataforma contempla **48 requisitos funcionales**, derivados de las historias de usuario de los cuatro actores principales.

Los requisitos cubren:

- consulta y búsqueda de viajes;
- reservas y disponibilidad;
- pagos;
- venta presencial;
- pasajes;
- empresas y agencias;
- rutas, horarios, tarifas y vehículos;
- usuarios y permisos;
- reportes;
- auditoría;
- notificaciones;
- alta demanda;
- control de concurrencia;
- aislamiento multiempresa y multiagencia.

Las **encomiendas no forman parte del alcance funcional actual** y, por tanto, no se incluyen requisitos relacionados con dicho módulo.