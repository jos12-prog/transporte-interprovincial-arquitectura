# Escenarios de Atributos de Calidad

Los escenarios de atributos de calidad describen situaciones concretas y verificables que permitirán evaluar el comportamiento de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial en Ayacucho.

Cada escenario identifica la fuente del estímulo, el evento que ocurre, el componente afectado, las condiciones de ejecución, la respuesta esperada y una medida cuantificable para su posterior validación mediante pruebas.

Los valores cuantitativos establecidos son objetivos iniciales y podrán ser ajustados cuando se disponga del entorno definitivo de pruebas.

---

## EQ-01. Rendimiento: consulta de viajes

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Rendimiento |
| **Fuente del estímulo** | Pasajeros y agentes que consultan viajes disponibles. |
| **Estímulo** | Se realizan múltiples consultas simultáneas de rutas, horarios y servicios disponibles. |
| **Artefacto** | API REST, módulo de consulta de servicios, Redis y PostgreSQL. |
| **Entorno** | Operación normal y periodos de alta demanda, como feriados o festividades. |
| **Respuesta esperada** | El sistema devuelve los servicios disponibles sin errores y utiliza la caché cuando corresponda. |
| **Medida de la respuesta** | El 95 % de las consultas deberá responder en **2 segundos o menos**, con menos del **1 % de errores técnicos**, durante una prueba con al menos 200 usuarios concurrentes. |
| **Verificación** | Ejecutar pruebas de carga y registrar tiempos de respuesta, errores y usuarios concurrentes. |
| **Drivers relacionados** | DA01, DA06, DA11 |
| **Estado** | Pendiente de validación en el entorno de pruebas. |

---

## EQ-02. Concurrencia: reserva simultánea del mismo asiento

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Concurrencia y consistencia |
| **Fuente del estímulo** | Pasajeros y agentes que intentan reservar simultáneamente. |
| **Estímulo** | Dos o más usuarios intentan reservar el mismo asiento o cupo del mismo servicio al mismo tiempo. |
| **Artefacto** | Módulo de Reservas, módulo de Disponibilidad y PostgreSQL. |
| **Entorno** | Servicio con disponibilidad limitada y múltiples solicitudes concurrentes. |
| **Respuesta esperada** | El sistema permite confirmar el recurso únicamente a una operación válida y rechaza las demás sin generar sobreventa. |
| **Medida de la respuesta** | En el 100 % de las pruebas, un mismo asiento o cupo deberá tener como máximo **una reserva confirmada**. No deberán existir registros de sobreventa. |
| **Verificación** | Ejecutar pruebas concurrentes sobre el mismo recurso y comprobar el estado final de reservas y disponibilidad en PostgreSQL. |
| **Drivers relacionados** | DA01, DA02, DA03, DA07 |
| **Estado** | Pendiente de validación. |

---

## EQ-03. Seguridad: aislamiento entre empresas y agencias

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Seguridad y aislamiento de información |
| **Fuente del estímulo** | Administrador de Agencia o Agente/Vendedor. |
| **Estímulo** | Un usuario intenta consultar o modificar información perteneciente a otra empresa o agencia. |
| **Artefacto** | API REST, autenticación, autorización y módulos de negocio. |
| **Entorno** | Plataforma multiempresa con varias agencias registradas en una misma base de datos. |
| **Respuesta esperada** | El backend rechaza la operación no autorizada y no devuelve información perteneciente a otra organización. |
| **Medida de la respuesta** | El **100 % de los intentos no autorizados** incluidos en las pruebas deberán ser rechazados y no deberán exponer información de otra empresa o agencia. |
| **Verificación** | Ejecutar pruebas de autorización utilizando diferentes roles, empresas y agencias. |
| **Drivers relacionados** | DA04, DA05, DA06 |
| **Estado** | Pendiente de validación. |

---

## EQ-04. Disponibilidad: continuidad de las operaciones críticas

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Disponibilidad |
| **Fuente del estímulo** | Pasajeros, agentes y administradores. |
| **Estímulo** | Los usuarios intentan consultar o realizar operaciones durante periodos de alta demanda o ante una falla controlada de un componente secundario. |
| **Artefacto** | API REST, aplicación, PostgreSQL, Redis y RabbitMQ. |
| **Entorno** | Sistema publicado y monitoreado durante una operación normal o de alta demanda. |
| **Respuesta esperada** | El sistema mantiene disponibles las operaciones críticas que no dependen del componente afectado y comunica de forma controlada las operaciones temporalmente no disponibles. |
| **Medida de la respuesta** | Alcanzar una disponibilidad mensual objetivo de **99,5 %** para las operaciones críticas. |
| **Verificación** | Revisar registros de monitoreo, errores, interrupciones y tiempo total de disponibilidad. |
| **Drivers relacionados** | DA05, DA06, DA10, DA15 |
| **Estado** | Pendiente de validación en producción o entorno de simulación. |

---

## EQ-05. Reserva temporal: liberación automática del asiento

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Consistencia y gestión de recursos |
| **Fuente del estímulo** | Pasajero que inicia una reserva pero no completa el pago. |
| **Estímulo** | El pasajero selecciona un asiento y obtiene una reserva temporal sin completar el pago. |
| **Artefacto** | Módulo de Reservas, disponibilidad, PostgreSQL y mecanismo de expiración. |
| **Entorno** | El servicio tiene disponibilidad limitada y la reserva se encuentra en estado `HELD`. |
| **Respuesta esperada** | Después del periodo establecido, la reserva pasa a `EXPIRED` y el asiento vuelve a estar disponible. |
| **Medida de la respuesta** | La reserva deberá mantenerse durante **10 minutos** y liberarse automáticamente como máximo **30 segundos después de su expiración**. |
| **Verificación** | Crear una reserva temporal, esperar su vencimiento y comprobar el cambio de estado y la disponibilidad del asiento. |
| **Drivers relacionados** | DA02, DA03, DA08 |
| **Estado** | Pendiente de validación. |

---

## EQ-06. Pagos: consistencia ante respuesta del proveedor

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Integridad y confiabilidad |
| **Fuente del estímulo** | Pasajero que confirma el pago de una reserva. |
| **Estímulo** | Culqi procesa una operación y devuelve una respuesta de aprobación, rechazo o error. |
| **Artefacto** | Servicio de Pagos, Payment Adapter, módulo de Reservas y PostgreSQL. |
| **Entorno** | Operación normal o escenario donde el proveedor externo presenta demora o error. |
| **Respuesta esperada** | El sistema actualiza correctamente el estado del pago y de la reserva sin generar confirmaciones duplicadas. |
| **Medida de la respuesta** | El **100 % de las operaciones** utilizadas en las pruebas deberán mantener una relación consistente entre pago, reserva y pasaje. Las notificaciones duplicadas del proveedor no deberán generar una segunda confirmación. |
| **Verificación** | Simular aprobaciones, rechazos, errores y mensajes repetidos del proveedor y comprobar los estados finales. |
| **Drivers relacionados** | DA03, DA09 |
| **Estado** | Pendiente de validación con la integración real o simulada de Culqi. |

---

## EQ-07. Procesamiento asíncrono: notificaciones por WhatsApp

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Rendimiento y desacoplamiento |
| **Fuente del estímulo** | Sistema después de confirmar una reserva o detectar una condición de notificación. |
| **Estímulo** | Se genera un evento que requiere enviar una notificación al pasajero. |
| **Artefacto** | Servicio de Notificaciones, RabbitMQ y WhatsApp Business Cloud API. |
| **Entorno** | Operación normal o indisponibilidad temporal del servicio externo de WhatsApp. |
| **Respuesta esperada** | El sistema publica el evento en RabbitMQ sin bloquear la operación principal y procesa posteriormente el envío. |
| **Medida de la respuesta** | La confirmación de una reserva no deberá depender de la respuesta inmediata de WhatsApp. Los eventos deberán ser procesados correctamente en al menos el **99 %** de las pruebas, considerando reintentos ante fallos temporales. |
| **Verificación** | Simular envíos exitosos, fallidos y reintentos y verificar el estado de los mensajes en la cola. |
| **Drivers relacionados** | DA10, DA13 |
| **Estado** | Pendiente de validación. |

---

## EQ-08. Caché: reducción de consultas repetitivas

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Rendimiento |
| **Fuente del estímulo** | Pasajeros que consultan repetidamente rutas, horarios y servicios. |
| **Estímulo** | Se reciben múltiples solicitudes para consultar información que cambia con poca frecuencia. |
| **Artefacto** | API REST, Redis y PostgreSQL. |
| **Entorno** | Periodo de alta demanda con múltiples consultas concurrentes. |
| **Respuesta esperada** | Redis responde las consultas almacenadas en caché y evita consultas innecesarias a PostgreSQL. |
| **Medida de la respuesta** | En las consultas configuradas para caché, al menos el **80 %** de las solicitudes repetitivas deberán ser atendidas mediante Redis durante la prueba definida. |
| **Verificación** | Comparar solicitudes atendidas por Redis frente a consultas realizadas directamente a PostgreSQL. |
| **Drivers relacionados** | DA06, DA11 |
| **Estado** | Pendiente de validación. |

---

## EQ-09. Escalabilidad: incremento de usuarios concurrentes

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Escalabilidad |
| **Fuente del estímulo** | Incremento simultáneo de pasajeros y agentes. |
| **Estímulo** | La cantidad de solicitudes aumenta durante una festividad o periodo de alta demanda. |
| **Artefacto** | Balanceador, instancias del backend, Redis, RabbitMQ y PostgreSQL. |
| **Entorno** | Carga superior a la operación normal del sistema. |
| **Respuesta esperada** | El sistema permite incrementar las instancias de la aplicación sin modificar las reglas principales del negocio. |
| **Medida de la respuesta** | Al duplicar la capacidad de las instancias de aplicación, el sistema deberá mantener el objetivo de rendimiento definido en EQ-01 sin incremento significativo de errores. |
| **Verificación** | Ejecutar pruebas con diferentes cantidades de instancias y comparar tiempos de respuesta, errores y capacidad procesada. |
| **Drivers relacionados** | DA01, DA05, DA06, DA14 |
| **Estado** | Pendiente de validación. |

---

## EQ-10. Auditoría: trazabilidad de operaciones críticas

| Elemento | Descripción |
|---|---|
| **Atributo de calidad** | Auditabilidad |
| **Fuente del estímulo** | Usuario que ejecuta una operación administrativa o comercial. |
| **Estímulo** | Se crea, modifica, confirma o cancela una operación crítica. |
| **Artefacto** | Módulos de Reservas, Pagos, Servicios, Administración y sistema de auditoría. |
| **Entorno** | Operación normal de la plataforma. |
| **Respuesta esperada** | El sistema registra quién realizó la operación, sobre qué recurso, cuándo se ejecutó y cuál fue el resultado. |
| **Medida de la respuesta** | El **100 % de las operaciones críticas definidas** deberá generar un registro de auditoría con usuario, empresa, agencia, operación, fecha y resultado. |
| **Verificación** | Ejecutar las operaciones críticas y comprobar la existencia e integridad de sus registros de auditoría. |
| **Drivers relacionados** | DA04, DA12, DA13 |
| **Estado** | Pendiente de validación. |

---

# 11. Resumen de escenarios

| ID | Atributo | Principal riesgo evaluado | Medida principal |
|---|---|---|---|
| EQ-01 | Rendimiento | Lentitud en consultas | P95 ≤ 2 s |
| EQ-02 | Concurrencia | Sobreventa | 0 sobreventas |
| EQ-03 | Seguridad | Acceso entre agencias | 100 % rechazos no autorizados |
| EQ-04 | Disponibilidad | Interrupción del servicio | 99,5 % |
| EQ-05 | Consistencia | Asientos bloqueados indefinidamente | Expiración en 10 min + 30 s |
| EQ-06 | Integridad | Pagos/reservas inconsistentes | 0 confirmaciones duplicadas |
| EQ-07 | Asincronía | Fallo de WhatsApp bloqueando operaciones | ≥99 % eventos procesados |
| EQ-08 | Rendimiento | Sobrecarga de BD | ≥80 % consultas repetitivas desde caché |
| EQ-09 | Escalabilidad | Incremento de usuarios | Mantener objetivos de EQ-01 |
| EQ-10 | Auditoría | Falta de trazabilidad | 100 % operaciones críticas auditadas |

---

# 12. Relación con la arquitectura

Los escenarios definidos serán utilizados posteriormente para validar las decisiones arquitectónicas.

```text
Drivers arquitectónicos
          ↓
Decisiones arquitectónicas
          ↓
Arquitectura de solución
          ↓
Escenarios de calidad
          ↓
Pruebas
          ↓
Resultados medidos
```

De esta manera, la arquitectura no se plantea únicamente como una estructura conceptual, sino como una solución que deberá demostrar mediante pruebas que puede responder a los principales desafíos del sistema: **concurrencia, sobreventa, aislamiento multiempresa, rendimiento, disponibilidad, pagos, procesamiento asíncrono, caché, escalabilidad y trazabilidad**.