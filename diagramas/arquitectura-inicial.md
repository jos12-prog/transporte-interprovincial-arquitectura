# Diagrama de Arquitectura Inicial

```mermaid
flowchart TB

    subgraph ACT["ACTORES"]
        P["Pasajero"]
        AV["Agente / Vendedor"]
        AA["Administrador de Agencia"]
        SA["Superadministrador"]
    end

    subgraph PRE["CAPA DE PRESENTACIÓN"]
        WEB["Aplicación Web"]
        UI["Interfaces según Rol"]
    end

    subgraph ACC["ACCESO Y SEGURIDAD"]
        HTTPS["Firewall / HTTPS"]
        GW["API Gateway / Balanceador"]
    end

    subgraph NEG["LÓGICA DE NEGOCIO / SERVICIOS"]
        AUTH["Usuarios y Permisos"]
        EMP["Empresas y Agencias"]
        RUT["Rutas y Horarios"]
        UNI["Unidades"]
        DISP["Consulta y Disponibilidad"]
        RES["Reservas"]
        PAG["Ventas y Pagos"]
        CR["Cancelaciones y Reprogramaciones"]
        NOT["Notificaciones"]
        REP["Reportes"]
        AUD["Auditoría"]
    end

    subgraph DAT["DATOS E INFRAESTRUCTURA"]
        DB[("PostgreSQL")]
        CACHE[("Caché")]
        QUEUE["Cola de Eventos"]
        MON["Backup / Monitoreo"]
    end

    subgraph EXT["SISTEMAS EXTERNOS"]
        PAYMENT["Pasarela de Pago"]
        MSG["Email / SMS / Push"]
    end

    ACT --> PRE
    PRE --> HTTPS
    HTTPS --> GW
    GW --> NEG

    NEG --> DB

    DISP --> CACHE

    NOT --> QUEUE
    REP --> QUEUE

    PAG --> PAYMENT
    QUEUE --> MSG

    DB --> MON
    NEG --> MON
```

## Interpretación

La arquitectura representa una solución distribuida organizada mediante capas y servicios. Los actores interactúan con la capa de presentación, mientras que el acceso a las funcionalidades se realiza mediante mecanismos de seguridad y un API Gateway o balanceador.

La lógica de negocio concentra las responsabilidades principales relacionadas con usuarios, empresas, agencias, rutas, unidades, disponibilidad, reservas, pagos, reprogramaciones, notificaciones, reportes y auditoría.

PostgreSQL mantiene la información transaccional, mientras que la caché permite apoyar las consultas frecuentes y la cola de eventos permite desacoplar procesos asíncronos. Finalmente, la plataforma se integra con servicios externos para pagos y notificaciones.