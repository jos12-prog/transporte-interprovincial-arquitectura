# Restricciones del Sistema

## 1. Descripción

Las restricciones representan condiciones y decisiones que limitan o condicionan el diseño y desarrollo de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

## 2. Restricciones identificadas

| ID | Restricción | Descripción |
|---|---|---|
| **RC01** | Aplicación web | La plataforma debe ser accesible mediante una aplicación web desde navegadores modernos. |
| **RC02** | Control de versiones | La documentación y los artefactos del proyecto deben gestionarse mediante Git y mantenerse en GitHub. |
| **RC03** | API REST | La comunicación entre la interfaz web y los servicios de aplicación se realizará mediante una API REST. |
| **RC04** | PostgreSQL | La información transaccional será almacenada utilizando PostgreSQL, de acuerdo con la arquitectura técnica propuesta. |
| **RC05** | Pasarela de pago | La plataforma deberá integrarse con una pasarela de pago externa para procesar las operaciones de pago. |
| **RC06** | Servicio de notificaciones | La plataforma deberá integrarse con servicios externos para el envío de notificaciones mediante los medios electrónicos disponibles. |
| **RC07** | Plataforma multiempresa | La solución debe permitir integrar diferentes empresas y agencias manteniendo su independencia operativa. |
| **RC08** | Control de acceso | El acceso a las funcionalidades deberá controlarse según rol, empresa y agencia. |
| **RC09** | Incorporación progresiva | La arquitectura debe permitir incorporar nuevas empresas y agencias sin afectar la operación existente. |

## 3. Impacto de las restricciones

Las restricciones identificadas condicionan la arquitectura de la solución. El carácter multiempresa requiere separar adecuadamente el acceso a la información; PostgreSQL condiciona la tecnología de persistencia; la pasarela de pago y los servicios de notificación requieren mecanismos de integración externa; mientras que la caché y el procesamiento asíncrono forman parte de la estrategia propuesta para responder a escenarios de crecimiento y alta demanda.