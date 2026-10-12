# Estilo Arquitectónico

## 1. Descripción

La **Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial** adopta una arquitectura organizada en capas, implementada inicialmente como un **monolito modular**.

La solución separa las responsabilidades de presentación, aplicación, dominio e infraestructura, permitiendo mantener una estructura organizada y facilitar la evolución progresiva del sistema.

A nivel de despliegue, la aplicación podrá disponer de múltiples instancias distribuidas mediante un balanceador de carga para responder al crecimiento de usuarios y operaciones.

---

## 2. Estilo seleccionado

Se selecciona una **arquitectura en capas con organización modular**.

La aplicación se mantiene inicialmente como una única unidad desplegable, pero internamente se divide en módulos funcionales con responsabilidades claramente definidas.

Los principales módulos son:

- Usuarios.
- Empresas y agencias.
- Servicios.
- Reservas.
- Pagos.
- Notificaciones.
- Reportes.

Esta organización permite evitar la complejidad inicial de una arquitectura de microservicios, manteniendo al mismo tiempo separación de responsabilidades y capacidad de evolución.

---

## 3. Organización general

La arquitectura se organiza en los siguientes niveles:

### Capa de Presentación

Responsable de la interacción con los usuarios de la plataforma mediante la interfaz web.

Actores principales:

- Pasajero.
- Agente / Vendedor.
- Administrador de Agencia.
- Superadministrador.

### Capa de Aplicación

Coordina los casos de uso y operaciones de la plataforma.

Incluye funcionalidades relacionadas con:

- gestión de usuarios;
- gestión de empresas y agencias;
- gestión de servicios;
- búsqueda y disponibilidad;
- gestión de reservas;
- procesamiento de pagos;
- notificaciones;
- reportes.

### Capa de Dominio

Contiene las reglas principales del negocio relacionadas con:

- pasajeros;
- empresas;
- agencias;
- rutas;
- servicios;
- unidades;
- cupos o asientos;
- reservas;
- pagos.

Esta capa concentra las reglas que deben mantenerse independientes de tecnologías específicas.

### Capa de Infraestructura

Contiene los componentes tecnológicos necesarios para soportar la aplicación, entre ellos:

- PostgreSQL para persistencia;
- Redis para caché;
- cola de eventos para procesamiento asíncrono;
- integración con pasarela de pago;
- servicio de notificaciones;
- mecanismos de auditoría y monitoreo;
- respaldo y recuperación.

---

## 4. Relación con los drivers arquitectónicos

| Driver | Influencia sobre el estilo |
|---|---|
| **DA01 – Consistencia y concurrencia** | Requiere separar claramente la lógica de reservas y mantener persistencia transaccional. |
| **DA02 – Rendimiento** | Justifica el uso de caché para consultas frecuentes. |
| **DA03 – Escalabilidad** | Justifica una organización modular y la posibilidad de escalamiento horizontal. |
| **DA04 – Disponibilidad** | Justifica mecanismos de balanceo, monitoreo y recuperación. |
| **DA05 – Seguridad** | Requiere autenticación, autorización y protección de las comunicaciones. |
| **DA06 – Integración de pagos** | Requiere desacoplar la lógica del sistema de servicios externos. |
| **DA07 – Procesamiento asíncrono** | Justifica el uso de una cola de eventos para tareas secundarias. |
| **DA08 – Trazabilidad** | Requiere auditoría, logs y monitoreo. |
| **DA09 – API REST** | Define la comunicación entre la presentación y la aplicación. |
| **DA10 – Mantenibilidad** | Justifica la modularidad y la separación de responsabilidades. |

---

## 5. Diagrama del estilo arquitectónico

El siguiente diagrama representa la **arquitectura en capas implementada como un monolito modular** para la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial.

```mermaid
%%{init: {
  "theme": "base",
  "flowchart": { "curve": "linear", "nodeSpacing": 25, "rankSpacing": 50, "htmlLabels": true },
  "themeVariables": {
    "fontFamily": "Arial, sans-serif",
    "fontSize": "12px",
    "lineColor": "#444",
    "primaryTextColor": "#111",
    "clusterBkg": "#ffffff",
    "clusterBorder": "#555"
  }
}}%%
flowchart TB

%% ================= ACTORES =================
PAS["Pasajero"]
AGE["Agente / Vendedor"]
ADM["Administrador de Agencia"]
SUP["Superadministrador"]

WEB["<b>Cliente Web</b><br/>Navegador · HTML / CSS / JavaScript"]

PAS --> WEB
AGE --> WEB
ADM --> WEB
SUP --> WEB

%% ================= MONOLITO =================
subgraph BACKEND["«monolito modular» Backend — una sola aplicación · un solo proceso · un solo despliegue"]
direction TB

MW["<b>Middlewares transversales</b><br/>Autenticación · Autorización · Validación · Manejo de errores · Auditoría"]

%% ---------- CAPA 1 ----------
subgraph CAPA1["1. CAPA DE PRESENTACIÓN"]
U1["<b>Usuarios</b><br/>Controller / API"]
E1["<b>Empresas y Agencias</b><br/>Controller / API"]
S1["<b>Servicios</b><br/>Controller / API"]
R1["<b>Reservas</b><br/>Controller / API"]
P1["<b>Pagos</b><br/>Controller / API"]
N1["<b>Notificaciones</b><br/>Controller / API"]
REP1["<b>Reportes</b><br/>Controller / API"]
end

%% ---------- CAPA 2 ----------
subgraph CAPA2["2. CAPA DE LÓGICA DE NEGOCIO"]
U2["Registro<br/>Login · Roles"]
E2["Empresas<br/>Agencias"]
S2["Rutas · Horarios<br/>Tarifas"]
R2["Disponibilidad<br/>Reserva · Confirmación"]
P2["Procesamiento<br/>Validación"]
N2["Correo · SMS<br/>Push"]
REP2["Consultas<br/>Estadísticas"]
end

%% ---------- CAPA 3 ----------
subgraph CAPA3["3. CAPA DE DATOS"]
U3["Repositorio<br/>Usuarios"]
E3["Repositorio<br/>Empresas"]
S3["Repositorio<br/>Servicios"]
R3["Repositorio<br/>Reservas"]
P3["Repositorio<br/>Pagos"]
N3["Repositorio<br/>Notificaciones"]
REP3["Repositorio<br/>Reportes"]
end

DATA["<b>Acceso a datos compartido</b><br/>ORM · Modelos · Pool de conexiones"]

MW --> CAPA1

U1 --> U2 --> U3
E1 --> E2 --> E3
S1 --> S2 --> S3
R1 --> R2 --> R3
P1 --> P2 --> P3
N1 --> N2 --> N3
REP1 --> REP2 --> REP3

U3 --> DATA
E3 --> DATA
S3 --> DATA
R3 --> DATA
P3 --> DATA
N3 --> DATA
REP3 --> DATA
end

%% ================= EXTERNOS =================
DB[("<b>PostgreSQL</b><br/>Base de datos transaccional")]
PAGO["<b>Pasarela de pago</b><br/>Servicio externo"]
NOTIF["<b>Servicio de notificaciones</b><br/>Email · SMS · Push"]

WEB -->|"HTTPS · JSON · API REST"| MW
DATA -->|"SQL · TCP"| DB
P2 -->|"HTTPS / REST"| PAGO
N2 -->|"HTTPS / REST"| NOTIF

%% ================= ESTILOS =================
classDef actor fill:#fff,stroke:#333,color:#111;
classDef cliente fill:#fff,stroke:#333,stroke-width:2px,color:#111;
classDef middleware fill:#dbe7fb,stroke:#5b7fb8,color:#111;
classDef pres fill:#fff,stroke:#4a6fa5,color:#111;
classDef neg fill:#dff0d8,stroke:#6b9a5b,color:#111;
classDef dat fill:#fff,stroke:#d39e42,color:#111;
classDef acceso fill:#fbe3b0,stroke:#d39e42,stroke-width:2px,color:#111;
classDef ext fill:#e6e6e6,stroke:#666,color:#111;
classDef db fill:#fff,stroke:#333,stroke-width:2px,color:#111;

class PAS,AGE,ADM,SUP actor;
class WEB cliente;
class MW middleware;
class U1,E1,S1,R1,P1,N1,REP1 pres;
class U2,E2,S2,R2,P2,N2,REP2 neg;
class U3,E3,S3,R3,P3,N3,REP3 dat;
class DATA acceso;
class PAGO,NOTIF ext;
class DB db;

style BACKEND fill:#fff,stroke:#333,stroke-width:2px,stroke-dasharray:6 4,color:#111
style CAPA1 fill:#e8eefb,stroke:#8aa4d6,color:#111
style CAPA2 fill:#e9f5e4,stroke:#9cc48c,color:#111
style CAPA3 fill:#fdf1dc,stroke:#e0b866,color:#111
```

## 6. Justificación

La arquitectura en capas permite separar las responsabilidades del sistema y mantener una organización comprensible.

El **monolito modular** permite que las funcionalidades se encuentren separadas internamente sin introducir inicialmente la complejidad operativa de una arquitectura de microservicios.

Además, la aplicación puede escalar horizontalmente mediante múltiples instancias distribuidas a través de un balanceador de carga cuando la demanda lo requiera.

La organización interna será complementada mediante **Clean Architecture**, con el objetivo de controlar las dependencias entre el dominio, los casos de uso, los adaptadores y la infraestructura.