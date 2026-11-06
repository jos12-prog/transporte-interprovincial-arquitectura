# Atributos de Calidad

Los atributos de calidad definen las características no funcionales que debe cumplir la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial en Ayacucho.

Estos atributos se priorizan considerando la naturaleza del sistema, la operación simultánea de diferentes agencias, la posibilidad de recibir múltiples solicitudes concurrentes, la necesidad de proteger la información de las empresas y agencias y la integración con servicios externos de pago y notificaciones.

Los valores cuantitativos establecidos en este documento representan objetivos iniciales de diseño y deberán ser validados mediante pruebas durante la implementación.

## Atributos de calidad prioritarios

| ID | Atributo | Prioridad | Justificación |
|---|---|---|---|
| **AC01** | Rendimiento | Alta | La plataforma debe responder oportunamente durante consultas de viajes, disponibilidad, reservas y ventas. |
| **AC02** | Escalabilidad | Alta | El sistema debe soportar el crecimiento de empresas, agencias, servicios, pasajeros y operaciones concurrentes sin rediseñar completamente la solución. |
| **AC03** | Disponibilidad | Alta | La consulta y operación de reservas debe mantenerse disponible durante los periodos normales y de alta demanda. |
| **AC04** | Seguridad | Crítica | La información de empresas, agencias, usuarios, reservas y pagos debe estar protegida frente a accesos no autorizados. |
| **AC05** | Concurrencia | Crítica | El sistema debe impedir que dos operaciones confirmen simultáneamente el mismo asiento o cupo. |
| **AC06** | Integridad y consistencia | Crítica | Las reservas, pagos, disponibilidad y pasajes deben mantener estados coherentes aun cuando ocurran fallos o solicitudes simultáneas. |
| **AC07** | Mantenibilidad | Alta | La solución debe permitir modificar reglas o integrar nuevos servicios sin afectar innecesariamente otros módulos. |
| **AC08** | Interoperabilidad | Alta | La plataforma debe integrarse con la pasarela de pagos y el servicio de notificaciones mediante interfaces desacopladas. |
| **AC09** | Trazabilidad y auditoría | Alta | Las operaciones administrativas y críticas deben poder ser identificadas y consultadas posteriormente. |
| **AC10** | Usabilidad | Media-Alta | Los pasajeros y usuarios administrativos deben poder realizar sus operaciones sin complejidad innecesaria. |

## Criterios de evaluación de los atributos prioritarios

### AC01 - Rendimiento

El sistema debe responder de manera adecuada ante consultas frecuentes de viajes, horarios y disponibilidad.

**Criterio inicial:**

- Con **100 usuarios concurrentes**, al menos el **95 % de las consultas de disponibilidad y búsqueda** deberá responder en un tiempo menor o igual a **2 segundos**, bajo un escenario de carga representativo.
- Las operaciones críticas de reserva deberán mantener tiempos de respuesta controlados aun cuando exista concurrencia.

**Verificación:**

Se realizarán pruebas de carga y rendimiento utilizando herramientas como k6 o equivalente, registrando tiempo de respuesta, tasa de errores y cantidad de solicitudes procesadas.

---

### AC02 - Escalabilidad

La plataforma debe permitir incorporar nuevas empresas, agencias, rutas, horarios y pasajeros sin realizar cambios estructurales en los módulos existentes.

**Criterio inicial:**

- La incorporación de una nueva empresa o agencia deberá realizarse mediante configuración y registro de información.
- El incremento de usuarios y solicitudes deberá poder atenderse mediante ampliación de recursos de infraestructura y mecanismos de caché y procesamiento asíncrono.

**Verificación:**

Se realizarán pruebas incrementales aumentando progresivamente usuarios concurrentes, solicitudes y datos almacenados.

---

### AC03 - Disponibilidad

La plataforma deberá mantener disponibles las funciones principales de consulta, disponibilidad y operación de reservas durante el horario de servicio.

**Criterio inicial:**

- Las fallas de servicios secundarios, como notificaciones, no deberán impedir la confirmación de operaciones que ya hayan cumplido correctamente sus condiciones.
- Las operaciones críticas deberán mantener su información aun cuando ocurra una interrupción temporal de componentes secundarios.

**Verificación:**

Se simularán fallos controlados de servicios secundarios y se comprobará que las operaciones principales continúen funcionando y que las fallas sean registradas.

---

### AC04 - Seguridad

El sistema deberá proteger la información de las diferentes empresas y agencias mediante autenticación, autorización y control del ámbito organizacional.

**Criterio inicial:**

- El **100 % de las pruebas de autorización definidas** deberá impedir accesos no permitidos.
- Un Administrador de Agencia no podrá consultar ni modificar información de otra empresa.
- Un Agente/Vendedor no podrá operar sobre una agencia diferente a la que tiene asignada.
- Un pasajero únicamente podrá consultar sus propios datos, reservas y pasajes.

**Verificación:**

Se ejecutarán pruebas de autenticación, autorización basada en roles, aislamiento por empresa/agencia y acceso indebido mediante API.

---

### AC05 - Concurrencia

La plataforma debe controlar operaciones simultáneas sobre un mismo asiento o cupo.

**Escenario principal:**

```text
Pasajero A ─────┐
                ├──> mismo viaje / mismo asiento
Pasajero B ─────┘
                ↓
        Control de concurrencia
                ↓
      Solo una operación confirma
```

**Criterio inicial:**

- En el **100 % de las pruebas concurrentes definidas**, no deberá existir más de una confirmación válida para el mismo asiento o cupo.
- Una operación presencial y una operación digital deberán utilizar la misma fuente transaccional de disponibilidad.
- Las reservas temporales deberán bloquear el recurso durante su vigencia.

**Verificación:**

Se ejecutarán solicitudes simultáneas sobre el mismo servicio y asiento, comprobando que solamente una operación pueda confirmar el recurso.

---

### AC06 - Integridad y consistencia

La información de reserva, pago, disponibilidad y pasaje deberá mantenerse coherente durante todo el ciclo de una operación.

**Criterio inicial:**

Una reserva deberá seguir un flujo controlado:

```text
Disponible
    ↓
Reserva temporal
    ↓
Pago pendiente
    ↓
Pago validado
    ↓
Reserva confirmada
    ↓
Pasaje emitido
```

Si el pago no se confirma dentro del periodo establecido:

```text
Reserva temporal
       ↓
    10 minutos
       ↓
     Expirada
       ↓
Asiento liberado
```

**Verificación:**

Se realizarán pruebas de interrupción, expiración, pago rechazado, doble confirmación y cancelación para comprobar que no queden reservas o pagos en estados inconsistentes.

---

### AC07 - Mantenibilidad

La arquitectura deberá permitir modificar reglas de negocio o componentes externos sin afectar innecesariamente el resto del sistema.

**Criterio inicial:**

- El cambio de proveedor de pagos deberá concentrarse principalmente en el adaptador correspondiente.
- El cambio del proveedor de notificaciones no deberá modificar las reglas centrales de reservas.
- Las reglas de negocio deberán mantenerse separadas de infraestructura y servicios externos.

**Verificación:**

Se realizará una revisión de dependencias y pruebas de regresión después de modificar componentes externos o reglas específicas.

---

### AC08 - Interoperabilidad

El sistema deberá comunicarse con servicios externos mediante interfaces claramente definidas.

Las principales integraciones previstas son:

```text
Plataforma
    │
    ├── Adapter ──> Pasarela de pagos
    │
    └── Adaptador ──> WhatsApp Business Cloud API
```

**Criterio inicial:**

- Los errores de un proveedor externo deberán manejarse sin comprometer la consistencia de las operaciones internas.
- Las respuestas de los servicios externos deberán validarse antes de modificar estados críticos.
- Las comunicaciones deberán utilizar protocolos seguros.

**Verificación:**

Se realizarán pruebas con respuestas exitosas, rechazadas, duplicadas, retrasadas y fallidas de los servicios externos.

---

### AC09 - Trazabilidad y auditoría

Las operaciones críticas deberán conservar información suficiente para reconstruir quién realizó una operación, cuándo se realizó y sobre qué empresa y agencia.

**Criterio inicial:**

Las operaciones administrativas y críticas deberán registrar como mínimo:

- usuario;
- rol;
- empresa;
- agencia;
- operación;
- fecha y hora;
- identificador de la entidad afectada;
- resultado de la operación.

**Verificación:**

Se ejecutarán operaciones de creación, modificación, cancelación y administración y posteriormente se verificará su existencia en los registros de auditoría.

---

### AC10 - Usabilidad

La plataforma deberá permitir que los diferentes actores completen sus operaciones de manera clara y consistente.

**Criterio inicial:**

- Las funciones principales deberán estar organizadas según el rol del usuario.
- El pasajero deberá poder completar el proceso de búsqueda, selección, reserva y pago sin pasos innecesarios.
- El Agente/Vendedor deberá poder registrar una operación presencial utilizando la misma disponibilidad del sistema.
- El Administrador deberá poder seleccionar únicamente las agencias que tiene autorizadas.

**Verificación:**

Se realizarán pruebas de uso con escenarios representativos de cada actor y se registrarán errores, tareas incompletas y tiempos de ejecución.

---

## Priorización arquitectónica

Los atributos con mayor impacto sobre las decisiones arquitectónicas son:

### Prioridad crítica

- **AC04 - Seguridad**
- **AC05 - Concurrencia**
- **AC06 - Integridad y consistencia**

Estos atributos determinan principalmente el diseño de autorización, aislamiento de información, transacciones y control de reservas.

### Prioridad alta

- **AC01 - Rendimiento**
- **AC02 - Escalabilidad**
- **AC03 - Disponibilidad**
- **AC07 - Mantenibilidad**
- **AC08 - Interoperabilidad**
- **AC09 - Trazabilidad**

Estos atributos determinan la necesidad de mecanismos como caché, procesamiento asíncrono, adaptadores para servicios externos, monitoreo y separación de responsabilidades.

### Prioridad media-alta

- **AC10 - Usabilidad**

Este atributo orienta principalmente el diseño de las interfaces y los flujos de interacción de cada actor.

---

## Relación con la solución arquitectónica

Los atributos de calidad identificados serán utilizados posteriormente para justificar las decisiones arquitectónicas de la plataforma.

| Necesidad | Mecanismo arquitectónico previsto |
|---|---|
| Alta concurrencia | Control transaccional y estrategia de concurrencia |
| Consultas frecuentes | Caché |
| Procesamiento de notificaciones | Cola de eventos |
| Integración de pagos | Patrón Adapter |
| Integración con WhatsApp | Adaptador y procesamiento asíncrono |
| Aislamiento entre empresas/agencias | Autenticación, autorización y ámbito organizacional |
| Crecimiento de usuarios | Escalamiento de la aplicación y servicios de soporte |
| Consistencia de reservas | Transacciones y control de concurrencia |
| Recuperación ante fallos | Persistencia, reintentos y monitoreo |
| Trazabilidad | Auditoría y registros estructurados |

Estos mecanismos no constituyen todavía decisiones definitivas de implementación. Su selección y justificación serán desarrolladas en los documentos de drivers y decisiones arquitectónicas.

---

## Resumen

La plataforma prioriza principalmente **seguridad, concurrencia e integridad**, debido a que una inconsistencia en la disponibilidad de un asiento o cupo puede producir una sobreventa y afectar directamente la operación de las agencias.

Asimismo, el sistema debe considerar **rendimiento, escalabilidad y disponibilidad** debido a la posibilidad de recibir un incremento significativo de usuarios y operaciones durante festividades, feriados y temporadas de alta demanda.

Finalmente, la **mantenibilidad, interoperabilidad y trazabilidad** permitirán integrar servicios externos como la pasarela de pagos y WhatsApp, mantener la evolución de la plataforma y conservar evidencia de las operaciones críticas.