# Requisitos Funcionales

## 1. Descripción

Los requisitos funcionales especifican las funciones que debe proporcionar la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial para satisfacer las necesidades expresadas mediante las historias de usuario.

## 2. Requisitos funcionales

| ID | Requisito funcional |
|---|---|
| **RF01** | El sistema debe permitir buscar viajes disponibles según origen, destino y fecha. |
| **RF02** | El sistema debe permitir consultar horarios, tarifas y disponibilidad actualizada de cupos o asientos de los servicios disponibles. |
| **RF03** | El sistema debe permitir seleccionar y generar una reserva temporal sobre un cupo o asiento disponible cuando la modalidad del servicio lo requiera. |
| **RF04** | El sistema debe bloquear temporalmente el cupo o asiento reservado durante el periodo de vigencia establecido. |
| **RF05** | El sistema debe liberar automáticamente el cupo o asiento cuando una reserva temporal expire. |
| **RF06** | El sistema debe garantizar que un mismo cupo o asiento no pueda ser confirmado simultáneamente para más de un pasajero. |
| **RF07** | El sistema debe permitir procesar el pago de un pasaje mediante una pasarela de pago externa. |
| **RF08** | El sistema debe confirmar la compra después de validar satisfactoriamente el resultado del pago. |
| **RF09** | El sistema debe actualizar automáticamente la disponibilidad después de confirmar, cancelar o expirar una reserva. |
| **RF10** | El sistema debe permitir al pasajero consultar sus reservas, pasajes y el estado de sus operaciones. |
| **RF11** | El sistema debe permitir cancelar reservas o pasajes de acuerdo con las condiciones establecidas para el servicio. |
| **RF12** | El sistema debe permitir gestionar la reprogramación de servicios y las reservas afectadas según las reglas establecidas. |
| **RF13** | El sistema debe enviar confirmaciones y notificaciones relacionadas con reservas, compras, modificaciones, reprogramaciones y cancelaciones. |
| **RF14** | El sistema debe permitir al agente/vendedor consultar servicios y disponibilidad correspondientes a su agencia. |
| **RF15** | El sistema debe permitir al agente/vendedor registrar reservas y ventas realizadas desde su punto de atención. |
| **RF16** | El sistema debe permitir registrar y gestionar rutas, horarios, tarifas y servicios asociados a una agencia. |
| **RF17** | El sistema debe permitir registrar y gestionar unidades de transporte y su capacidad cuando corresponda. |
| **RF18** | El sistema debe permitir registrar modificaciones de programación y conservar un historial de los cambios realizados. |
| **RF19** | El sistema debe permitir registrar, actualizar, activar y suspender empresas de transporte. |
| **RF20** | El sistema debe permitir registrar y gestionar agencias y puntos autorizados de atención asociados a cada empresa. |
| **RF21** | El sistema debe permitir gestionar usuarios, roles y permisos de acceso. |
| **RF22** | El sistema debe restringir el acceso a las funcionalidades según el rol, empresa y agencia asociados al usuario. |
| **RF23** | El sistema debe registrar las operaciones críticas realizadas por los usuarios para fines de auditoría y trazabilidad. |
| **RF24** | El sistema debe generar reportes sobre ventas, demanda, ocupación, rutas y comportamiento de los servicios. |
| **RF25** | El sistema debe permitir visualizar indicadores administrativos según los permisos del usuario. |

## 3. Relación entre historias de usuario y requisitos funcionales

| Historia de usuario | Requisitos funcionales relacionados |
|---|---|
| **HU01** – Buscar viajes | RF01 |
| **HU02** – Consultar información y disponibilidad | RF02 |
| **HU03** – Reservar temporalmente un cupo o asiento | RF03, RF04, RF05, RF06 |
| **HU04** – Realizar pago y compra | RF07, RF08, RF09 |
| **HU05** – Consultar reservas y pasajes | RF10 |
| **HU06** – Cancelar o reprogramar | RF11, RF12, RF09 |
| **HU07** – Recibir notificaciones | RF13 |
| **HU08** – Consultar servicios desde la agencia | RF14 |
| **HU09** – Registrar reservas y ventas | RF15, RF06, RF09 |
| **HU10** – Gestionar rutas, horarios y servicios | RF16 |
| **HU11** – Gestionar unidades de transporte | RF17 |
| **HU12** – Gestionar modificaciones y reprogramaciones | RF12, RF18 |
| **HU13** – Consultar reportes de agencia | RF24, RF25 |
| **HU14** – Gestionar empresas | RF19 |
| **HU15** – Gestionar agencias | RF20 |
| **HU16** – Gestionar usuarios y permisos | RF21, RF22 |
| **HU17** – Consultar indicadores globales | RF24, RF25 |

## 4. Trazabilidad

La relación entre historias de usuario y requisitos funcionales permite identificar qué funcionalidades del sistema responden a las necesidades de cada actor. Los requisitos relacionados con disponibilidad, reservas y concurrencia tendrán especial importancia en las decisiones arquitectónicas debido a su impacto sobre la consistencia de las operaciones.