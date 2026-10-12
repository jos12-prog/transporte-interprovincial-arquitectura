
# 05. Restricciones del Sistema

## 1. Descripción

Las restricciones establecen las condiciones y límites que deben respetarse durante el análisis, diseño e implementación de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Estas restricciones se derivan del alcance funcional y de las reglas de operación del sistema.

## 2. Restricciones del proyecto

| ID | Restricción | Justificación |
|---|---|---|
| RES01 | El sistema contempla cuatro actores humanos: Pasajero/Cliente, Agente/Vendedor, Administrador de Agencia y Superadministrador. | Mantener los roles definidos en el análisis. |
| RES02 | La plataforma debe permitir gestionar múltiples empresas y agencias. | Integrar diferentes operadores de transporte. |
| RES03 | Cada empresa debe administrar sus servicios, tarifas y operaciones de manera independiente. | Evitar interferencias entre operadores. |
| RES04 | Los usuarios internos solo podrán acceder a empresas y agencias autorizadas. | Proteger la información y las operaciones. |
| RES05 | El sistema debe admitir vehículos con asientos numerados o cupos, según la modalidad del servicio. | Adaptarse a distintos tipos de transporte. |
| RES06 | Las reservas no deben generar duplicación de asientos ni sobreventa de cupos. | Garantizar la integridad de las ventas. |
| RES07 | Una reserva pagada electrónicamente requiere confirmación verificable de la pasarela de pago. | Evitar confirmaciones incorrectas. |
| RES08 | El servicio de encomiendas será configurable por empresa, según sus rutas, tarifas y políticas. | Respetar las condiciones de cada operador. |
| RES09 | La gestión de encomiendas no debe modificar la disponibilidad de asientos de pasajeros. | Separar ambos procesos operativos. |
| RES10 | El sistema debe proteger los datos personales y conservar trazabilidad de operaciones críticas. | Garantizar seguridad y control. |

## 3. Restricciones de alcance

- El proyecto comprende reservas y ventas de pasajes, administración de empresas y agencias, programación de viajes y gestión de encomiendas.
- Los pagos electrónicos dependen de una pasarela externa.
- Las notificaciones pueden utilizar proveedores externos.
- Las políticas de cancelación, reprogramación y encomiendas dependen de las condiciones de cada empresa.
- No se incluye la operación física de los vehículos ni el seguimiento GPS en tiempo real como funcionalidad obligatoria.

## 4. Consideraciones tecnológicas

La elección del lenguaje de programación, framework, base de datos, infraestructura y mecanismos de despliegue corresponde a las decisiones arquitectónicas.

Por tanto, estas tecnologías no se consideran restricciones externas del proyecto mientras no exista una obligación institucional, contractual o de infraestructura que las imponga.

La arquitectura deberá priorizar una implementación inicial viable y permitir su evolución conforme aumente la demanda.

## 5. Relación con otros documentos

- **01-actores.md:** define los roles y responsabilidades.
- **02-historias-del-usuario.md:** describe las necesidades de los actores.
- **03-requisitos-funcionales.md:** especifica las funcionalidades y reglas de negocio.
- **04-atributos-de-calidad.md:** establece las características de calidad.
- **05-restricciones.md:** delimita las condiciones que debe respetar la solución.

Estas restricciones serán consideradas al definir los drivers y las decisiones arquitectónicas.
