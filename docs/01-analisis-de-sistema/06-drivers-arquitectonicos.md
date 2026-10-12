# 06. Drivers Arquitectónicos

## 1. Descripción

Los drivers arquitectónicos son los requisitos, atributos de calidad y restricciones que influyen directamente en las decisiones de arquitectura de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Su identificación permite priorizar los aspectos más importantes del sistema y justificar la estructura de la solución.

## 2. Drivers arquitectónicos identificados

| ID | Driver arquitectónico | Prioridad | Atributo relacionado |
|---|---|---|---|
| DA01 | Consistencia de reservas y pagos | Crítica | AC01 |
| DA02 | Seguridad y aislamiento multiempresa | Alta | AC02 |
| DA03 | Rendimiento de consultas y disponibilidad | Alta | AC03 |
| DA04 | Disponibilidad y recuperación | Alta | AC04, AC08 |
| DA05 | Escalabilidad del sistema | Alta | AC05 |
| DA06 | Mantenibilidad y evolución modular | Alta | AC06, AC09 |

## 3. Descripción de los drivers

### DA01 — Consistencia de reservas y pagos

**Necesidad:** Evitar reservas duplicadas, sobreventa de asientos o cupos y confirmaciones incorrectas de pagos.

**Implicación arquitectónica:** Requiere operaciones transaccionales, control de concurrencia y verificación de pagos.

**Trazabilidad:** RF05-RF11, RF13-RF14, RF19; AC01; RES06-RES07.

### DA02 — Seguridad y aislamiento multiempresa

**Necesidad:** Garantizar que cada empresa y agencia acceda únicamente a la información y operaciones autorizadas.

**Implicación arquitectónica:** Requiere autenticación, autorización por rol y validación del ámbito organizacional en las operaciones.

**Trazabilidad:** RF20-RF23, RF40-RF44; AC02; RES02-RES04, RES10.

### DA03 — Rendimiento de consultas y disponibilidad

**Necesidad:** Permitir búsquedas eficientes de empresas, rutas, horarios, tarifas y disponibilidad.

**Implicación arquitectónica:** Requiere consultas optimizadas, índices adecuados y mecanismos de optimización según la demanda.

**Trazabilidad:** RF01-RF04, RF30-RF31; AC03.

### DA04 — Disponibilidad y recuperación

**Necesidad:** Mantener los servicios esenciales operativos y recuperar información ante fallos.

**Implicación arquitectónica:** Requiere manejo de errores, monitoreo, respaldos y procedimientos de recuperación.

**Trazabilidad:** RF05-RF19, RF30-RF39; AC04, AC08.

### DA05 — Escalabilidad del sistema

**Necesidad:** Permitir el crecimiento de empresas, agencias, usuarios, viajes y encomiendas.

**Implicación arquitectónica:** Requiere una solución que permita ampliar la capacidad de procesamiento y almacenamiento de forma progresiva.

**Trazabilidad:** RF01-RF04, RF20-RF39; AC05; RES02.

### DA06 — Mantenibilidad y evolución modular

**Necesidad:** Facilitar modificaciones en reservas, pagos, transporte y encomiendas sin afectar innecesariamente otras funcionalidades.

**Implicación arquitectónica:** Requiere separación de responsabilidades, módulos definidos e interfaces claras entre componentes.

**Trazabilidad:** RF05-RF48; AC06, AC09; RES08-RES09.

## 4. Driver prioritario

El driver principal es **DA01 — Consistencia de reservas y pagos**, debido a que la asignación incorrecta de asientos o cupos puede generar sobreventas y conflictos operativos.

Este driver condiciona el diseño de las transacciones, el control de concurrencia y la confirmación de pagos.

DA02 también es fundamental porque la plataforma integra múltiples empresas que deben operar de manera independiente.

## 5. Relación con la arquitectura

Los drivers identificados servirán como base para:

- Definir y justificar las decisiones arquitectónicas.
- Seleccionar el estilo y enfoque arquitectónico.
- Diseñar los componentes principales del sistema.
- Establecer escenarios de calidad y pruebas de validación.

Las soluciones tecnológicas específicas se documentarán en las decisiones arquitectónicas.
