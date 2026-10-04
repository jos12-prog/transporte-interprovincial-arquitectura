# Drivers Arquitectónicos

## 1. Descripción

Los drivers arquitectónicos corresponden a los requisitos funcionales, atributos de calidad y restricciones que tienen una influencia significativa sobre las decisiones de diseño de la **Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial**.

Estos drivers permiten justificar las principales decisiones de la arquitectura, especialmente aquellas relacionadas con concurrencia, rendimiento, escalabilidad, disponibilidad, seguridad, integración, procesamiento asíncrono, trazabilidad y mantenibilidad.

---

## 2. Drivers identificados

| ID | Driver arquitectónico | Origen | ¿Por qué influye en la arquitectura? |
|---|---|---|---|
| **DA01** | El sistema debe evitar la asignación duplicada y la sobreventa ante reservas simultáneas. | RF06 / AC05 | Requiere mecanismos de control de concurrencia y consistencia transaccional sobre la disponibilidad de cupos o asientos. |
| **DA02** | El sistema debe mantener tiempos de respuesta adecuados durante periodos de alta demanda. | AC01 | Influye en la optimización de consultas, uso de caché para información consultada frecuentemente y distribución eficiente de solicitudes. |
| **DA03** | La plataforma debe soportar el crecimiento progresivo de pasajeros, empresas, agencias y operaciones. | AC03 / RC09 | Influye en la modularidad del sistema, el balanceo de carga y la posibilidad de realizar escalamiento horizontal. |
| **DA04** | El sistema debe mantenerse disponible durante periodos críticos de operación. | AC02 / AC08 | Influye en la incorporación de mecanismos de monitoreo, respaldo, recuperación ante fallos y distribución de carga. |
| **DA05** | El sistema debe controlar el acceso según rol, empresa y agencia. | RF22 / AC04 / RC08 | Requiere mecanismos de autenticación, autorización, HTTPS y control de permisos. |
| **DA06** | La plataforma debe integrarse con una pasarela de pago externa. | RF07 / RC05 | Condiciona la comunicación con servicios externos y requiere desacoplar el núcleo del sistema del proveedor de pagos. |
| **DA07** | El sistema debe procesar notificaciones y tareas secundarias sin afectar las operaciones principales. | RF13 / AC01 | Justifica mecanismos de procesamiento asíncrono para desacoplar tareas secundarias mediante una cola de eventos. |
| **DA08** | El sistema debe registrar operaciones críticas para auditoría y trazabilidad. | RF23 / AC07 | Requiere mecanismos de registro, auditoría y monitoreo de las operaciones realizadas en la plataforma. |
| **DA09** | La comunicación entre la interfaz web y los servicios debe realizarse mediante API REST. | RC03 | Condiciona el mecanismo de comunicación entre la capa de presentación y la lógica de negocio. |
| **DA10** | El sistema debe permitir modificar o ampliar funcionalidades sin afectar innecesariamente otros módulos. | AC06 | Influye en la separación de responsabilidades, modularidad y control de dependencias internas mediante Clean Architecture. |

---

## 3. Drivers prioritarios

Los drivers de mayor impacto sobre la arquitectura son:

| Prioridad | Driver | Impacto principal |
|---|---|---|
| **Alta** | **DA01 – Consistencia y concurrencia** | Evitar reservas duplicadas y sobreventa. |
| **Alta** | **DA02 – Rendimiento** | Mantener respuestas adecuadas durante alta demanda. |
| **Alta** | **DA03 – Escalabilidad** | Permitir crecimiento y escalamiento horizontal. |
| **Alta** | **DA04 – Disponibilidad** | Mantener la plataforma operativa en periodos críticos. |
| **Alta** | **DA05 – Seguridad** | Proteger el acceso según roles, empresas y agencias. |
| **Alta** | **DA10 – Mantenibilidad** | Facilitar cambios mediante modularidad y Clean Architecture. |
| **Media** | **DA06 – Integración de pagos** | Desacoplar la plataforma del proveedor externo de pagos. |
| **Media** | **DA07 – Procesamiento asíncrono** | Evitar que tareas secundarias bloqueen operaciones principales. |
| **Media** | **DA08 – Trazabilidad** | Mantener registro de operaciones críticas. |
| **Media** | **DA09 – API REST** | Estandarizar la comunicación entre presentación y aplicación. |

### Driver crítico del sistema

El **DA01 – Consistencia y concurrencia** constituye uno de los drivers más importantes del proyecto debido a que múltiples pasajeros pueden intentar reservar simultáneamente un mismo cupo o asiento.

La arquitectura deberá garantizar que únicamente una operación pueda confirmar la asignación, evitando duplicidad de reservas y sobreventa.

---

## 4. Relación entre drivers y decisiones arquitectónicas

| Driver | Necesidad arquitectónica | Decisión o mecanismo relacionado |
|---|---|---|
| **DA01 – Consistencia** | Evitar reservas simultáneas sobre el mismo cupo. | Control de concurrencia y persistencia transaccional en PostgreSQL. |
| **DA02 – Rendimiento** | Reducir tiempos de respuesta y carga sobre la base de datos. | Caché Redis y optimización de consultas frecuentes. |
| **DA03 – Escalabilidad** | Soportar el crecimiento de usuarios y operaciones. | Monolito modular, balanceo de carga y escalamiento horizontal. |
| **DA04 – Disponibilidad** | Mantener el servicio durante periodos críticos. | Balanceador, monitoreo, respaldo y recuperación ante fallos. |
| **DA05 – Seguridad** | Proteger recursos y funcionalidades. | HTTPS, autenticación, autorización y control de permisos. |
| **DA06 – Integración de pagos** | Evitar dependencia directa del proveedor externo. | Interfaces y adaptadores para la pasarela de pago. |
| **DA07 – Procesamiento asíncrono** | Evitar bloquear operaciones principales. | Cola de eventos para notificaciones y tareas secundarias. |
| **DA08 – Trazabilidad** | Registrar operaciones críticas. | Auditoría, logs y monitoreo. |
| **DA09 – API REST** | Comunicar presentación y lógica de negocio. | API REST como mecanismo de comunicación. |
| **DA10 – Mantenibilidad** | Facilitar modificaciones y evolución del sistema. | Monolito modular + Clean Architecture. |

---

## 5. Relación con la arquitectura técnica

Los drivers arquitectónicos justifican la incorporación de los principales componentes y mecanismos de la solución:

- **DNS:** resolución del nombre de dominio y direccionamiento hacia el punto de entrada de la plataforma.
- **Firewall / HTTPS:** protección de las comunicaciones y control del acceso externo.
- **API Gateway / Balanceador de carga:** distribución de solicitudes y soporte al escalamiento horizontal.
- **Monolito modular:** organización de las funcionalidades en módulos con responsabilidades claramente separadas.
- **PostgreSQL:** persistencia transaccional y soporte para la consistencia de las operaciones.
- **Redis:** almacenamiento temporal de información consultada frecuentemente para mejorar el rendimiento.
- **Cola de eventos:** procesamiento asíncrono de notificaciones y tareas secundarias.
- **Pasarela de pago:** integración externa para el procesamiento de pagos.
- **Servicio de notificaciones:** envío de correo electrónico, SMS o notificaciones push.
- **Auditoría y monitoreo:** registro y supervisión de operaciones críticas.
- **Backup y recuperación:** protección y recuperación de la información ante fallos.

---

## 6. Síntesis

Los drivers identificados permiten establecer una relación directa entre las necesidades del sistema y las decisiones arquitectónicas.

La solución se orienta hacia un **monolito modular con capacidad de escalamiento horizontal**, utilizando **Clean Architecture como enfoque para organizar las dependencias internas**.

Los mecanismos de caché, balanceo, procesamiento asíncrono, persistencia transaccional, seguridad, monitoreo y recuperación complementan la arquitectura para responder a los atributos de calidad prioritarios del sistema.