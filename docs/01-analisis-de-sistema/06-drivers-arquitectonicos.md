# Drivers Arquitectónicos

Los drivers arquitectónicos representan los requisitos funcionales, atributos de calidad y restricciones que tienen una influencia significativa en las decisiones de arquitectura de la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial en Ayacucho.

Los drivers identificados permiten justificar posteriormente el estilo arquitectónico, los componentes, las tecnologías, los mecanismos de concurrencia, la estrategia de escalabilidad y las integraciones externas.

## 1. Identificación de drivers arquitectónicos

| ID | Driver arquitectónico | Origen | ¿Por qué influye en la arquitectura? |
|---|---|---|---|
| **DA01** | El sistema debe soportar múltiples pasajeros, agentes y administradores realizando operaciones simultáneamente. | AC01, AC02, AC05 | Influye en la estrategia de concurrencia, procesamiento de solicitudes, rendimiento y escalabilidad. |
| **DA02** | El sistema debe impedir la confirmación simultánea del mismo asiento o cupo. | RF12, RF26, AC05, AC06 | Obliga a definir mecanismos transaccionales y de control de concurrencia para evitar sobreventas. |
| **DA03** | El sistema debe mantener consistencia entre reservas, pagos, disponibilidad y pasajes. | RF10, RF11, RF19, RF21, AC06 | Determina el manejo de transacciones, estados de reserva y confirmación de operaciones. |
| **DA04** | El sistema debe aislar la información de las diferentes empresas y agencias que utilizan la plataforma. | RF35, RF39, RC02, RC03, AC04 | Influye directamente en autenticación, autorización, modelo de datos y control del ámbito organizacional. |
| **DA05** | El sistema debe permitir el crecimiento de empresas, agencias, pasajeros, servicios y operaciones sin modificar completamente la solución. | AC02, RC02 | Influye en la modularidad, escalabilidad y organización de los componentes. |
| **DA06** | El sistema debe mantener tiempos de respuesta adecuados durante periodos de alta demanda. | AC01, AC03, RC12 | Influye en la utilización de caché, optimización de consultas y distribución de carga. |
| **DA07** | El sistema debe permitir que las operaciones digitales y presenciales utilicen una disponibilidad común. | RF23, RF24, RF26, RC07 | Obliga a centralizar la disponibilidad y evitar fuentes independientes que puedan generar inconsistencias. |
| **DA08** | El sistema debe mantener una reserva temporal durante un periodo limitado y liberarla automáticamente si no se confirma el pago. | RF07–RF13, RC08 | Influye en la gestión de estados, expiración de reservas, tareas programadas y actualización de disponibilidad. |
| **DA09** | El sistema debe integrar una pasarela de pago externa sin acoplar la lógica de negocio al proveedor. | RF18–RF21, RC10, AC08 | Determina el uso de un adaptador para aislar la integración de Culqi y facilitar una futura sustitución del proveedor. |
| **DA10** | El sistema debe enviar notificaciones sin bloquear las operaciones principales de reserva y venta. | RF47, RF48, RC11, RC13 | Influye en el procesamiento asíncrono y en el uso de una cola de eventos para desacoplar las notificaciones. |
| **DA11** | El sistema debe soportar consultas frecuentes sin generar una carga innecesaria sobre la base de datos. | AC01, RC12 | Justifica la incorporación de una capa de caché para información de consulta frecuente. |
| **DA12** | El sistema debe mantener trazabilidad de las operaciones administrativas y críticas. | RF43, AC09 | Influye en el diseño de auditoría, registros estructurados y mecanismos de seguimiento. |
| **DA13** | El sistema debe integrarse mediante interfaces claramente definidas con servicios externos. | AC08, RC10, RC11, RC15 | Influye en la separación de responsabilidades y en la definición de contratos de integración. |
| **DA14** | La solución debe permitir evolucionar sus módulos sin generar dependencias innecesarias entre las reglas de negocio y la infraestructura. | AC07, RC15 | Justifica una organización modular y la separación de responsabilidades mediante Clean Architecture. |

---

## 2. Drivers prioritarios

No todos los drivers tienen el mismo impacto sobre la solución. Los siguientes tienen prioridad arquitectónica.

### DA02 — Control de concurrencia

Es uno de los principales drivers del proyecto debido a que diferentes pasajeros y agentes pueden intentar adquirir el mismo asiento o cupo simultáneamente.

La arquitectura debe garantizar que solamente una operación pueda confirmar el recurso.

```text
Pasajero A ──────┐
                 ├──> Servicio de Reservas
Pasajero B ──────┘
                 │
Agente ──────────┘
                 ↓
       Control transaccional
                 ↓
       Disponibilidad única
```

**Impacto:** base de datos, transacciones, bloqueo/control de concurrencia y modelo de estados.

---

### DA04 — Seguridad y aislamiento multiempresa

La plataforma será utilizada por diferentes empresas y agencias utilizando una misma solución y una misma base de datos.

Por ello, el sistema debe identificar el ámbito organizacional del usuario:

```text
Usuario
   ↓
Rol
   ↓
Empresa
   ↓
Agencia
   ↓
Recursos autorizados
```

**Impacto:** autenticación, autorización, middleware/interceptores, modelo de datos y control de acceso.

---

### DA05 — Escalabilidad

La plataforma debe poder incorporar progresivamente nuevas empresas, agencias, pasajeros y servicios.

El crecimiento esperado no debe obligar a modificar completamente la solución.

**Impacto:** arquitectura modular, separación de responsabilidades, caché, procesamiento asíncrono y capacidad de escalar la aplicación.

---

### DA06 — Rendimiento durante alta demanda

Durante festividades, feriados y fechas de alta movilidad puede producirse un incremento significativo de consultas y reservas.

**Impacto:** caché, optimización de consultas, procesamiento asíncrono y capacidad de atender solicitudes concurrentes.

---

### DA08 — Reserva temporal

La reserva temporal requiere mantener un recurso bloqueado durante un periodo limitado.

El flujo arquitectónico será:

```text
Disponible
    ↓
HELD
    ↓
10 minutos
    ↓
 ┌───────────────┐
 │               │
Pago confirmado  Sin pago
 │               │
 ↓               ↓
CONFIRMADA     EXPIRED
                 ↓
          Liberar recurso
```

**Impacto:** gestión de estados, persistencia, expiración y actualización de disponibilidad.

---

### DA09 — Integración de pagos

La solución utilizará **Culqi** como proveedor externo de pagos.

La lógica de reservas no deberá depender directamente de la implementación específica de Culqi.

```text
Reserva
   ↓
Servicio de pagos
   ↓
Payment Adapter
   ↓
Culqi
   ↓
Resultado del pago
   ↓
Confirmación de reserva
```

**Impacto:** patrón Adapter, contratos de integración, manejo de errores, validación de respuestas y webhooks.

---

### DA10 — Notificaciones asíncronas

Las notificaciones de WhatsApp no deben bloquear una reserva o venta.

El flujo previsto será:

```text
Reserva confirmada
       ↓
Publicar evento
       ↓
RabbitMQ
       ↓
Servicio de notificaciones
       ↓
WhatsApp Business Cloud API
```

Si WhatsApp presenta una falla temporal, la operación principal deberá conservarse correctamente.

**Impacto:** cola de eventos, reintentos, procesamiento asíncrono y tolerancia a fallos.

---

### DA11 — Caché

Determinadas consultas serán realizadas frecuentemente, por ejemplo:

- rutas;
- destinos;
- horarios;
- información de servicios;
- disponibilidad de información no crítica.

Se utilizará **Redis** para reducir consultas repetitivas a PostgreSQL cuando sea apropiado.

La caché no será la fuente principal de verdad para las operaciones críticas de reserva.

```text
Cliente
   ↓
API
   ↓
Redis
   │
   ├── Existe → responder
   │
   └── No existe
          ↓
      PostgreSQL
          ↓
       Redis
```

**Impacto:** rendimiento, disponibilidad y estrategia de invalidación de caché.

---

### DA12 — Trazabilidad

Las operaciones críticas deberán poder reconstruirse posteriormente.

Se deberá identificar como mínimo:

- usuario;
- rol;
- empresa;
- agencia;
- operación;
- fecha y hora;
- recurso afectado;
- resultado.

**Impacto:** auditoría, almacenamiento de eventos y registros estructurados.

---

## 3. Relación entre drivers y mecanismos arquitectónicos

| Driver | Mecanismo arquitectónico |
|---|---|
| **DA01 - Concurrencia de usuarios** | API REST, control de concurrencia y escalabilidad horizontal. |
| **DA02 - Evitar sobreventa** | Transacciones, restricciones de BD y control de concurrencia. |
| **DA03 - Consistencia** | Transacciones, estados controlados y validación de operaciones. |
| **DA04 - Aislamiento** | RBAC + ámbito empresa/agencia + autorización en backend. |
| **DA05 - Escalabilidad** | Monolito modular, separación de módulos y escalamiento de infraestructura. |
| **DA06 - Rendimiento** | Redis, optimización de consultas y procesamiento asíncrono. |
| **DA07 - Disponibilidad común** | Fuente transaccional única para canales web y presencial. |
| **DA08 - Reserva temporal** | Estados de reserva, expiración y tareas programadas. |
| **DA09 - Pagos** | Adapter + Culqi + validación de resultado/webhook. |
| **DA10 - Notificaciones** | RabbitMQ + consumidor de eventos + WhatsApp API. |
| **DA11 - Consultas frecuentes** | Redis como caché. |
| **DA12 - Auditoría** | Registro de eventos y operaciones críticas. |
| **DA13 - Integraciones** | Interfaces y adaptadores externos. |
| **DA14 - Mantenibilidad** | Monolito modular + Clean Architecture. |

---

## 4. Priorización de drivers

| Prioridad | Drivers |
|---|---|
| **Crítica** | DA02, DA03, DA04 |
| **Alta** | DA01, DA05, DA06, DA08, DA09, DA10 |
| **Media-Alta** | DA07, DA11, DA12, DA13, DA14 |

Los drivers críticos serán considerados como restricciones principales para la definición de la arquitectura.

---

## 5. Trazabilidad inicial

Los drivers arquitectónicos se derivan de los elementos previamente definidos:

```text
Necesidad del negocio
        ↓
Actores
        ↓
Historias de usuario
        ↓
Requisitos funcionales
        ↓
Atributos de calidad
        ↓
Restricciones
        ↓
Drivers arquitectónicos
        ↓
Decisiones arquitectónicas
        ↓
Arquitectura de la solución
```

Esta trazabilidad permitirá justificar por qué cada decisión arquitectónica responde a una necesidad concreta del sistema.

---

## 6. Aspectos que deberán resolverse en la arquitectura

Los siguientes aspectos deberán ser definidos en la siguiente etapa:

1. Estrategia concreta de control de concurrencia para las reservas.
2. Modelo de estados de una reserva.
3. Estrategia de expiración automática de reservas.
4. Estrategia de caché con Redis.
5. Diseño de la cola de eventos con RabbitMQ.
6. Integración de Culqi mediante Adapter.
7. Integración de WhatsApp mediante procesamiento asíncrono.
8. Modelo de aislamiento multiempresa y multiagencia.
9. Estrategia de autenticación y autorización.
10. Organización modular de la aplicación.
11. Estrategia de despliegue y escalabilidad.
12. Monitoreo, auditoría y recuperación ante fallos.

Estos puntos serán utilizados como entrada para las decisiones arquitectónicas y el diseño de la arquitectura inicial.