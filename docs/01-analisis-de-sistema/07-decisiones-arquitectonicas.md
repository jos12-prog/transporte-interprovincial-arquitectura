# 07. Decisiones Arquitectónicas (ADRs)

## 1. Fundamentación y Metodología (Paso 5 y Paso 13 de la Docente)

Las **Decisiones Arquitectónicas** representan las respuestas formales de ingeniería ante los drivers arquitectónicos identificados (DA01 a DA08). 

Siguiendo la metodología enseñada por la docente, cada decisión de alto impacto se formaliza mediante un **ADR (Architecture Decision Record)**, el cual documenta:
* **Contexto y problema:** ¿Qué situación o driver motiva la decisión?
* **Alternativas evaluadas:** ¿Qué otras opciones técnicas se analizaron?
* **Decisión y justificación:** ¿Qué opción se adoptó y por qué?
* **Consecuencias:** ¿Qué beneficios aporta y qué compromisos (trade-offs) asumimos?

---

## 2. Registro de Decisiones Arquitectónicas (ADRs)

### ADR01: Adopción del Estilo Monolito Modular con Arquitectura en Capas
* **Estado:** Aprobado.
* **Drivers vinculados:** DA05 (Escalabilidad), DA06 (Mantenibilidad), DA01 (Consistencia).
* **Contexto y problema:** Se requiere una solución que soporte 8 módulos funcionales complejos (reservas, pagos, encomiendas, administración, etc.) en un entorno académico y operativo inicial en Ayacucho. Se debe evitar la fragmentación operativa sin sacrificar la mantenibilidad futura.
* **Alternativas evaluadas:**
  1. *Monolito tradicional en capas (Sin modularidad estricta):* Rápido de programar, pero tiende al acoplamiento espagueti y rompe el aislamiento entre módulos.
  2. *Microservicios distribuidos:* Máxima independencia de despliegue, pero introduce enorme complejidad de red, transacciones distribuidas (Saga), orquestación con Kubernetes y altos costos de infraestructura.
  3. *Monolito Modular con arquitectura en capas:* Unifica el despliegue en un único artefacto ejecutable, manteniendo límites lógicos estrictos de código e interfaces entre módulos.
* **Decisión y justificación:** Se adopta **Monolito Modular en capas**. Permite transacciones ACID locales inmediatas en PostgreSQL para resolver reservas sin sobreventa (DA01), reduce la sobrecarga de infraestructura y permite migrar módulos a microservicios en el futuro si la demanda lo exige.
* **Consecuencias:**
  * *Positivas:* Despliegue simple, desarrollo ágil, llamadas en memoria de latencia cero entre módulos, facilidad de refactorización y pruebas unificadas.
  * *Trade-offs:* Requiere disciplina estricta del equipo para no realizar consultas cruzadas directas entre bases de datos o esquemas privados de diferentes módulos.

---

### ADR02: Adopción del Enfoque Clean Architecture por Módulo
* **Estado:** Aprobado.
* **Drivers vinculados:** DA06 (Mantenibilidad), DA08 (Testabilidad), DA07 (Interoperabilidad).
* **Contexto y problema:** Las reglas de negocio de reservas (vencimiento de retención temporal, cálculo de cupos, políticas de cancelación) no deben estar acopladas a la base de datos (PostgreSQL), al framework web (ASP.NET Core) ni a librerías externas.
* **Alternativas evaluadas:**
  1. *Arquitectura tradicional N-Capas (Presentación $\rightarrow$ Negocio $\rightarrow$ Datos):* La capa de negocio suele depender directamente de los modelos ORM (Entity Framework), acoplando el dominio a la base de datos.
  2. *Clean Architecture / Arquitectura Hexagonal:* Invierte las dependencias: el Dominio se sitúa al centro, la Aplicación define puertos (interfaces) y la Infraestructura implementa adaptadores hacia afuera.
* **Decisión y justificación:** Se adopta **Clean Architecture**. Cada módulo organiza su código en cuatro capas: *Presentación*, *Aplicación*, *Dominio* e *Infraestructura*. El núcleo de dominio es puro (C# estándar) y no referencia librerías externas.
* **Consecuencias:**
  * *Positivas:* Alta testabilidad unitaria mediante mocks de repositorios, independencia tecnológica y reglas de negocio protegidas frente a cambios de frameworks.
  * *Trade-offs:* Mayor número de clases, interfaces, mapeadores y DTOs iniciales.

---

### ADR03: Selección del Stack Tecnológico Principal (ASP.NET Core 8+ y React + Vite)
* **Estado:** Aprobado.
* **Drivers vinculados:** DA01 (Consistencia), DA03 (Rendimiento), DA06 (Mantenibilidad), R02, R03.
* **Contexto y problema:** Se requiere un entorno backend con tipado estricto, excelente soporte de transacciones ACID y concurrencia asíncrona, combinado con un cliente web ágil para visualización gráfica de asientos de buses.
* **Alternativas evaluadas:**
  1. *Backend Node.js (Express/NestJS) + Frontend Angular:* Buena opción, pero C# provee mayor madurez en compilación estricta, control transaccional relacional y concurrencia sin bloqueo de event loop.
  2. *Backend ASP.NET Core 8+ (C#) + Frontend React 18+ (TypeScript, Vite):* Rendimiento sobresaliente en benchmarks I/O, soporte nativo de inyección de dependencias, tipado robusto de extremo a extremo con TypeScript y ecosistema maduro.
* **Decisión y justificación:** Se selecciona **ASP.NET Core 8+** para la API REST y **React + Vite** para la SPA web.
* **Consecuencias:**
  * *Positivas:* Máximo rendimiento en procesamiento transaccional, mantenibilidad corporativa y rica experiencia de usuario en selección de asientos de dos pisos.
  * *Trade-offs:* Requiere sincronización de contratos DTO entre C# y TypeScript (gestionable mediante esquemas OpenAPI/Swagger).

---

### ADR04: Selección de PostgreSQL como Motor de Base de Datos Principal y Fuente de Verdad
* **Estado:** Aprobado.
* **Drivers vinculados:** DA01 (Consistencia estricta), DA04 (Recuperabilidad), R01 (PostgreSQL).
* **Contexto y problema:** La venta de pasajes exige consistencia inmediata (ACID). No se puede tolerar eventual consistency en la reserva del último asiento disponible de un bus hacia Lima en Semana Santa.
* **Alternativas evaluadas:**
  1. *Base de datos NoSQL (MongoDB, DynamoDB):* Excelente escalabilidad horizontal, pero carece de bloqueos transaccionales pesimistas nativos y comprobaciones de integridad relacional complejas.
  2. *Base de datos Relacional PostgreSQL 16+:* Motor relacional open source líder, soporte completo de transacciones serializables, bloqueo de filas con `FOR UPDATE`, soporte JSONB e índices parciales condicionales.
* **Decisión y justificación:** Se adopta **PostgreSQL** como la única fuente de verdad transaccional del sistema.
* **Consecuencias:**
  * *Positivas:* Cero riesgo de asientos duplicados gracias a restricciones de exclusión e índices únicos parciales. Soporte de transacciones atómicas seguras.
  * *Trade-offs:* Escalamiento vertical prioritario en la base de datos de escritura; para consultas pesadas futuras se requerirán réplicas de lectura.

---

### ADR05: Mecanismo de Seguridad y Aislamiento Multiempresa
* **Estado:** Aprobado.
* **Drivers vinculados:** DA02 (Seguridad y aislamiento multiempresa), R04, R10.
* **Contexto y problema:** Múltiples empresas competidoras utilizan la misma plataforma física. Una empresa nunca debe poder leer, editar ni consultar información de otras empresas o agencias.
* **Alternativas evaluadas:**
  1. *Base de datos física independiente por empresa (Multi-database):* Máximo aislamiento, pero eleva drásticamente los costos de mantenimiento, migraciones y despliegue.
  2. *Base de datos compartida con esquema discriminador y Claims de autorización:* Todas las tablas operativas contienen `company_id` y `agency_id`. Los tokens JWT transportan los Claims de la empresa y los middlewares de ASP.NET Core junto con Global Query Filters de EF Core inyectan automáticamente el filtro en cada consulta SQL.
* **Decisión y justificación:** Se adopta **Base de datos compartida con aislamiento lógico estricto mediante Claims y Global Query Filters**.
* **Consecuencias:**
  * *Positivas:* Costo de infraestructura optimizado, mantenimiento centralizado y garantía de que ninguna consulta filtre datos ajenos a nivel de ORM.
  * *Trade-offs:* Las consultas SQL nativas directas deben auditarse rigurosamente para no omitir el parámetro de aislamiento.

---

### ADR06: Integración con Pasarela de Pagos mediante Adaptador e Idempotencia
* **Estado:** Aprobado.
* **Drivers vinculados:** DA01 (Consistencia de pagos), DA07 (Interoperabilidad externa), R05.
* **Contexto y problema:** La confirmación de pasajes pagados depende de webhooks enviados por pasarelas externas (Stripe, Culqi, Niubiz). Las pasarelas pueden reenviar notificaciones repetidas por caídas de red o demoras en la respuesta HTTP.
* **Alternativas evaluadas:**
  1. *Confirmar reserva directamente en la llamada cliente:* Vulnerable a fraude y manipulación de datos en el navegador.
  2. *Recepción de webhook sin validación de idempotencia:* Si la pasarela reintenta el webhook 3 veces, se corre el riesgo de emitir pasajes duplicados o generar inconsistencias en los libros contables.
  3. *Patrón Adapter con registro de idempotencia (`payment_transactions`):* Se verifica la firma criptográfica (HMAC) y se almacena el identificador único del evento (`idempotency_key`). Si el evento ya fue procesado, se responde HTTP 200 inmediatamente sin reprocesar.
* **Decisión y justificación:** Se adopta el **Patrón Adapter con tabla de eventos idempotentes**.
* **Consecuencias:**
  * *Positivas:* Cero pagos duplicados, desacoplamiento del proveedor externo y tolerancia ante fallos de conectividad.
  * *Trade-offs:* Requiere almacenamiento adicional de eventos de pago y verificación criptográfica obligatoria.

---

### ADR07: Desacoplamiento de Almacenamiento de Archivos (Object Storage)
* **Estado:** Aprobado.
* **Drivers vinculados:** DA05 (Escalabilidad), DA03 (Rendimiento).
* **Contexto y problema:** Las empresas deben subir logotipos, fotografías de buses, boletos PDF generados y fotografías de guías de encomiendas. Almacenar binarios (BLOBs) en PostgreSQL satura el tamaño de la base de datos y ralentiza los backups y consultas.
* **Alternativas evaluadas:**
  1. *Almacenamiento en disco local del servidor:* No escala horizontalmente si se tienen varias instancias de la API en contenedores.
  2. *Almacenar BLOBs en PostgreSQL:* Degrada el rendimiento de lectura de la base de datos y triplica el tamaño de los respaldos.
  3. *Almacenamiento de objetos compatible con S3 (MinIO / AWS S3):* Los archivos binarios se almacenan en un bucket y en PostgreSQL solo se guardan las URLs o identificadores con metadatos.
* **Decisión y justificación:** Se adopta **Object Storage compatible S3**.
* **Consecuencias:**
  * *Positivas:* Base de datos relacional ligera y ágil, soporte para URLs firmadas temporales para descarga segura de pasajes.
  * *Trade-offs:* Requiere configurar un servicio de almacenamiento adicional (MinIO para desarrollo / S3 para producción).

---

### ADR08: Estrategia de Caché y Optimización Progresiva (YAGNI / KISS)
* **Estado:** Aprobado.
* **Drivers vinculados:** DA03 (Rendimiento), DA05 (Escalabilidad).
* **Contexto y problema:** En fases iniciales, la carga de consultas de rutas y catálogos puede ser resuelta directamente por PostgreSQL con índices optimizados. Agregar Redis desde el día 1 incrementa la complejidad operativa sin necesidad justificada.
* **Alternativas evaluadas:**
  1. *Caché Redis distribuido obligatorio desde el día 1:* Aporta alta velocidad, pero añade otro contenedor a mantener, sincronización de invalidación de caché y riesgo de mostrar asientos desactualizados.
  2. *Caché en memoria en ASP.NET Core (`IMemoryCache`) para catálogos estáticos + PostgreSQL indexado para disponibilidad:* Mantiene la simplicidad; Redis se reserva como táctica de escalabilidad futura cuando las métricas demuestren saturación.
* **Decisión y justificación:** Se adopta **Caché en memoria para datos estáticos (ciudades, terminales) y PostgreSQL con índices para disponibilidad en tiempo real**. Redis queda documentado como táctica arquitectónica de evolución.
* **Consecuencias:**
  * *Positivas:* Simplicidad operativa inicial, cero problemas de inconsistencia de caché en disponibilidad de asientos.
  * *Trade-offs:* En picos extremos de consulta, la base de datos absorberá las lecturas si no se escala con réplicas.

---

## 3. Matriz de Cobertura de Drivers vs. ADRs

| Driver Arquitectónico | ADR01 (Monolito) | ADR02 (Clean Arch) | ADR03 (Stack) | ADR04 (Postgres) | ADR05 (Seguridad) | ADR06 (Pagos) | ADR07 (Storage) | ADR08 (Caché) |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **DA01 — Consistencia de reservas y pagos** | ✓ | | ✓ | ✓ | | ✓ | | |
| **DA02 — Seguridad multiempresa** | | | | | ✓ | | | |
| **DA03 — Rendimiento en búsquedas** | | | ✓ | ✓ | | | ✓ | ✓ |
| **DA04 — Disponibilidad y recuperación** | ✓ | | | ✓ | | ✓ | | |
| **DA05 — Escalabilidad en picos** | ✓ | | ✓ | | | | ✓ | ✓ |
| **DA06 — Mantenibilidad modular** | ✓ | ✓ | ✓ | | | | | |
| **DA07 — Interoperabilidad externa** | | ✓ | | | | ✓ | ✓ | |
| **DA08 — Observabilidad y testabilidad**| | ✓ | ✓ | ✓ | ✓ | ✓ | | |
