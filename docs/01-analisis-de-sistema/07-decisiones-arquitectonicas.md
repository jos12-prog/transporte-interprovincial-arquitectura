
# 07. Decisiones Arquitectónicas

## 1. Descripción

Las decisiones arquitectónicas establecen las tecnologías, estructuras y mecanismos seleccionados para implementar la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

Estas decisiones responden a los drivers arquitectónicos DA01-DA06 y consideran una implementación inicial viable, con posibilidades de evolución.

## 2. Decisiones arquitectónicas

| ID | Decisión | Justificación | Drivers |
|---|---|---|---|
| ADR01 | Adoptar un monolito modular con arquitectura en capas y enfoque Clean Architecture. | Facilita la separación de responsabilidades y reduce la complejidad operativa inicial. | DA05, DA06 |
| ADR02 | Utilizar React, TypeScript y Vite para la aplicación web. | Permite desarrollar una interfaz modular, responsive y mantenible. | DA03, DA06 |
| ADR03 | Utilizar ASP.NET Core y C# para la API REST. | Proporciona una plataforma adecuada para implementar reglas de negocio, seguridad y operaciones transaccionales. | DA01, DA02, DA06 |
| ADR04 | Utilizar PostgreSQL como base de datos relacional principal. | Permite garantizar integridad referencial y gestionar transacciones concurrentes. | DA01, DA04 |
| ADR05 | Implementar autenticación y autorización por roles y ámbito organizacional. | Protege las operaciones y mantiene el aislamiento entre empresas y agencias. | DA02 |
| ADR06 | Controlar las reservas mediante transacciones y restricciones de base de datos. | Evita asignaciones duplicadas y sobreventa de asientos o cupos. | DA01 |
| ADR07 | Integrar los pagos mediante un proveedor externo con verificación e idempotencia. | Evita confirmaciones incorrectas o duplicadas. | DA01, DA04 |
| ADR08 | Separar las funcionalidades en módulos de negocio. | Facilita el mantenimiento y la evolución independiente de las funcionalidades. | DA06 |
| ADR09 | Utilizar almacenamiento de objetos para imágenes y archivos. | Evita almacenar archivos binarios directamente en la base de datos transaccional. | DA05, DA06 |
| ADR10 | Implementar respaldos, registros y monitoreo. | Permite detectar fallos, investigar operaciones y recuperar información. | DA04 |
| ADR11 | Diseñar el backend para permitir escalamiento horizontal progresivo. | Facilita aumentar la capacidad de procesamiento cuando crezca la demanda. | DA05 |
| ADR12 | Incorporar mecanismos de procesamiento secundario y caché cuando exista una necesidad comprobada. | Evita complejidad prematura y permite optimizar tareas no críticas. | DA03, DA05 |

## 3. Arquitectura seleccionada

### ADR01 — Estilo y organización arquitectónica

Se adopta una arquitectura con las siguientes características:

- **Estilo arquitectónico:** arquitectura en capas.
- **Estructura de implementación:** monolito modular.
- **Enfoque de diseño:** Clean Architecture.
- **Comunicación externa:** API REST mediante HTTPS.

Los módulos se ejecutarán inicialmente como una aplicación backend integrada, manteniendo responsabilidades y dependencias internas definidas.

No se adoptará una arquitectura de microservicios en la primera versión, debido a su mayor complejidad de despliegue, comunicación y mantenimiento.

### ADR02 y ADR03 — Tecnologías de aplicación

**Frontend:**
- React.
- TypeScript.
- Vite.
- Tailwind CSS y shadcn/ui.

**Backend:**
- ASP.NET Core.
- C#.
- API REST.
- Entity Framework Core para acceso a datos.

La aplicación web permitirá el acceso desde navegadores de computadoras, tabletas y teléfonos móviles.

### ADR04 y ADR06 — Persistencia y consistencia

PostgreSQL será la fuente principal de información para empresas, agencias, viajes, reservas, pagos y encomiendas.

Las operaciones críticas de reserva deberán utilizar:

- Transacciones de base de datos.
- Restricciones de integridad.
- Control de concurrencia.
- Validación de disponibilidad.
- Gestión del vencimiento de reservas temporales.

Para los servicios con asientos numerados se impedirá la asignación duplicada de un asiento en un mismo viaje.

Para los servicios por cupos se impedirá superar la capacidad disponible.

### ADR05 — Seguridad multiempresa

Se utilizará ASP.NET Core Identity para gestionar identidades y autenticación.

La autorización deberá verificar:

- Rol del usuario.
- Empresa autorizada.
- Agencia autorizada.
- Permiso requerido para la operación.

La separación de información entre empresas deberá aplicarse desde el backend y las consultas de datos, no únicamente desde la interfaz.

### ADR07 — Integración de pagos

La plataforma se integrará con una pasarela de pago externa.

La confirmación de una reserva pagada deberá realizarse únicamente después de verificar el resultado de la operación.

Se aplicará idempotencia para evitar procesar dos veces una misma notificación o confirmación de pago.

### ADR08 — Módulos del sistema

El backend se organizará inicialmente en los siguientes módulos:

1. Identidad y acceso.
2. Empresas y agencias.
3. Rutas, vehículos y programación.
4. Reservas y pasajes.
5. Ventas y pagos.
6. Encomiendas.
7. Reportes y auditoría.
8. Notificaciones e integraciones.

Estos módulos serán partes internas del monolito modular, no microservicios independientes.

### ADR09 — Almacenamiento de archivos

Los logos, fotografías y archivos asociados se almacenarán en un servicio de almacenamiento de objetos compatible con S3 o equivalente.

PostgreSQL almacenará las referencias y metadatos necesarios.

### ADR10 — Operación y recuperación

La solución incorporará:

- Registros de errores y eventos relevantes.
- Auditoría de operaciones críticas.
- Monitoreo básico de la aplicación.
- Respaldos periódicos de PostgreSQL.
- Procedimientos de restauración.

### ADR11 — Despliegue y escalabilidad

La aplicación se preparará para ejecutarse mediante contenedores Docker.

Un servidor proxy inverso, como Nginx, podrá gestionar el acceso HTTPS y distribuir solicitudes cuando se incorporen varias instancias del backend.

El escalamiento horizontal se realizará según las necesidades de capacidad, manteniendo PostgreSQL como fuente de verdad de las reservas.

### ADR12 — Optimización progresiva

En una evolución posterior podrán incorporarse:

- Redis para consultas frecuentes y datos temporales no autoritativos.
- Procesamiento en segundo plano para notificaciones y tareas secundarias.
- Mensajería duradera para eventos críticos cuando sea necesaria.
- Varias instancias del backend detrás de un balanceador de carga.

Estas herramientas no reemplazarán las transacciones ni las restricciones de PostgreSQL utilizadas para controlar reservas.

## 4. Implementación inicial y evolución

| Componente | Implementación inicial | Evolución |
|---|---|---|
| Frontend | React + TypeScript + Vite | Optimización y ampliación funcional |
| Backend | ASP.NET Core, monolito modular | Múltiples instancias si la demanda lo requiere |
| Base de datos | PostgreSQL | Optimización, réplicas de lectura según necesidad |
| Seguridad | Identity y autorización por ámbito | Controles adicionales según riesgo |
| Reservas | Transacciones y control de concurrencia | Optimización bajo mayor carga |
| Archivos | Almacenamiento de objetos | Escalamiento de capacidad |
| Notificaciones | Integración y procesamiento básico | Cola duradera si es necesaria |
| Caché | No obligatoria inicialmente | Redis según mediciones |
| Infraestructura | Docker, HTTPS y respaldos | Balanceador y despliegue distribuido |

## 5. Conclusión

La arquitectura seleccionada prioriza la consistencia de reservas, la seguridad multiempresa y la mantenibilidad.

El monolito modular con Clean Architecture permite construir una primera versión funcional sin introducir la complejidad de microservicios.

La separación de módulos y el diseño de despliegue permitirán evolucionar la solución conforme aumenten las empresas, agencias y operaciones.
