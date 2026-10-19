# Tecnologías, Implementación y Despliegue

## 1. Principio Rector (Paso 11 de la Docente)

> [!IMPORTANT]
> **Máxima de la docente:**
> *"La tecnología implementa decisiones: no define por sí sola la arquitectura."*
> 
> Las tecnologías seleccionadas son el medio para concretar los estilos, enfoques y tácticas arquitectónicas decididas en fases previas para satisfacer los drivers del sistema. No se elige una arquitectura porque se usa una tecnología, sino que se selecciona la tecnología adecuada para materializar las decisiones de diseño.

---

## 2. Stack Tecnológico del Sistema y su Justificación

| Capa / Función | Tecnología Seleccionada | Justificación Arquitectónica | Decisión / ADR Relacionado |
|---|---|---|:---:|
| **Frontend Web** | **React 18+ con TypeScript y Vite** | Interfaz reactiva tipo SPA (Single Page Application). Vite provee compilación ultrarrápida; TypeScript aporta tipado estricto que previene errores en la selección de asientos de 1 o 2 pisos. | ADR02, ADR03 |
| **Estilos & UI** | **Tailwind CSS y componentes accesibles** | Diseño adaptable y responsive para dispositivos móviles de pasajeros y pantallas optimizadas de ventanilla con atajos para agentes. | ADR02 |
| **Backend API** | **ASP.NET Core 8+ (C#)** | Plataforma de código abierto multiplataforma de altísimo rendimiento I/O. Soporte nativo de Inyección de Dependencias, concurrencia asíncrona sin sobrecarga y middlewares seguros. | ADR01, ADR03 |
| **Acceso a Datos** | **Entity Framework Core 8 + Npgsql** | Mapeo relacional objeto (ORM) con soporte para Global Query Filters (aislamiento multiempresa), migraciones automáticas y consultas SQL nativas con bloqueo pesimista. | ADR02, ADR04, ADR05 |
| **Base de Datos** | **PostgreSQL 16+** | Motor relacional robusto con soporte ACID estricto, índices únicos condicionales para evitar sobreventa de asientos (`idx_unique_active_seat`) y transacciones serializables. | ADR04 (DA01) |
| **Almacén de Objetos** | **Cloud Object Storage (compatible S3 / MinIO)** | Almacenamiento desacoplado para fotografías de buses, comprobantes digitalizados y guías de encomiendas, evitando engrosar la base de datos relacional. | ADR07 |
| **Caché** | **IMemoryCache (inicial) / Redis (evolución)** | Reducción de latencia en consultas repetitivas de catálogos y rutas fijas. | ADR08 (Táctica 1) |
| **Proxy & Balanceador** | **Nginx** | Servidor web perimetral de alto rendimiento: terminación TLS 1.3 (HTTPS), compresión Gzip/Brotli, rate limiting y balanceo de carga entre contenedores. | ADR11 (Táctica 5) |
| **Contenedores** | **Docker & Docker Compose** | Empaquetamiento estándar, reproducible y portable de los entornos de desarrollo, pruebas y producción sobre Linux. | ADR11 |
| **Control de Versiones** | **Git & GitHub** | Repositorio centralizado de código y documentación arquitectónica con control estricto de ramas y revisiones. | R07 |
| **Pipeline CI/CD** | **GitHub Actions** | Automatización de compilación, ejecución de pruebas unitarias de casos de uso y empaquetamiento de imágenes Docker en cada integración. | DA08 |
| **Observabilidad** | **Serilog + Health Checks** | Logs estructurados en formato JSON para trazabilidad y auditoría de eventos de negocio y endpoints de estado (`/health`). | DA08 |

---

## 3. Modelo de Implementación y Despliegue (Paso 12 de la Docente)

### 3.1. Justificación del Despliegue Monolítico Modular

La docente resalta una distinción clave entre las estrategias de despliegue:

```mermaid
flowchart TD
    subgraph OPCIONES["Estrategias de Despliegue"]
        M["<b>Monolito Modular</b><br/>(1 solo despliegue)<br/>• Rápido crecimiento<br/>• Menor costo de operación<br/>• Menos componentes de red<br/>• Transacciones ACID locales"]
        MS["<b>Microservicios</b><br/>(Varios despliegues independientes)<br/>• Alta complejidad de red<br/>• Transacciones distribuidas (Saga)<br/>• Alto costo de infraestructura"]
    end

    classDef mon fill:#E8F5E9,stroke:#4CAF50,color:#1B5E20
    classDef mic fill:#FFEBEE,stroke:#E57373,color:#B71C1C
    class M mon
    class MS mic
```

Para la **Plataforma Integrada de Transporte Interprovincial**, se elige el **Monolito Modular**:
* **Despliegue unificado:** Todos los 8 módulos de negocio (MI01 a MI08) se compilan en un único binario ejecutable alojado en un contenedor Docker.
* **Crecimiento rápido con menor operación:** Evita la sobrecarga de gestionar un clúster Kubernetes o múltiples tuberías de despliegue independientes.
* **Garantía de consistencia:** Permite que las operaciones críticas de reserva y pago se coordinen dentro de la misma transacción en PostgreSQL, eliminando la necesidad de lidiar con consistencia eventual entre microservicios.

---

### 3.2. Diagrama de Despliegue Físico en Servidor

```mermaid
flowchart TB
    CLIENTE["Navegador Web / Dispositivo Móvil<br/>(Pasajero, Agente, Administrador)"]

    subgraph SERVIDOR["Servidor de Producción (Linux / Cloud VPS)"]
        NGINX["Proxy Inverso & Balanceador<br/><b>Nginx (Puerto 443 HTTPS / TLS 1.3)</b>"]

        subgraph RED_DOCKER["Red Interna Docker (Bridge Privado)"]
            FRONT["Contenedor Frontend<br/><b>React SPA (Static Web Assets)</b>"]
            
            subgraph BACKEND_POOL["Pool de Backend (Monolito Modular)"]
                API1["Contenedor API 1<br/><b>ASP.NET Core 8</b>"]
                API2["Contenedor API 2 (Réplica)<br/><b>ASP.NET Core 8</b>"]
            end

            DB["Contenedor Base de Datos<br/><b>PostgreSQL 16 (Puerto 5432)</b><br/>Volumen Persistente"]
            OBJ["Almacén de Objetos<br/><b>MinIO / S3 Storage</b><br/>Volumen Persistente"]
        end
    end

    EXT_PAGOS["Pasarela de Pagos Externa<br/>(Stripe / Culqi / Niubiz)"]
    EXT_NOTIF["Proveedor Notificaciones<br/>(Servicio Email / SMS)"]

    CLIENTE -->|"HTTPS / TLS 1.3"| NGINX
    NGINX -->|"/ (recursos estáticos)"| FRONT
    NGINX -->|"/api/v1 (proxy pass balanceado)"| BACKEND_POOL
    BACKEND_POOL -->|"TCP / Npgsql con Pool"| DB
    BACKEND_POOL -->|"S3 API"| OBJ
    BACKEND_POOL -->|"HTTPS / REST"| EXT_PAGOS
    BACKEND_POOL -->|"HTTPS / REST"| EXT_NOTIF

    classDef net fill:#E3F2FD,stroke:#1E88E5,color:#0D47A1
    classDef comp fill:#E8F5E9,stroke:#43A047,color:#1B5E20
    classDef ext fill:#FFF3E0,stroke:#FB8C00,color:#E65100
    class NGINX,FRONT,API1,API2,DB,OBJ net
    class CLIENTE comp
    class EXT_PAGOS,EXT_NOTIF ext
```

---

## 4. Pipeline de CI/CD y Flujo Operativo

El ciclo de desarrollo y puesta en producción sigue el flujo formal ilustrado por la docente:

```mermaid
sequenceDiagram
    autonumber
    actor Dev as Desarrollador
    participant GH as GitHub (Repositorio)
    participant GA as GitHub Actions (CI/CD)
    participant Reg as Docker Container Registry
    participant Srv as Servidor Docker / AWS
    participant Mon as Monitoreo (Logs / Métricas)

    Dev->>GH: git push / Pull Request a rama main
    GH->>GA: Dispara Workflow Automatizado
    activate GA
    GA->>GA: 1. Compilación y Linter (Dotnet & ESLint)
    GA->>GA: 2. Ejecutar Pruebas Unitarias de Clean Architecture
    GA->>GA: 3. Construir Imagen Docker del Monolito
    GA->>Reg: 4. Publicar Imagen Etiquetada (v1.0.X)
    deactivate GA
    Reg->>Srv: 5. Despliegue con Docker Compose (Rolling update)
    Srv->>Srv: Reinicia contenedor sin interrumpir servicio
    Srv->>Mon: 6. Emisión de logs estructurados y Health Checks
    Mon-->>Dev: Alertas y métricas de rendimiento en tiempo real
```

1. **Código en GitHub:** Todo cambio de código fuente y diagramas seVersiona en el repositorio Git.
2. **Integración Continua (CI):** GitHub Actions compila el backend y frontend y corre pruebas unitarias de casos de uso (ej. `CrearReserva`, `ConfirmarReserva`). Si una prueba falla, el pipeline aborta.
3. **Construcción y Registro de Contenedores:** Se construye la imagen optimizada con Docker multi-stage build.
4. **Despliegue Continuo (CD):** Se actualiza el contenedor en el servidor de destino mediante Docker Compose sin tiempo de inactividad visible.
5. **Monitoreo y Operación:** Se recolectan métricas de uso y logs de auditoría para supervisar la salud del sistema.
