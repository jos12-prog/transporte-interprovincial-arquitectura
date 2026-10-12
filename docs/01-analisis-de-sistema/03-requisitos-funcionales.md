

# 03. Requisitos Funcionales

## 1. Descripción

Los requisitos funcionales establecen las operaciones que debe realizar la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

La plataforma centraliza los servicios de diferentes empresas y agencias, incluyendo consulta de viajes, reserva y venta de pasajes, administración de transporte y gestión de encomiendas.

El acceso a las funcionalidades depende de los cuatro roles definidos y del ámbito organizacional autorizado.

## 2. Requisitos funcionales

### 2.1. Consulta y búsqueda de viajes

| ID | Requisito funcional | Historia |
|---|---|---|
| RF01 | Permitir buscar viajes por origen, destino y fecha. | HU01 |
| RF02 | Mostrar las empresas que ofrecen viajes para la búsqueda realizada. | HU01 |
| RF03 | Mostrar horarios, tarifas, tipo de vehículo y disponibilidad de cada servicio. | HU02 |
| RF04 | Permitir seleccionar un servicio y consultar sus asientos o cupos disponibles. | HU02, HU03 |

### 2.2. Reservas y pasajes

| ID | Requisito funcional | Historia |
|---|---|---|
| RF05 | Permitir registrar una reserva asociada a un pasajero y un viaje programado. | HU03 |
| RF06 | Permitir seleccionar un asiento numerado o un cupo, según la modalidad del servicio. | HU03 |
| RF07 | Impedir la asignación duplicada de un asiento o la sobreventa de cupos en un mismo viaje. | HU03 |
| RF08 | Gestionar reservas temporales con vencimiento y liberación de disponibilidad. | HU03 |
| RF09 | Permitir consultar las reservas propias y sus estados. | HU05 |
| RF10 | Permitir solicitar cancelaciones y reprogramaciones según las condiciones aplicables. | HU06 |
| RF11 | Actualizar la disponibilidad al confirmar, vencer, cancelar o reprogramar una reserva. | HU03, HU06 |
| RF12 | Generar un comprobante o pasaje digital asociado a una venta confirmada. | HU04, HU05 |

### 2.3. Pagos y ventas presenciales

| ID | Requisito funcional | Historia |
|---|---|---|
| RF13 | Permitir iniciar pagos electrónicos mediante una pasarela externa. | HU04 |
| RF14 | Verificar la confirmación del pago antes de registrar una reserva como pagada. | HU04 |
| RF15 | Permitir al agente consultar viajes, tarifas y disponibilidad. | HU10 |
| RF16 | Permitir registrar reservas, ventas y cobros presenciales autorizados. | HU11 |
| RF17 | Identificar al agente y la agencia responsables de cada venta presencial. | HU11 |
| RF18 | Permitir gestionar reservas dentro de los permisos del agente y las condiciones del servicio. | HU12 |
| RF19 | Evitar que una misma confirmación de pago se procese más de una vez. | HU04 |

### 2.4. Administración de empresas y agencias

| ID | Requisito funcional | Historia |
|---|---|---|
| RF20 | Registrar empresas con nombre comercial, razón social, RUC, logo, fotografías, dirección, ubicación y datos de contacto. | HU21 |
| RF21 | Registrar y administrar agencias asociadas a cada empresa. | HU22 |
| RF22 | Activar, suspender y actualizar empresas y agencias. | HU22 |
| RF23 | Permitir al administrador gestionar los datos operativos de su empresa y agencias autorizadas. | HU14 |

### 2.5. Gestión de rutas, vehículos y programación

| ID | Requisito funcional | Historia |
|---|---|---|
| RF24 | Registrar y administrar orígenes, destinos, rutas, recorridos y paradas aplicables. | HU15 |
| RF25 | Configurar tarifas de pasajes por ruta o servicio de cada empresa. | HU15 |
| RF26 | Registrar vehículos con placa, tipo, capacidad, distribución de asientos y estado operativo. | HU16 |
| RF27 | Permitir configurar modalidades de asignación por asiento numerado o cupo. | HU16 |
| RF28 | Programar viajes asociando empresa, ruta, fecha, horarios, tarifa y vehículo cuando corresponda. | HU17 |
| RF29 | Permitir modificar la programación identificando reservas afectadas y conservando trazabilidad. | HU17 |

### 2.6. Gestión de encomiendas

| ID | Requisito funcional | Historia |
|---|---|---|
| RF30 | Mostrar empresas que ofrecen encomiendas para un origen y destino. | HU07 |
| RF31 | Mostrar tarifas, condiciones y restricciones de envío de cada empresa. | HU07 |
| RF32 | Permitir solicitar una encomienda con remitente, destinatario, origen, destino y características del paquete. | HU08 |
| RF33 | Registrar encomiendas con descripción, peso y demás datos exigidos por la política de la empresa. | HU08, HU13 |
| RF34 | Generar un código único de seguimiento por encomienda. | HU08, HU09 |
| RF35 | Permitir consultar el estado de una encomienda mediante su código de seguimiento, protegiendo los datos personales. | HU09 |
| RF36 | Permitir al agente registrar la recepción, cobro, despacho, llegada y entrega de encomiendas. | HU13 |
| RF37 | Permitir al administrador configurar rutas habilitadas, tarifas y políticas de encomiendas de su empresa. | HU19 |
| RF38 | Validar las condiciones de la empresa antes de aceptar una encomienda. | HU08, HU19 |
| RF39 | Registrar los cambios de estado y la evidencia de entrega de las encomiendas. | HU13 |

### 2.7. Usuarios, permisos y seguridad

| ID | Requisito funcional | Historia |
|---|---|---|
| RF40 | Permitir la autenticación de usuarios para acceder a funcionalidades privadas. | HU05, HU11, HU14, HU23 |
| RF41 | Gestionar usuarios y permisos según los cuatro roles establecidos. | HU18, HU23 |
| RF42 | Restringir el acceso a información y operaciones según la empresa y agencia autorizadas. | HU18, HU23 |
| RF43 | Permitir al administrador gestionar vendedores dentro de su ámbito autorizado. | HU18 |
| RF44 | Registrar auditoría de las operaciones críticas. | HU24 |

### 2.8. Reportes y notificaciones

| ID | Requisito funcional | Historia |
|---|---|---|
| RF45 | Generar reportes de ventas, reservas, ocupación y encomiendas por empresa y agencia autorizada. | HU20 |
| RF46 | Permitir consultar reportes generales de la plataforma y registros de auditoría. | HU24 |
| RF47 | Generar notificaciones de confirmaciones, cancelaciones y cambios relevantes de reservas. | HU04, HU05, HU06 |
| RF48 | Generar notificaciones sobre eventos relevantes de encomiendas. | HU08, HU09 |

## 3. Reglas de negocio

| ID | Regla |
|---|---|
| RN01 | Una empresa puede tener varias agencias y administrar sus servicios de forma independiente. |
| RN02 | Solo existen cuatro roles humanos: Pasajero/Cliente, Agente/Vendedor, Administrador de Agencia y Superadministrador. |
| RN03 | Los administradores y agentes solo pueden operar dentro de sus empresas y agencias autorizadas. |
| RN04 | Un viaje programado pertenece a una empresa y tiene una ruta, fecha y horario definidos. |
| RN05 | La disponibilidad se controla por viaje programado y por asiento o cupo, según corresponda. |
| RN06 | Un asiento no puede confirmarse en dos reservas del mismo viaje ni superarse la capacidad de cupos. |
| RN07 | Una reserva temporal vigente debe retener su asiento o cupo hasta su confirmación o vencimiento. |
| RN08 | Una reserva solo puede marcarse como pagada después de verificar el pago correspondiente. |
| RN09 | Las cancelaciones y reprogramaciones deben respetar las condiciones aplicables al servicio. |
| RN10 | Cada empresa determina si ofrece encomiendas y establece sus rutas, tarifas y políticas. |
| RN11 | Una encomienda solo puede aceptarse si cumple las condiciones vigentes de la empresa. |
| RN12 | Cada encomienda debe tener un código de seguimiento único. |
| RN13 | La entrega de una encomienda debe registrar fecha, responsable y verificación de entrega. |
| RN14 | Las encomiendas no deben alterar la disponibilidad de asientos de pasajeros. |
| RN15 | Las ventas presenciales deben identificar al agente y la agencia responsables. |
| RN16 | Las operaciones críticas deben conservar trazabilidad. |

## 4. Requisitos pendientes de parametrización

Antes de implementar los procesos correspondientes, deben definirse:

- Tiempo máximo de vigencia de una reserva temporal.
- Condiciones de cancelación, reprogramación y devolución.
- Medios de cobro presencial y procedimiento de confirmación.
- Límites de peso, dimensiones, artículos restringidos y condiciones de entrega de encomiendas.
- Modalidades de tarifas y cálculo del costo de envío.
- Procedimiento de recepción y verificación de entrega.

Estos parámetros no deben establecerse arbitrariamente, ya que pueden depender de las políticas de cada empresa.

## 5. Trazabilidad

Los requisitos RF01-RF48 se relacionan con las historias HU01-HU24.

Las reglas RN01-RN16 complementan los requisitos funcionales y deben considerarse en el diseño, implementación y pruebas.

Los atributos de calidad, escenarios y decisiones arquitectónicas se documentarán posteriormente, tomando como referencia estos requisitos.
