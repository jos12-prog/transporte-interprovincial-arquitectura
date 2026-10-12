
# Principios de Diseño

## 1. Propósito

Este documento establece los principios de diseño que orientarán el desarrollo de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Su aplicación busca mantener un código comprensible, seguro, modular y fácil de mantener, de acuerdo con la arquitectura en capas, el monolito modular y Clean Architecture definidos para el proyecto.

Los principios se aplicarán principalmente al backend desarrollado con ASP.NET Core y C#.

## 2. Principios seleccionados

| Código | Principio | Aplicación en el proyecto |
|---|---|---|
| PR01 | Responsabilidad Única (SRP) | Separar las responsabilidades de controladores, casos de uso, dominio y persistencia. |
| PR02 | Abierto/Cerrado (OCP) | Incorporar nuevas reglas de disponibilidad sin modificar las implementaciones existentes. |
| PR03 | Sustitución de Liskov (LSP) | Permitir sustituir implementaciones que respeten un mismo contrato. |
| PR04 | Segregación de Interfaces (ISP) | Definir contratos específicos para cada necesidad funcional. |
| PR05 | Inversión de Dependencias (DIP) | Evitar que los casos de uso dependan de tecnologías concretas. |
| PR06 | DRY | Evitar duplicar reglas críticas del negocio. |
| PR07 | KISS | Priorizar soluciones simples y comprensibles. |
| PR08 | YAGNI | Evitar implementar funcionalidades e infraestructura sin una necesidad identificada. |

## 3. Aplicación de los principios SOLID

### 3.1. PR01 — Responsabilidad Única (SRP)

Cada clase o componente debe tener una responsabilidad claramente definida.

**Aplicación: módulo Reservas y Pasajes**

- `ReservationsController`: recibe solicitudes HTTP y devuelve respuestas.
- `CreateReservation`: coordina el caso de uso de creación de reservas.
- `Reservation`: representa la reserva y sus reglas de estado.
- `ReservationRepository`: implementa las operaciones de persistencia.

**Criterio:** modificar la tecnología de persistencia no debe requerir cambios en las reglas de negocio de la entidad Reserva.

### 3.2. PR02 — Abierto/Cerrado (OCP)

Los componentes deben permitir extender comportamientos sin modificar innecesariamente el código existente.

**Aplicación: disponibilidad de servicios**

La plataforma contempla dos modalidades:

- Asientos numerados.
- Cupos según capacidad.

Ambas pueden implementar el contrato `IAvailabilityStrategy`, permitiendo incorporar nuevas modalidades cuando exista una necesidad real.

**Criterio:** agregar una estrategia de disponibilidad no debe obligar a modificar las reglas de las demás estrategias.

### 3.3. PR03 — Sustitución de Liskov (LSP)

Las implementaciones de una interfaz deben respetar el comportamiento establecido por su contrato.

**Aplicación: integración de pagos**

Los adaptadores que implementen `IPaymentGateway` deben devolver resultados coherentes y verificables, independientemente del proveedor externo utilizado.

**Criterio:** cambiar el adaptador de pagos no debe alterar las reglas de confirmación de una reserva.

### 3.4. PR04 — Segregación de Interfaces (ISP)

Los componentes deben depender únicamente de las operaciones que necesitan.

**Aplicación: persistencia y servicios externos**

Se utilizarán contratos específicos, como:

- `IReservationRepository`: persistencia de reservas.
- `IAvailabilityService`: disponibilidad de servicios.
- `IPaymentGateway`: integración con pagos.
- `INotificationService`: envío de notificaciones.

**Criterio:** un módulo no debe implementar métodos ajenos a su responsabilidad.

### 3.5. PR05 — Inversión de Dependencias (DIP)

Los casos de uso deben depender de abstracciones y no de implementaciones tecnológicas.

**Aplicación: módulo Reservas y Pasajes**

`CreateReservation` utiliza `IReservationRepository`, mientras que `ReservationRepository` implementa ese contrato mediante Entity Framework Core y PostgreSQL.

La configuración de ASP.NET Core conecta ambas implementaciones mediante inyección de dependencias.

**Criterio:** los casos de uso no deben acceder directamente a `DbContext` ni a servicios externos concretos.

## 4. Principios complementarios

### 4.1. PR06 — DRY

Las reglas de negocio deben tener una implementación claramente identificada para evitar inconsistencias.

**Aplicación:**
- Centralizar las reglas de transición de estados de reservas.
- Reutilizar los controles de autorización por empresa y agencia.
- Evitar duplicar la validación de disponibilidad en distintos controladores.

La reutilización debe respetar los límites entre módulos y no generar dependencias innecesarias.

### 4.2. PR07 — KISS

Se priorizarán implementaciones sencillas que satisfagan los requerimientos establecidos.

**Aplicación:**
- Mantener un monolito modular en lugar de adoptar microservicios.
- Utilizar comunicación interna entre módulos.
- Emplear las capacidades de ASP.NET Core y PostgreSQL antes de introducir tecnologías adicionales.

### 4.3. PR08 — YAGNI

No se incorporarán funcionalidades o componentes sin una necesidad identificada.

**Aplicación:**
- Redis se considera una optimización futura, no una dependencia inicial.
- Las colas de eventos podrán incorporarse cuando los procesos asíncronos lo justifiquen.
- No se requiere Kubernetes para la primera versión.

Estas decisiones podrán revisarse cuando las mediciones de rendimiento o las necesidades operativas lo requieran.

## 5. Reglas generales de implementación

| Código | Regla |
|---|---|
| RG01 | Los controladores deben delegar las operaciones a los casos de uso. |
| RG02 | Las reglas del negocio deben permanecer independientes de frameworks y bases de datos. |
| RG03 | Los módulos deben comunicarse mediante contratos internos definidos. |
| RG04 | La infraestructura debe implementar las interfaces requeridas por la aplicación. |
| RG05 | La autorización debe considerar el rol y el contexto de empresa y agencia. |
| RG06 | La asignación de asientos y cupos debe protegerse mediante transacciones y control de concurrencia. |
| RG07 | Las confirmaciones de pago deben verificarse y procesarse de manera idempotente. |
| RG08 | Las operaciones de encomiendas deben mantener su trazabilidad sin depender de la disponibilidad de asientos. |

## 6. Verificación de los principios

Durante el desarrollo y la revisión del código se comprobará lo siguiente:

| Aspecto | Criterio de verificación |
|---|---|
| Separación de responsabilidades | Los controladores no contienen lógica de negocio ni consultas directas a PostgreSQL. |
| Independencia del dominio | Las entidades no dependen de ASP.NET Core ni Entity Framework Core. |
| Inversión de dependencias | Los casos de uso utilizan contratos para acceder a persistencia e integraciones. |
| Modularidad | Un módulo no modifica directamente los datos privados de otro. |
| Seguridad | Las operaciones verifican permisos y alcance de empresa o agencia. |
| Consistencia | Las pruebas concurrentes no producen sobreventa. |
| Idempotencia | Una notificación de pago repetida no duplica la confirmación. |
| Mantenibilidad | Los cambios de infraestructura no obligan a modificar las reglas del dominio. |

## 7. Relación con los drivers arquitectónicos

| Driver | Contribución de los principios |
|---|---|
| DA01 — Consistencia | Centralización de reglas y separación de operaciones transaccionales. |
| DA02 — Seguridad multiempresa | Responsabilidades claras y controles de autorización reutilizables. |
| DA03 — Rendimiento | Implementaciones optimizables sin alterar el dominio. |
| DA04 — Disponibilidad y recuperación | Componentes delimitados que facilitan el diagnóstico y la recuperación. |
| DA05 — Escalabilidad | Diseño modular que admite evolución y réplicas del backend sin estado. |
| DA06 — Mantenibilidad | Aplicación de SOLID, DRY, KISS y YAGNI para reducir acoplamiento y complejidad. |

## 8. Conclusión

Los principios SOLID, DRY, KISS y YAGNI orientan el diseño interno de la plataforma, estableciendo criterios concretos para separar responsabilidades, controlar dependencias y evitar complejidad innecesaria.

Su aplicación permitirá desarrollar los módulos de reservas, transporte, pagos y encomiendas de forma consistente con las decisiones arquitectónicas del proyecto.
