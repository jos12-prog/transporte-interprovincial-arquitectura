# Tecnologías, Implementación y Despliegue

## 1. Principio rector (Paso 11)

> **La tecnología implementa decisiones: no define por sí sola la arquitectura.**
>
> Las tecnologías seleccionadas son el medio para concretar los estilos, enfoques y tácticas arquitectónicas decididas en fases previas para satisfacer los drivers del sistema. No se elige una arquitectura porque se usa una tecnología; se selecciona la tecnología adecuada para materializar las decisiones de diseño.

---

## 2. Stack tecnológico del sistema

| Capa / Función | Tecnología seleccionada | Justificación arquitectónica | Decisión relacionada |
|---|---|---|:---:|
| **Frontend Web** | React 18 + TypeScript + Vite | Interfaz SPA reactiva con tipado estricto. Vite provee compilación rápida. TypeScript previene errores en la selección de asientos y operaciones multiagencia. | DA-03 |
| **Estilos e interfaz** | Tailwind CSS | Diseño adaptable y responsive para pasajeros (web) y agentes (ventanilla). | DA-03 |
| **Backend API** | ASP.NET Core 8 (C#) | Plataforma de alto rendimiento con soporte nativo de Inyección de Dependencias, concurrencia asíncrona y middlewares seguros. | DA-01, DA-02, DA-14 |
| **Acceso a datos** | Entity Framework Core 8 + Npgsql | ORM con soporte para Global Query Filters (aislamiento multiempresa), migraciones y consultas SQL nativas con bloqueo pesimista. | DA-04, DA-05 |
| **Base de datos** | PostgreSQL 16+ | Motor relacional con soporte ACID estricto, índices únicos condicionales para evitar sobreventa (`idx_unique_active_seat`) y transacciones serializables. | DA-04, DA-07 |
| **Caché** | Redis | Almacén en memoria para consultas frecuentes de rutas, horarios y servicios. Componente obligatorio desde la primera versión (RC12). | DA-09, DA-11 |
| **Cola de mensajería** | RabbitMQ | Cola durable para procesamiento asíncrono de notificaciones y eventos secundarios. Componente obligatorio desde la primera versión (RC13). | DA-10 |
| **Proxy y balanceador** | Nginx | Servidor perimetral: terminación TLS 1.3 (HTTPS), compresión Gzip/Brotli, rate limiting y balanceo de carga entre contenedores. | DA-01, DA-05, DA-14 |
| **Contenedores** | Docker y Docker Compose | Empaquetamiento estándar, reproducible y portable de todos los entornos: desarrollo, pruebas y producción. | DA-14 |
| **Control de versiones** | Git y GitHub | Repositorio centralizado de código y documentación arquitectónica con control de ramas. | — |
| **Pipeline CI/CD** | GitHub Actions | Automatización de compilación, pruebas unitarias y construcción de imágenes Docker en cada integración. | DA-15 |
| **Observabilidad** | Serilog + Health Checks | Logs estructurados en formato JSON para trazabilidad y auditoría. Endpoints de estado (`/health`) para monitoreo. | DA-12, DA-15 |
| **Pasarela de pagos** | Culqi (mediante Adapter) | Pasarela orientada al contexto peruano, integrada mediante el patrón Adapter (`IPaymentGateway` → `CulqiPaymentAdapter`). Permite cambio futuro sin modificar el dominio. | DA-09, DA-11 |
| **Notificaciones** | WhatsApp Business Cloud API (mediante Adapter) | Canal de comunicación para confirmaciones, recordatorios, cancelaciones y alertas preventivas. Procesado de forma asíncrona mediante RabbitMQ. | DA-10, DA-12 |

---

## 3. Implementación y despliegue (Paso 12)

### 3.1. Justificación del despliegue monolítico modular

```mermaid
flowchart TD
    subgraph OPCIONES["Estrategias de despliegue evaluadas"]
        M["<b>Monolito Modular</b><br/>(1 solo despliegue)<br/>• Rápido crecimiento<br/>• Menor costo de operación<br/>• Transacciones ACID locales<br/>• Límites modulares claros"]
        MS["<b>Microservicios</b><br/>(Varios despliegues independientes)<br/>• Alta complejidad de red<br/>• Transacciones distribuidas (Saga)<br/>• Alto costo de infraestructura<br/>• No justificado en el alcance inicial"]
    end

    classDef mon fill:#E8F5E9,stroke:#4CAF50,color:#1B5E20
    classDef mic fill:#FFEBEE,stroke:#E57373,color:#B71C1C
    class M mon
    class MS mic
```

Para la Plataforma Integrada de Transporte Interprovincial se selecciona el **Monolito Modular** porque:

- **Despliegue unificado:** todos los módulos funcionales se compilan en un único binario ejecutable alojado en un contenedor Docker.
- **Consistencia garantizada:** las operaciones críticas de reserva y pago se coordinan dentro de la misma transacción PostgreSQL, eliminando la necesidad de consistencia eventual entre servicios.
- **Menor complejidad operacional:** evita la gestión de múltiples tuberías de despliegue y comunicación entre servicios independientes.
- **Evolución posible:** los límites modulares bien definidos permiten extraer un módulo como servicio independiente si el crecimiento futuro lo justifica.

---

### 3.2. Diagrama de despliegue físico

```mermaid
flowchart TB
    CLIENTE["Navegador Web / Dispositivo Móvil<br/>(Pasajero, Agente, Administrador de Agencia, Superadministrador)"]

    subgraph SERVIDOR["Servidor de Producción (Linux / Cloud VPS)"]
        NGINX["Proxy Inverso y Balanceador<br/><b>Nginx (Puerto 443 — HTTPS / TLS 1.3)</b>"]

        subgraph RED_DOCKER["Red Interna Docker (Bridge Privado)"]
            FRONT["Contenedor Frontend<br/><b>React SPA (Static Web Assets)</b>"]

            subgraph BACKEND_POOL["Pool de Backend (Monolito Modular)"]
                API1["Contenedor API 1<br/><b>ASP.NET Core 8</b>"]
                API2["Contenedor API 2 (Réplica)<br/><b>ASP.NET Core 8</b>"]
            end

            DB["Contenedor Base de Datos<br/><b>PostgreSQL 16 (Puerto 5432)</b><br/>Volumen Persistente"]
            DB_REPLICA["Contenedor Réplica de Lectura<br/><b>PostgreSQL 16 (Standby)</b><br/>Reportes y Auditoría"]
            REDIS["Contenedor Caché<br/><b>Redis (Puerto 6379)</b><br/>Consultas frecuentes"]
            RABBIT["Contenedor Cola de Mensajería<br/><b>RabbitMQ (Puerto 5672)</b><br/>Eventos asíncronos"]
        end
    end

    EXT_PAGOS["Pasarela de Pagos Externa<br/><b>Culqi</b><br/>(mediante CulqiPaymentAdapter)"]
    EXT_NOTIF["Proveedor de Notificaciones<br/><b>WhatsApp Business Cloud API</b><br/>(mediante WhatsAppAdapter)"]

    CLIENTE -->|"HTTPS / TLS 1.3"| NGINX
    NGINX -->|"/ (recursos estáticos)"| FRONT
    NGINX -->|"/api/v1 (proxy pass balanceado)"| BACKEND_POOL
    BACKEND_POOL -->|"TCP / Npgsql con Pool"| DB
    DB -->|"Streaming Replication"| DB_REPLICA
    BACKEND_POOL -->|"Lectura de reportes"| DB_REPLICA
    BACKEND_POOL -->|"TCP (caché)"| REDIS
    BACKEND_POOL -->|"AMQP (publicar eventos)"| RABBIT
    RABBIT -->|"AMQP (consumir eventos)"| BACKEND_POOL
    BACKEND_POOL -->|"HTTPS / REST"| EXT_PAGOS
    BACKEND_POOL -->|"HTTPS / REST"| EXT_NOTIF

    classDef net fill:#E3F2FD,stroke:#1E88E5,color:#0D47A1
    classDef comp fill:#E8F5E9,stroke:#43A047,color:#1B5E20
    classDef ext fill:#FFF3E0,stroke:#FB8C00,color:#E65100
    class NGINX,FRONT,API1,API2,DB,DB_REPLICA,REDIS,RABBIT net
    class CLIENTE comp
    class EXT_PAGOS,EXT_NOTIF ext
```

---

## 4. Pipeline de CI/CD y flujo operativo

```mermaid
sequenceDiagram
    autonumber
    actor Dev as Desarrollador
    participant GH as GitHub (Repositorio)
    participant GA as GitHub Actions (CI/CD)
    participant Reg as Docker Container Registry
    participant Srv as Servidor Docker
    participant Mon as Monitoreo (Serilog / Health Checks)

    Dev->>GH: git push / Pull Request a rama main
    GH->>GA: Dispara Workflow Automatizado
    activate GA
    GA->>GA: 1. Compilación y Linter (dotnet build + ESLint)
    GA->>GA: 2. Ejecutar pruebas unitarias de casos de uso
    GA->>GA: 3. Construir imagen Docker del Monolito
    GA->>Reg: 4. Publicar imagen etiquetada (v1.0.X)
    deactivate GA
    Reg->>Srv: 5. Despliegue con Docker Compose (rolling update)
    Srv->>Srv: Reinicia contenedor sin interrumpir servicio
    Srv->>Mon: 6. Emisión de logs estructurados y Health Checks
    Mon-->>Dev: Alertas y métricas de rendimiento en tiempo real
```

1. **Código en GitHub:** todo cambio de código fuente y documentación se versiona en el repositorio Git.
2. **Integración Continua (CI):** GitHub Actions compila el backend y el frontend y ejecuta pruebas unitarias de los casos de uso principales. Si una prueba falla, el pipeline se detiene.
3. **Construcción de contenedor:** se genera la imagen optimizada con Docker multi-stage build.
4. **Despliegue Continuo (CD):** se actualiza el contenedor en el servidor mediante Docker Compose sin tiempo de inactividad visible.
5. **Monitoreo y operación:** se recolectan métricas de uso y logs de auditoría para supervisar el estado del sistema.

---

## 5. Relación tecnologías — decisiones arquitectónicas

| Tecnología | Táctica que implementa | Decisión arquitectónica |
|---|---|---|
| PostgreSQL 16 | Bloqueo pesimista, transacciones ACID, replicación | DA-04, DA-07 |
| Redis | Caché de consultas frecuentes | DA-09 |
| RabbitMQ | Colas y procesamiento asíncrono | DA-10 |
| ASP.NET Core 8 + Polly | Circuit Breaker y reintentos | DA-09, DA-10 |
| ASP.NET Core 8 + JWT | Autenticación y autorización RBAC | DA-06 |
| ASP.NET Core 8 + Rate Limiting | Rate Limiting | DA-04 |
| Nginx | Balanceo de carga, terminación TLS | DA-01, DA-05 |
| Culqi (CulqiPaymentAdapter) | Patrón Adapter para pagos | DA-11 |
| WhatsApp Business Cloud API (WhatsAppAdapter) | Patrón Adapter para notificaciones | DA-12 |
| Serilog | Auditoría y logs estructurados | DA-13, DA-15 |
| Docker + Docker Compose | Despliegue reproducible del monolito | DA-01 |
| GitHub Actions | Pipeline CI/CD | DA-15 |
