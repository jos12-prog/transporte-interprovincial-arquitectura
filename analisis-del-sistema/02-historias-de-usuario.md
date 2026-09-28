# Historias de Usuario

## 1. Descripción

Las historias de usuario describen las principales necesidades de los actores que interactúan con la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Se utiliza la siguiente estructura:

**Como [actor], quiero [acción], para [beneficio].**

## 2. Historias de usuario

| ID | Actor | Historia de usuario |
|---|---|---|
| **HU01** | Pasajero | Como pasajero, quiero buscar viajes por origen, destino y fecha, para encontrar los servicios disponibles que se ajusten a mi necesidad. |
| **HU02** | Pasajero | Como pasajero, quiero consultar los horarios, tarifas y disponibilidad de cupos o asientos, para seleccionar una alternativa de viaje disponible. |
| **HU03** | Pasajero | Como pasajero, quiero reservar temporalmente un cupo o asiento, para mantenerlo disponible mientras completo el proceso de compra. |
| **HU04** | Pasajero | Como pasajero, quiero realizar el pago de mi pasaje, para confirmar la compra del servicio seleccionado. |
| **HU05** | Pasajero | Como pasajero, quiero consultar mis reservas y pasajes, para conocer el estado de mis operaciones y la información de mis viajes. |
| **HU06** | Pasajero | Como pasajero, quiero cancelar o solicitar la reprogramación de una reserva o pasaje según las condiciones del servicio, para gestionar cambios en mi viaje. |
| **HU07** | Pasajero | Como pasajero, quiero recibir confirmaciones y notificaciones sobre mi reserva o servicio, para mantenerme informado sobre compras, modificaciones, reprogramaciones o cancelaciones. |
| **HU08** | Agente/Vendedor | Como agente/vendedor, quiero consultar servicios y disponibilidad de cupos o asientos, para atender a los pasajeros desde mi agencia. |
| **HU09** | Agente/Vendedor | Como agente/vendedor, quiero registrar reservas y ventas, para gestionar las operaciones realizadas en mi punto de atención. |
| **HU10** | Administrador de Agencia | Como administrador de agencia, quiero gestionar rutas, horarios y servicios de mi agencia, para mantener actualizada su oferta de transporte. |
| **HU11** | Administrador de Agencia | Como administrador de agencia, quiero gestionar las unidades de transporte y su capacidad, para asociarlas a los servicios programados cuando corresponda. |
| **HU12** | Administrador de Agencia | Como administrador de agencia, quiero gestionar modificaciones y reprogramaciones de servicios, para mantener actualizada la programación de mi agencia. |
| **HU13** | Administrador de Agencia | Como administrador de agencia, quiero consultar reportes de ventas, demanda y ocupación, para analizar el comportamiento de los servicios de mi agencia. |
| **HU14** | Superadministrador | Como superadministrador, quiero registrar y administrar empresas de transporte, para controlar los operadores que participan en la plataforma. |
| **HU15** | Superadministrador | Como superadministrador, quiero registrar y administrar agencias asociadas a las empresas, para organizar los diferentes puntos de atención de la plataforma. |
| **HU16** | Superadministrador | Como superadministrador, quiero gestionar usuarios, roles y permisos, para controlar el acceso a las funcionalidades según la empresa y agencia correspondiente. |
| **HU17** | Superadministrador | Como superadministrador, quiero consultar indicadores y reportes globales, para supervisar el comportamiento general de la plataforma. |

## 3. Distribución de historias por actor

| Actor | Historias |
|---|---|
| Pasajero | HU01 – HU07 |
| Agente/Vendedor | HU08 – HU09 |
| Administrador de Agencia | HU10 – HU13 |
| Superadministrador | HU14 – HU17 |

## 4. Consideraciones

Las historias de usuario identificadas representan las principales necesidades funcionales de los actores humanos del sistema. Las interacciones con la pasarela de pago y el servicio de notificaciones serán consideradas posteriormente como requisitos funcionales e integraciones externas de la arquitectura.