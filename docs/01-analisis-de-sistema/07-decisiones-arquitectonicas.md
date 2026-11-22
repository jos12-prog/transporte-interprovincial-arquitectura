# Decisiones Arquitectónicas

Las decisiones arquitectónicas establecen las principales soluciones técnicas adoptadas para responder a los drivers arquitectónicos, atributos de calidad y restricciones identificados para la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial en Ayacucho.

Cada decisión se establece considerando el contexto académico del proyecto, la necesidad de soportar concurrencia, el aislamiento entre empresas y agencias, la integración con servicios externos y la posibilidad de crecimiento de la plataforma.

---

## 1. Resumen de decisiones arquitectónicas

| ID | Decisión arquitectónica | Driver principal | Decisión adoptada |
|---|---|---|---|
| **ADR-01** | Estilo de arquitectura | DA01, DA05, DA14 | Monolito modular |
| **ADR-02** | Enfoque arquitectónico | DA14, AC07 | Clean Architecture |
| **ADR-03** | Comunicación entre cliente y backend | DA13, RC15 | API REST |
| **ADR-04** | Persistencia | DA02, DA03, DA07 | PostgreSQL |
| **ADR-05** | Modelo multiempresa/multiagencia | DA04 | Base de datos compartida, esquemas por módulo y aislamiento por tenant |
| **ADR-06** | Control de acceso | DA04, AC04 | RBAC + ámbito organizacional y agencia activa |
| **ADR-07** | Control de concurrencia | DA02, DA03, DA07 | Bloqueo por asiento de viaje y restricción única en PostgreSQL |
| **ADR-08** | Reserva temporal | DA08, DA03 | Estado HELD digital durante 10 minutos y expiración distribuida |
| **ADR-09** | Caché | DA06, DA11 | Redis para búsquedas y catálogos, TTL de 60 segundos |
| **ADR-10** | Procesamiento asíncrono | DA10 | RabbitMQ |
| **ADR-11** | Pasarela de pagos | DA09 | Culqi mediante patrón Adapter |
| **ADR-12** | Notificaciones | DA10, DA13 | WhatsApp Business Cloud API mediante procesamiento asíncrono |
| **ADR-13** | Auditoría | DA12 | Registro de operaciones críticas |
| **ADR-14** | Escalabilidad | DA01, DA05, DA06 | Escalamiento horizontal de la aplicación |
| **ADR-15** | Observabilidad | DA12, AC03 | Logs estructurados, métricas y monitoreo |

---

# 2. ADR-01 — Monolito modular

## Contexto

La plataforma debe manejar diferentes módulos relacionados entre sí: usuarios, empresas, agencias, rutas, servicios, reservas, pagos, disponibilidad, notificaciones y reportes.

Aunque el sistema debe estar preparado para crecer, dividir inicialmente la solución en múltiples microservicios incrementaría la complejidad de desarrollo, despliegue, comunicación y operación.

## Decisión

Se utilizará un **monolito modular**.

Los módulos estarán separados internamente y tendrán responsabilidades claramente definidas, pero inicialmente se desplegarán como una aplicación principal.

### Módulos principales

- Usuarios y autenticación.
- Empresas y agencias.
- Rutas.
- Servicios y horarios.
- Unidades y capacidad.
- Disponibilidad.
- Reservas.
- Pagos.
- Notificaciones.
- Reportes.
- Auditoría.

## Justificación

Esta decisión permite mantener una arquitectura organizada y escalable sin introducir desde el inicio la complejidad operacional de los microservicios.

Además, facilita mantener las operaciones críticas de reservas y disponibilidad dentro de una misma aplicación y base de datos transaccional.

## Consecuencia

La solución tendrá menor complejidad operacional inicialmente, pero deberá mantener límites claros entre módulos para permitir una futura extracción de componentes si el crecimiento lo requiere.

---

# 3. ADR-02 — Clean Architecture

## Contexto

Las reglas de negocio no deben depender directamente de PostgreSQL, Redis, RabbitMQ, Culqi, WhatsApp o del framework utilizado.

## Decisión

Se utilizará **Clean Architecture** dentro de los módulos principales.

La organización será:

```text
┌─────────────────────────────────────┐
│          PRESENTACIÓN               │
│       Controllers / API REST        │
├─────────────────────────────────────┤
│           APLICACIÓN                │
│        Casos de uso / DTO           │
├─────────────────────────────────────┤
│             DOMINIO                 │
│     Entidades y reglas de negocio   │
├─────────────────────────────────────┤
│         INFRAESTRUCTURA             │
│ PostgreSQL / Redis / RabbitMQ / APIs│
└─────────────────────────────────────┘
```

## Justificación

Permite proteger las reglas críticas del negocio frente a cambios tecnológicos y facilita las pruebas unitarias.

## Consecuencia

Se incrementa ligeramente la cantidad de estructuras y clases, pero se obtiene mayor mantenibilidad y separación de responsabilidades.

---

# 4. ADR-03 — API REST

## Contexto

La plataforma tendrá diferentes canales de acceso y deberá comunicarse con servicios externos.

## Decisión

La comunicación entre las aplicaciones cliente y el backend se realizará mediante una **API REST**.

```text
Web responsive / Agente
        │
        ▼
     API REST
        │
        ▼
Monolito modular
```

## Justificación

Permite separar la presentación de la lógica de negocio y facilita incorporar posteriormente nuevos clientes.

## Consecuencia

Las interfaces deberán utilizar contratos claramente definidos, validación de datos y mecanismos de autenticación.

---

# 5. ADR-04 — PostgreSQL como base de datos

## Contexto

El sistema manejará información altamente relacionada y operaciones transaccionales críticas.

## Decisión

Se utilizará **PostgreSQL** como base de datos principal.

La base almacenará:

- usuarios;
- empresas;
- agencias;
- rutas;
- servicios;
- unidades;
- disponibilidad;
- reservas;
- pagos;
- pasajes;
- notificaciones;
- auditoría.

## Justificación

El sistema requiere transacciones, restricciones de integridad, relaciones entre entidades y control de concurrencia.

## Consecuencia

Las operaciones críticas de reserva y disponibilidad deberán diseñarse considerando las capacidades transaccionales de PostgreSQL.

---

# 6. ADR-05 — Base de datos compartida con aislamiento lógico

## Contexto

La plataforma será utilizada por múltiples empresas y agencias, pero se mantendrá una única base de datos.

## Decisión

Se utilizará una **base PostgreSQL compartida**, con un esquema y un `DbContext` por módulo. La separación de tenants se aplicará mediante `company_id`, `agency_id`, Global Query Filters y validación de autorización en cada caso de uso; los módulos accederán a datos ajenos solo mediante contratos públicos.

Las entidades operativas tendrán referencia a su ámbito organizacional.

```text
Empresa
   │
   ├── Agencia
   │      │
   │      ├── Usuarios
   │      ├── Servicios
   │      ├── Reservas
   │      └── Operaciones
   │
   └── Agencia
```

## Justificación

Permite administrar centralizadamente la plataforma y facilita el crecimiento sin mantener una base de datos independiente por empresa.

## Consecuencia

El aislamiento deberá aplicarse obligatoriamente en la autorización y en las consultas del backend.

La seguridad no dependerá únicamente de ocultar información en la interfaz.

---

# 7. ADR-06 — RBAC + ámbito organizacional

## Contexto

La plataforma tendrá cuatro actores principales:

- Superadmin.
- Administrador de Agencia.
- Agente/Vendedor.
- Pasajero.

## Decisión

Se utilizará **RBAC (Role-Based Access Control)** complementado con el ámbito organizacional.

```text
Usuario
   │
   ▼
Rol
   │
   ▼
Empresa / Agencia
   │
   ▼
Recursos permitidos
```

## Reglas principales

### Superadmin

Puede administrar la plataforma y gestionar empresas, agencias y administradores.

### Administrador de Agencia

Puede gestionar una o varias agencias autorizadas pertenecientes a una sola empresa. El token contiene `sub`, `role`, `company_id` y `agency_ids[]`; la agencia activa se envía en `X-Agency-Id` y el backend la valida en cada caso de uso.

### Agente/Vendedor

Puede realizar operaciones comerciales únicamente dentro de su agencia.

### Pasajero

Puede consultar sus propios datos, reservas y pasajes.

## Justificación

El rol por sí solo no es suficiente. Un Administrador de Agencia podría tener el mismo rol que otro administrador, pero no debe acceder a la información de otra agencia.

---

# 8. ADR-07 — Control de concurrencia

## Contexto

Dos o más pasajeros o agentes pueden intentar reservar simultáneamente el mismo asiento numerado de un viaje.

## Decisión

La disponibilidad será controlada mediante **transacciones y mecanismos de concurrencia en PostgreSQL**.

```text
Solicitud A ──┐
              ├──> PostgreSQL
Solicitud B ──┘        │
                       ▼
                Control transaccional
                       │
                       ▼
              Un único recurso confirmado
```

## Justificación

La base de datos será la fuente de verdad de las operaciones críticas.

## Consecuencia

Las operaciones de reserva deberán ser cortas, transaccionales e idempotentes cuando corresponda. La asignación bloquea `TripSeat` con `SELECT FOR UPDATE`; un índice único parcial sobre `trip_seat_id` con estados `HELD` y `CONFIRMED` actúa como salvaguarda contra carreras.

---

# 9. ADR-08 — Reserva temporal de 10 minutos

## Contexto

Un pasajero puede seleccionar un asiento y tardar algunos minutos en completar el pago.

Mantener el asiento bloqueado indefinidamente perjudicaría la disponibilidad.

## Decisión

Solo las reservas iniciadas en el canal digital tendrán una retención `HELD` de **10 minutos**. La venta presencial registra pago en efectivo, Yape/Plin o POS y confirma directamente sin cierre de caja.

```text
DISPONIBLE
     ↓
   HELD
     ↓
10 minutos
     │
     ├───────────────┐
     ▼               ▼
PAGO CONFIRMADO   SIN PAGO
     │               │
     ▼               ▼
 CONFIRMADA       EXPIRED
                     │
                     ▼
              LIBERAR ASIENTOS
```

Las máquinas de estados se mantienen separadas: reserva `HELD`, `CONFIRMED`, `CANCELLED`, `EXPIRED`, `RESCHEDULED`; pago `PENDING`, `APPROVED`, `REJECTED`, `ERROR`, `REFUNDED`; pasaje `ISSUED`, `USED`, `VOIDED`.

## Justificación

El periodo permite completar razonablemente el proceso de pago y, al mismo tiempo, evita bloquear recursos durante demasiado tiempo.

## Consecuencia

Debe existir un mecanismo que detecte y expire automáticamente las reservas vencidas. Un `BackgroundService` revisa las reservas cada 10 segundos y usa `FOR UPDATE SKIP LOCKED`, lo que permite ejecutar varias instancias sin procesar una retención más de una vez.

---

# 10. ADR-09 — Redis como caché

## Contexto

Las consultas de rutas, horarios y servicios pueden repetirse constantemente, especialmente durante periodos de alta demanda.

## Decisión

Se utilizará **Redis como capa de caché** exclusivamente para búsquedas de viajes y catálogos con TTL de 60 segundos.

```text
Cliente
   ↓
API
   ↓
Redis
 ┌─┴───────────────┐
 │                 │
Existe          No existe
 │                 │
 ▼                 ▼
Respuesta       PostgreSQL
                   │
                   ▼
                 Redis
```

## Regla importante

Redis **no será la fuente principal de verdad para una reserva**.

El mapa de asientos, su disponibilidad exacta y las operaciones de reserva se consultan siempre contra PostgreSQL; no se almacenan en caché.

## Consecuencia

Será necesario definir políticas de expiración e invalidación de caché.

---

# 11. ADR-10 — RabbitMQ para procesamiento asíncrono

## Contexto

Algunas operaciones no necesitan ejecutarse dentro de la transacción principal.

Por ejemplo:

- envío de WhatsApp;
- recordatorios;
- notificaciones;
- procesamiento de eventos secundarios.

## Decisión

Se utilizará **Transactional Outbox + RabbitMQ** como mecanismo de mensajería y procesamiento asíncrono. El evento se persiste junto a la transacción principal y un consumidor despacha la notificación.

```text
Reserva confirmada
       │
       ▼
Persistir evento en Outbox
       │
       ▼
    RabbitMQ
       │
       ▼
Consumidor
       │
       ▼
WhatsApp
```

## Justificación

Evita que un problema temporal de un servicio externo bloquee una operación principal.

## Consecuencia

Se deberán implementar reintentos, manejo de errores y control de mensajes duplicados.

---

# 12. ADR-11 — Culqi mediante Adapter

## Contexto

El sistema requiere una pasarela de pagos externa.

Las alternativas consideradas son:

- Stripe.
- Culqi.
- Niubiz.

## Decisión

Se seleccionará **Culqi** para la primera versión y se integrará mediante `IPaymentGateway` en Aplicación, implementada por `CulqiPaymentAdapter` en Infraestructura. El frontend obtiene un token mediante Culqi Checkout; el backend crea el cargo y valida la respuesta del servidor. Los webhooks firmados HMAC reconcilian el resultado; el primer evento válido es autoritativo y el procesamiento es idempotente por `charge_id` único.

```text
Servicio de Pagos
       │
       ▼
Payment Adapter
       │
       ▼
      Culqi
```

## Justificación

Para este proyecto se prioriza una alternativa orientada al contexto peruano y adecuada para una integración mediante API.

El patrón Adapter permite mantener la lógica de negocio independiente del proveedor seleccionado.

Por tanto, si posteriormente se requiriera utilizar Stripe o Niubiz, la lógica principal de reservas no debería modificarse significativamente.

## Consecuencia

El sistema deberá manejar:

- creación de operación de pago;
- respuesta del proveedor;
- confirmación;
- rechazo;
- errores;
- validación de notificaciones/webhooks;
- idempotencia.

Si la aprobación llega después de expirar la reserva, el backend confirma únicamente si todos los asientos siguen libres; en caso contrario, registra `REFUNDED`.

---

# 13. ADR-12 — WhatsApp para notificaciones

## Contexto

Las notificaciones son un complemento importante de la plataforma y representan un plus funcional del proyecto.

## Decisión

Se utilizará **WhatsApp Business Cloud API** a través de `INotificationGateway` en Aplicación, implementada por `WhatsAppNotificationAdapter` en Infraestructura. Las notificaciones se procesan de forma asíncrona con Transactional Outbox y RabbitMQ.

Las notificaciones podrán incluir:

- confirmación de reserva;
- confirmación de pago;
- emisión del pasaje;
- recordatorio próximo al viaje;
- aviso de alta demanda;
- cambios de horario;
- cancelaciones.

## Ejemplo de notificación preventiva

```text
Viaje: Ayacucho → Lima

Fecha: 15 de diciembre

⚠ Se prevé alta demanda durante esta fecha
debido a la festividad programada.

Te recomendamos reservar con anticipación.
```

## Justificación

La notificación se considera una funcionalidad complementaria y no debe bloquear las operaciones principales.

Por ello, será procesada mediante RabbitMQ.

Las alertas internas de alta demanda se envían al Administrador de Agencia. Las alertas a pasajeros requieren opt-in y ocupación superior al 70 %.

---

# 14. ADR-13 — Auditoría

## Contexto

Las operaciones administrativas y comerciales deben poder ser rastreadas.

## Decisión

Se registrarán operaciones críticas como:

- creación o modificación de servicios;
- modificación de horarios;
- creación de reservas;
- cancelaciones;
- pagos;
- emisión de pasajes;
- modificaciones administrativas.

El registro deberá conservar:

```text
Usuario
Empresa
Agencia
Operación
Fecha/Hora
Recurso afectado
Resultado
```

## Justificación

Permite investigar errores, operaciones no autorizadas y problemas de negocio.

---

# 15. ADR-14 — Escalabilidad horizontal

## Contexto

La cantidad de usuarios puede aumentar significativamente durante festividades, feriados y temporadas de alta demanda.

## Decisión

La aplicación deberá diseñarse para permitir ejecutar múltiples instancias del backend detrás de un balanceador.

```text
                    Balanceador
                         │
             ┌───────────┼───────────┐
             ▼           ▼           ▼
          API 1        API 2        API 3
             │           │           │
             └───────────┼───────────┘
                         ▼
                     PostgreSQL
```

Redis permitirá reducir determinadas consultas repetitivas y RabbitMQ permitirá desacoplar tareas asíncronas.

## Justificación

Permite aumentar capacidad sin modificar las reglas principales del sistema.

---

# 16. ADR-15 — Observabilidad

## Contexto

Una plataforma con reservas, pagos y servicios externos requiere identificar errores y degradaciones.

## Decisión

Se implementarán mecanismos de:

- logs estructurados;
- métricas;
- monitoreo;
- trazabilidad de operaciones;
- alertas para errores críticos.

## Justificación

Permite detectar problemas relacionados con reservas, pagos, colas, caché y servicios externos.

---

# 17. Matriz de alternativas evaluadas

| Decisión | Alternativa | Seleccionada | Justificación |
|---|---|---|---|
| Arquitectura | Microservicios | No | Mayor complejidad para el alcance inicial. |
| Arquitectura | Monolito modular | **Sí** | Permite modularidad y menor complejidad operacional. |
| Persistencia | PostgreSQL | **Sí** | Requerimientos transaccionales y relacionales. |
| Caché | Redis | **Sí** | Adecuado para consultas frecuentes. |
| Mensajería | RabbitMQ | **Sí** | Permite procesamiento asíncrono y desacoplamiento. |
| Pago | Stripe | No | Se prioriza una alternativa orientada al contexto peruano. |
| Pago | Culqi | **Sí** | Adecuado para la primera versión y encapsulado mediante Adapter. |
| Pago | Niubiz | No | Se mantiene como alternativa futura. |
| Comunicación | API REST | **Sí** | Separación entre clientes y backend. |
| Notificaciones | WhatsApp Business API | **Sí** | Canal de comunicación complementario para pasajeros. |

---

# 18. Relación entre decisiones y drivers

| Driver | Decisiones relacionadas |
|---|---|
| DA01 - Concurrencia de usuarios | ADR-01, ADR-07, ADR-14 |
| DA02 - Evitar sobreventa | ADR-07 |
| DA03 - Consistencia | ADR-04, ADR-07, ADR-08 |
| DA04 - Seguridad multiempresa | ADR-05, ADR-06 |
| DA05 - Escalabilidad | ADR-01, ADR-09, ADR-10, ADR-14 |
| DA06 - Rendimiento | ADR-09, ADR-14 |
| DA07 - Disponibilidad común | ADR-04, ADR-07 |
| DA08 - Reserva temporal | ADR-08 |
| DA09 - Pagos | ADR-11 |
| DA10 - Notificaciones | ADR-10, ADR-12 |
| DA11 - Caché | ADR-09 |
| DA12 - Auditoría | ADR-13, ADR-15 |
| DA13 - Interoperabilidad | ADR-03, ADR-11, ADR-12 |
| DA14 - Mantenibilidad | ADR-01, ADR-02 |

---

# 19. Resultado de las decisiones

Las decisiones adoptadas permiten establecer una arquitectura inicial coherente con las necesidades del proyecto:

```text
                    USUARIOS
                        │
                        ▼
              Aplicación Web Responsive
                        │
                        ▼
                 API REST
                        │
                        ▼
              MONOLITO MODULAR
                        │
        ┌───────────────┼────────────────┐
        │               │                │
        ▼               ▼                ▼
   Reservas        Disponibilidad      Pagos
        │               │                │
        │               │             Adapter
        │               │                │
        │               │              Culqi
        │               │
        └───────┬───────┘
                ▼
            PostgreSQL
                │
       ┌────────┴────────┐
       ▼                 ▼
     Redis            RabbitMQ
                         │
                         ▼
                  Notificaciones
                         │
                         ▼
                  WhatsApp API
```

Esta arquitectura responde principalmente a los problemas de **concurrencia, sobreventa, aislamiento entre empresas y agencias, rendimiento, escalabilidad, integración de pagos y notificaciones**, que constituyen los principales desafíos arquitectónicos de la plataforma.
