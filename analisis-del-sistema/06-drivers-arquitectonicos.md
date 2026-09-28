# Drivers Arquitectónicos

## 1. Descripción

Los drivers arquitectónicos corresponden a los requisitos funcionales, atributos de calidad y restricciones que tienen una influencia significativa sobre las decisiones de diseño de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

## 2. Drivers identificados

| ID | Driver arquitectónico | Origen | ¿Por qué influye en la arquitectura? |
|---|---|---|---|
| **DA01** | El sistema debe evitar la asignación duplicada y la sobreventa ante reservas simultáneas. | RF06 / AC05 | Requiere mecanismos de control de concurrencia y consistencia transaccional sobre la disponibilidad de cupos o asientos. |
| **DA02** | El sistema debe mantener tiempos de respuesta adecuados durante periodos de alta demanda. | AC01 | Influye en el uso de caché, optimización de consultas y distribución de solicitudes. |
| **DA03** | La plataforma debe soportar el crecimiento progresivo de pasajeros, empresas, agencias y operaciones. | AC03 / RC08 | Influye en la separación de responsabilidades, escalamiento de componentes y distribución de carga. |
| **DA04** | El sistema debe mantenerse disponible durante periodos críticos de operación. | AC02 / AC08 | Influye en los mecanismos de monitoreo, respaldo y recuperación ante fallos. |
| **DA05** | El sistema debe controlar el acceso según rol, empresa y agencia. | RF22 / AC04 / RC08 | Requiere mecanismos centralizados de autenticación, autorización y control de permisos. |
| **DA06** | La plataforma debe integrarse con una pasarela de pago externa. | RF07 / RC05 | Condiciona la comunicación con servicios externos y el tratamiento de respuestas y fallos durante el proceso de pago. |
| **DA07** | El sistema debe procesar notificaciones y tareas secundarias sin afectar las operaciones principales. | RF13 / AC01 | Puede justificar mecanismos de procesamiento asíncrono para desacoplar procesos secundarios. |
| **DA08** | El sistema debe registrar operaciones críticas para auditoría y trazabilidad. | RF23 / AC07 | Requiere mecanismos de registro, auditoría y monitoreo de operaciones. |
| **DA09** | La comunicación entre la interfaz web y los servicios debe realizarse mediante API REST. | RC03 | Condiciona el mecanismo de comunicación entre presentación y lógica de negocio. |

## 3. Drivers prioritarios

Los drivers de mayor impacto para la arquitectura son el control de concurrencia en las reservas, el rendimiento durante alta demanda, la escalabilidad, la disponibilidad y la seguridad.

Especialmente, el control de concurrencia constituye un driver crítico debido a que la plataforma manejará una cantidad limitada de cupos o asientos que pueden ser solicitados simultáneamente por diferentes pasajeros.

## 4. Relación con las decisiones arquitectónicas

Los drivers identificados justifican la incorporación de mecanismos y componentes como:

- balanceo y distribución de solicitudes;
- caché para información consultada frecuentemente;
- persistencia transaccional;
- control de concurrencia para reservas;
- autenticación y autorización;
- procesamiento asíncrono mediante cola de eventos;
- integración con servicios externos;
- auditoría, monitoreo y respaldo.

Estas decisiones serán representadas posteriormente en la arquitectura lógica y técnica de la solución.