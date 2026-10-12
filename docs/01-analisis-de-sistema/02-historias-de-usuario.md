

# 02. Historias de Usuario

## 1. Descripción

Las historias de usuario describen las necesidades de los cuatro actores de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Constituyen la base para definir los requisitos funcionales, reglas de negocio y funcionalidades que serán implementadas.

## 2. Historias de usuario

### 2.1. Pasajero / Cliente (ACT01)

| ID | Historia de usuario | Prioridad |
|---|---|---|
| HU01 | Como pasajero, quiero buscar viajes por origen, destino y fecha, visualizando las empresas que ofrecen el servicio, para comparar alternativas. | Alta |
| HU02 | Como pasajero, quiero consultar horarios, tarifas, tipos de vehículo y disponibilidad para seleccionar un viaje. | Alta |
| HU03 | Como pasajero, quiero seleccionar un asiento o cupo disponible y reservarlo para asegurar mi viaje. | Crítica |
| HU04 | Como pasajero, quiero pagar electrónicamente para confirmar la compra de mi pasaje. | Alta |
| HU05 | Como pasajero, quiero consultar mis reservas y pasajes para conocer su estado y detalles. | Alta |
| HU06 | Como pasajero, quiero solicitar cancelaciones o reprogramaciones según las condiciones del servicio. | Media |
| HU07 | Como cliente, quiero consultar las empresas que realizan envíos hacia un destino y conocer sus condiciones y tarifas. | Alta |
| HU08 | Como cliente, quiero solicitar una encomienda indicando remitente, destinatario y características del paquete. | Alta |
| HU09 | Como cliente, quiero consultar el estado de mi encomienda mediante un código de seguimiento. | Alta |

### 2.2. Agente / Vendedor (ACT02)

| ID | Historia de usuario | Prioridad |
|---|---|---|
| HU10 | Como agente, quiero consultar viajes, tarifas y disponibilidad para atender a los pasajeros. | Alta |
| HU11 | Como agente, quiero registrar reservas, ventas presenciales y cobros autorizados para atender a los clientes. | Crítica |
| HU12 | Como agente, quiero consultar, cancelar o reprogramar reservas según mis permisos y las condiciones aplicables. | Alta |
| HU13 | Como agente, quiero registrar, cobrar, recibir, despachar y entregar encomiendas para gestionar los envíos de mi agencia. | Alta |

### 2.3. Administrador de Agencia (ACT03)

| ID | Historia de usuario | Prioridad |
|---|---|---|
| HU14 | Como administrador, quiero gestionar la información operativa de mi empresa y sus agencias autorizadas. | Alta |
| HU15 | Como administrador, quiero administrar rutas, orígenes, destinos, recorridos y tarifas para configurar los servicios. | Alta |
| HU16 | Como administrador, quiero registrar vehículos por tipo, placa, capacidad y distribución de asientos. | Alta |
| HU17 | Como administrador, quiero programar viajes definiendo ruta, fecha, horario, tarifa y vehículo asignado. | Alta |
| HU18 | Como administrador, quiero gestionar vendedores y sus permisos dentro de mi empresa y agencias autorizadas. | Alta |
| HU19 | Como administrador, quiero configurar rutas, tarifas y políticas de encomiendas para ofrecer envíos según las condiciones de mi empresa. | Alta |
| HU20 | Como administrador, quiero consultar reportes de ventas, reservas, ocupación y encomiendas para supervisar las operaciones. | Alta |

### 2.4. Superadministrador (ACT04)

| ID | Historia de usuario | Prioridad |
|---|---|---|
| HU21 | Como superadministrador, quiero registrar empresas con sus datos legales, nombre comercial, logo, fotografías, ubicación y contacto. | Alta |
| HU22 | Como superadministrador, quiero administrar empresas, agencias y sus estados de activación para controlar su participación en la plataforma. | Alta |
| HU23 | Como superadministrador, quiero gestionar usuarios, roles y permisos generales para controlar el acceso al sistema. | Alta |
| HU24 | Como superadministrador, quiero consultar reportes generales y registros de auditoría para supervisar la plataforma. | Alta |

## 3. Criterios de aceptación principales

| ID | Historia | Criterio de aceptación |
|---|---|---|
| CA01 | HU01 | La búsqueda muestra únicamente servicios de empresas que operan para el origen, destino y fecha seleccionados. |
| CA02 | HU03 | Un mismo asiento o cupo no puede confirmarse en dos reservas del mismo viaje. |
| CA03 | HU04 | La reserva solo se registra como pagada después de verificar la confirmación del pago. |
| CA04 | HU11 | Las ventas presenciales identifican al agente y la agencia responsables. |
| CA05 | HU16 | Los vehículos permiten configurar su tipo, capacidad y asientos según la modalidad de servicio. |
| CA06 | HU17 | Un viaje programado queda asociado a una ruta, fecha, horario y empresa, y a un vehículo cuando corresponda. |
| CA07 | HU19 | Las condiciones y tarifas de encomiendas se configuran independientemente para cada empresa. |
| CA08 | HU09 | El cliente puede consultar el estado del envío sin acceder a datos personales de terceros. |
| CA09 | HU21 | La empresa registra su información identificativa, ubicación e imágenes correspondientes. |
| CA10 | HU23 | Los usuarios solo pueden acceder a funciones y datos autorizados según su rol y ámbito organizacional. |

## 4. Reglas generales

- RG01: El sistema contempla únicamente cuatro roles humanos.
- RG02: Cada empresa administra sus servicios, vehículos, tarifas y políticas de encomiendas de manera independiente.
- RG03: El Administrador de Agencia también asume la gestión operativa de su empresa.
- RG04: Los agentes operan únicamente dentro de sus agencias autorizadas.
- RG05: La disponibilidad de asientos y cupos debe mantenerse consistente durante reservas simultáneas.
- RG06: Las encomiendas deben cumplir las políticas vigentes de la empresa seleccionada.
- RG07: Las operaciones críticas deben mantener trazabilidad.
- RG08: Las cancelaciones y reprogramaciones están sujetas a las condiciones aplicables.

## 5. Trazabilidad

Las historias HU01-HU24 serán la base para los requisitos funcionales.

Los criterios de aceptación y las reglas generales permitirán verificar la implementación y orientar las decisiones arquitectónicas posteriores.
