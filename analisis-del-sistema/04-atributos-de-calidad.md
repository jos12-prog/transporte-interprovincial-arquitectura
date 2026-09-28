# Atributos de Calidad

## 1. Descripción

Los atributos de calidad establecen cómo debe comportarse la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial, además de las funcionalidades que proporciona.

Debido a que la plataforma deberá operar con múltiples empresas y agencias y soportar periodos de alta demanda, se consideran especialmente importantes el rendimiento, disponibilidad, escalabilidad, seguridad, consistencia, mantenibilidad y trazabilidad.

## 2. Atributos de calidad

| ID | Atributo de calidad | Escenario de calidad |
|---|---|---|
| **AC01** | Rendimiento | Las consultas de viajes, horarios y disponibilidad deben responder eficientemente incluso cuando exista una gran cantidad de pasajeros realizando consultas simultáneamente. |
| **AC02** | Disponibilidad | La plataforma debe mantenerse disponible durante periodos críticos como feriados, vacaciones y temporadas de alta demanda. |
| **AC03** | Escalabilidad | La arquitectura debe soportar el crecimiento progresivo del número de pasajeros, empresas, agencias, servicios y operaciones sin requerir modificaciones estructurales importantes. |
| **AC04** | Seguridad | El sistema debe proteger el acceso a la información y funcionalidades mediante autenticación y autorización de acuerdo con el rol, empresa y agencia asociados a cada usuario. |
| **AC05** | Consistencia e integridad | Ante solicitudes concurrentes sobre un mismo cupo o asiento, el sistema debe garantizar que únicamente una operación pueda confirmar su asignación, evitando sobreventa o duplicidad. |
| **AC06** | Mantenibilidad | La solución debe organizar sus responsabilidades de manera modular para facilitar modificaciones, correcciones y ampliaciones sin afectar innecesariamente otros componentes. |
| **AC07** | Trazabilidad | Las operaciones críticas, modificaciones, reservas y transacciones deben conservar información suficiente para identificar las acciones realizadas y facilitar procesos de auditoría. |
| **AC08** | Recuperación ante fallos | Las operaciones críticas deben mantener su integridad ante interrupciones y la solución debe considerar mecanismos de respaldo y recuperación. |

## 3. Escenario crítico de calidad

Uno de los escenarios de mayor importancia ocurre durante temporadas de alta demanda, cuando múltiples pasajeros pueden intentar reservar simultáneamente un mismo cupo o asiento.

En este escenario, la plataforma debe mantener tiempos de respuesta adecuados, conservar su disponibilidad y garantizar la consistencia de la información. Si dos o más usuarios intentan reservar el mismo cupo o asiento, únicamente una operación deberá poder confirmar la asignación.

Este escenario relaciona principalmente los atributos de rendimiento, disponibilidad, escalabilidad y consistencia.

## 4. Influencia sobre la arquitectura

Los atributos de calidad identificados condicionarán diferentes decisiones arquitectónicas. La escalabilidad y el rendimiento influirán en los mecanismos de distribución de carga, caché y procesamiento; la disponibilidad y recuperación ante fallos influirán en los mecanismos de monitoreo y respaldo; la seguridad requerirá mecanismos de autenticación y autorización; y la consistencia tendrá influencia directa sobre el manejo transaccional de las reservas y la disponibilidad.