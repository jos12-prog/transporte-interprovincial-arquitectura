# Restricciones del Sistema

## 1. Descripción

Las restricciones representan condiciones y decisiones que limitan o condicionan el diseño y desarrollo de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

## 2. Restricciones identificadas

| ID | Restricción | Descripción |
|---|---|---|
| **RC01** | Plataforma centralizada multiempresa | La solución debe integrar diferentes empresas y agencias de transporte dentro de una misma plataforma, manteniendo la independencia operativa de cada organización. |
| **RC02** | Control de acceso | El acceso a las funcionalidades debe realizarse según el rol, empresa y agencia asociados a cada usuario. |
| **RC03** | Base de datos PostgreSQL | La información transaccional del sistema será almacenada utilizando PostgreSQL, de acuerdo con la arquitectura técnica propuesta. |
| **RC04** | Integración con pasarela de pago | La plataforma deberá utilizar un proveedor externo para procesar las operaciones de pago. |
| **RC05** | Servicios externos de notificación | El envío de notificaciones deberá integrarse con medios electrónicos disponibles, como correo electrónico, SMS o notificaciones push. |
| **RC06** | Procesamiento asíncrono | La solución deberá considerar un mecanismo de mensajería o cola de eventos para ejecutar procesos asíncronos como notificaciones, tareas secundarias y generación de reportes. |
| **RC07** | Uso de caché | La arquitectura deberá considerar un mecanismo de caché, como Redis o un servicio equivalente, para información consultada frecuentemente. |
| **RC08** | Incorporación progresiva | La arquitectura debe permitir incorporar nuevas empresas y agencias sin afectar la operación existente del sistema. |
| **RC09** | Control de versiones | La documentación y los artefactos del proyecto deberán gestionarse mediante Git y mantenerse en un repositorio GitHub. |

## 3. Impacto de las restricciones

Las restricciones identificadas condicionan la arquitectura de la solución. El carácter multiempresa requiere separar adecuadamente el acceso a la información; PostgreSQL condiciona la tecnología de persistencia; la pasarela de pago y los servicios de notificación requieren mecanismos de integración externa; mientras que la caché y el procesamiento asíncrono forman parte de la estrategia propuesta para responder a escenarios de crecimiento y alta demanda.