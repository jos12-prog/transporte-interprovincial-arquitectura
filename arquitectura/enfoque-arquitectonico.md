# Enfoque Arquitectónico

## 1. Descripción

Para la **Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial** se adopta **Clean Architecture** como enfoque arquitectónico.

Este enfoque complementa el estilo arquitectónico definido previamente y permite organizar las responsabilidades internas del sistema de manera que las reglas principales del negocio permanezcan independientes de tecnologías específicas como bases de datos, frameworks, servicios externos o mecanismos de comunicación.

La regla principal establece que las dependencias deben orientarse hacia el núcleo de la aplicación.

---

## 2. Enfoque seleccionado: Clean Architecture

La solución se organiza en cuatro niveles principales:

### 2.1 Entidades / Dominio

Representa el núcleo de la aplicación y contiene las reglas principales del negocio.

Principales elementos:

- Usuario.
- Empresa de transporte.
- Agencia.
- Ruta.
- Unidad de transporte.
- Servicio programado.
- Asiento o cupo.
- Reserva.
- Pago.

En esta capa se encuentran las reglas críticas del sistema, especialmente aquellas relacionadas con disponibilidad, reservas y prevención de sobreventa.

---

### 2.2 Casos de Uso / Aplicación

Contiene las operaciones que la plataforma permite realizar.

Principales casos de uso:

- autenticar usuario;
- gestionar empresas y agencias;
- gestionar rutas y servicios;
- consultar servicios disponibles;
- consultar disponibilidad;
- crear reserva;
- confirmar reserva;
- cancelar o reprogramar reserva;
- procesar pago;
- enviar notificaciones;
- generar reportes.

Esta capa coordina las reglas del dominio para cumplir los requerimientos funcionales del sistema.

---

### 2.3 Adaptadores de Interfaz

Permiten comunicar los casos de uso con los mecanismos externos de entrada y salida.

Incluye:

- Controllers de la API REST.
- DTO y validadores.
- Interfaces de repositorios.
- Adaptadores de persistencia.
- Adaptador de pasarela de pago.
- Adaptador del servicio de notificaciones.

Los adaptadores transforman la información entre los formatos externos y los utilizados por la aplicación.

---

### 2.4 Frameworks e Infraestructura

Contiene las tecnologías y servicios concretos utilizados para ejecutar la solución.

Incluye:

- PostgreSQL.
- Redis.
- Cola de eventos.
- API REST.
- Pasarela de pago externa.
- Servicio de notificaciones.
- Autenticación y autorización.
- Monitoreo y auditoría.
- Mecanismos de respaldo y recuperación.

Estos componentes se encuentran en la parte externa de la arquitectura y no deben determinar las reglas principales del negocio.

---
## 3. Diagrama de Clean Architecture

El siguiente diagrama representa la aplicación de **Clean Architecture** en la Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial. Las reglas del negocio permanecen en el dominio, mientras que los componentes tecnológicos y servicios externos se mantienen en las capas externas.

```mermaid
%%{init: {"theme":"base","flowchart":{"curve":"linear","nodeSpacing":22,"rankSpacing":50,"padding":14,"htmlLabels":true},"themeVariables":{"fontFamily":"Arial, sans-serif","fontSize":"13px","lineColor":"#444","primaryTextColor":"#111","clusterBkg":"#ffffff","clusterBorder":"#888","edgeLabelBackground":"#ffffff"}}}%%
flowchart LR

USR["<b>USUARIOS</b><br/>Pasajero<br/>Agente / Vendedor<br/>Administrador de Agencia<br/>Superadministrador"]

subgraph SISTEMA["«aplicación» Plataforma Integrada de Reservas y Gestión de Transporte Interprovincial"]

subgraph ADAPT["ADAPTADORES Y FRAMEWORKS — dependen del framework web, la base de datos, la caché y los servicios externos"]

subgraph PRES["PRESENTACIÓN"]
P4["<i>«componente»</i><br/><b>Administración</b><br/>empresas · agencias · servicios"]
P1["<i>«componente»</i><br/><b>Interfaz Web</b><br/>consulta de servicios"]
P2["<i>«componente»</i><br/><b>Autenticación</b><br/>inicio de sesión y roles"]
P5["<i>«componente»</i><br/><b>Reportes</b><br/>consultas y resultados"]
P3["<i>«componente»</i><br/><b>Gestión de Reservas</b><br/>reserva y reprogramación"]
end

subgraph APP["APLICACIÓN — casos de uso"]

subgraph CASOS["Casos de uso"]
A2["<i>«caso de uso»</i><br/><b>Gestionar Empresas<br/>y Agencias</b>"]
A3["<i>«caso de uso»</i><br/><b>Gestionar Servicios</b>"]
A1["<i>«caso de uso»</i><br/><b>Consultar Servicios</b>"]
A4["<i>«caso de uso»</i><br/><b>Gestionar Reservas</b>"]
A6["<i>«caso de uso»</i><br/><b>Enviar Notificación</b>"]
A5["<i>«caso de uso»</i><br/><b>Procesar Pago</b>"]
end

subgraph DOM["DOMINIO — núcleo de la aplicación"]

subgraph ENT["Modelos (entidades y reglas)"]
D1["<i>«entidad»</i><br/><b>Usuario</b><br/>rol · credenciales"]
D2["<i>«entidad»</i><br/><b>Empresa · Agencia</b><br/>datos · estado"]
D3["<i>«entidad»</i><br/><b>Ruta · Servicio</b><br/>horario · tarifa"]
D4["<i>«entidad»</i><br/><b>Unidad · Asiento</b><br/>disponibilidad"]
D5["<i>«entidad»</i><br/><b>Reserva</b><br/>estado · cupo · pasajero"]
D6["<i>«entidad»</i><br/><b>Pago</b><br/>monto · estado"]
end

subgraph CON["Contratos (puertos)"]
C1["<i>«interfaz»</i><br/><b>RepositorioUsuarios</b>"]
C2["<i>«interfaz»</i><br/><b>RepositorioEmpresasServicios</b>"]
C3["<i>«interfaz»</i><br/><b>RepositorioReservas</b>"]
C4["<i>«interfaz»</i><br/><b>ProcesadorPagos</b>"]
C5["<i>«interfaz»</i><br/><b>NotificadorUsuario</b>"]
end
end
end

subgraph INFRA["INFRAESTRUCTURA"]
I1["<i>«adaptador»</i><br/><b>Adaptador PostgreSQL</b><br/>repositorios"]
I3["<i>«adaptador»</i><br/><b>Adaptador de Pagos</b><br/>pasarela externa"]
I4["<i>«adaptador»</i><br/><b>Adaptador Notificaciones</b><br/>email · SMS · push"]
I2["<i>«adaptador»</i><br/><b>Adaptador Redis</b><br/>caché"]
I5["<i>«componente»</i><br/><b>Cola de Eventos</b><br/>procesamiento asíncrono"]
end

COMP["<i>«raíz de composición»</i><br/><b>COMPOSICIÓN / CONFIGURACIÓN</b><br/>conecta los contratos del dominio<br/>con los adaptadores de infraestructura"]
end
end

subgraph EXTERNOS["SISTEMAS EXTERNOS"]
EXT1["<i>«sistema externo»</i><br/><b>Pasarela de Pago</b>"]
EXT2["<i>«sistema externo»</i><br/><b>Servicio de Notificaciones</b><br/>email · SMS · push"]
end

USR -->|navegador| P1

P4 --> A2
P4 --> A3
P1 -->|invoca| A1
P2 --> A1
P5 --> A1
P3 --> A4

A2 -.-> D2
A3 -.-> D3
A1 -.-> D3
A4 -.-> D5
A6 -.-> D5
A5 -.-> D6

D1 ~~~ C1
D2 ~~~ C2
D3 ~~~ C3
D4 ~~~ C4
D5 ~~~ C5

C1 -.- I1
C2 -.- I1
C3 -.- I1
C4 -.- I3
C5 -.- I4

COMP ~~~ I2

I3 -->|"HTTPS / REST"| EXT1
I4 -->|"HTTPS / REST"| EXT2

classDef usuario fill:#ffffff,stroke:#333,stroke-width:1.5px,color:#111;
classDef pres fill:#ffffff,stroke:#4a6fa5,color:#111;
classDef caso fill:#ffffff,stroke:#7ca66b,color:#111;
classDef ent fill:#ffffff,stroke:#d3a443,color:#111;
classDef con fill:#fffaf0,stroke:#c99b3c,color:#111;
classDef infra fill:#ffffff,stroke:#9873ad,color:#111;
classDef comp fill:#ffffff,stroke:#333,stroke-width:1.5px,color:#111;
classDef ext fill:#ececec,stroke:#666,stroke-width:1.5px,color:#111;
class USR usuario;
class P1,P2,P3,P4,P5 pres;
class A1,A2,A3,A4,A5,A6 caso;
class D1,D2,D3,D4,D5,D6 ent;
class C1,C2,C3,C4,C5 con;
class I1,I2,I3,I4,I5 infra;
class COMP comp;
class EXT1,EXT2 ext;

style SISTEMA fill:#ffffff,stroke:#444,stroke-width:2px,stroke-dasharray:8 5,color:#111
style ADAPT fill:#f4f4f4,stroke:#999,color:#111
style PRES fill:#dce9f8,stroke:#6c8db5,color:#111
style APP fill:#e6f3de,stroke:#7ca66b,color:#111
style CASOS fill:#f4faf0,stroke:#a9c99b,color:#111
style DOM fill:#fff3cf,stroke:#d3a443,stroke-width:2px,color:#111
style ENT fill:#fffdf5,stroke:#d8b76b,color:#111
style CON fill:#fffdf5,stroke:#d8b76b,color:#111
style INFRA fill:#efe6f7,stroke:#9873ad,color:#111
style EXTERNOS fill:#ffffff,stroke:#777,color:#111

linkStyle 18,19,20,21,22 stroke:#7d54a0,stroke-width:1.6px;
```

### Leyenda

| Representación | Significado |
|---|---|
| **→ Flecha continua** | Llamada o comunicación en tiempo de ejecución. |
| **⇢ Flecha punteada** | Dependencia de código: los casos de uso dependen de las entidades del dominio. |
| **Línea punteada morada** | El adaptador de infraestructura implementa el contrato (puerto) definido en el dominio. |
| **Presentación** | Interacción con los usuarios. |
| **Aplicación** | Casos de uso del sistema. |
| **Dominio** | Entidades, reglas de negocio y contratos. |
| **Infraestructura** | Implementaciones tecnológicas y adaptadores. |
| **Sistemas externos** | Servicios que se encuentran fuera de la plataforma. |

### Regla de dependencia

1. **El dominio no depende de infraestructura.**
2. **Los casos de uso dependen del dominio y sus contratos.**
3. **La infraestructura implementa los contratos definidos hacia el núcleo.**
4. **PostgreSQL, Redis, la pasarela de pago y los servicios de notificaciones permanecen fuera del dominio.**
5. **Los detalles tecnológicos pueden cambiar sin modificar las reglas principales del negocio.**
## 4. Aplicación al proyecto

Clean Architecture permite que las reglas principales de la plataforma permanezcan independientes de la infraestructura utilizada.

Por ejemplo, el proceso de reserva pertenece al núcleo funcional del sistema. La lógica que determina si un asiento se encuentra disponible y si una reserva puede ser confirmada no debe depender directamente de PostgreSQL, Redis o de la interfaz web.

De forma similar, el procesamiento de pagos se comunica con una abstracción definida por la aplicación. La implementación concreta de la pasarela de pago se mantiene en los adaptadores e infraestructura.

Esto permite sustituir o modificar tecnologías externas con menor impacto sobre las reglas del negocio.

---

## 5. Relación con el driver de mantenibilidad

El enfoque responde principalmente al:

**DA10 – Mantenibilidad:** el sistema debe permitir modificar o ampliar funcionalidades sin afectar innecesariamente otros módulos.

Clean Architecture contribuye a este driver mediante:

- separación de responsabilidades;
- control de dependencias;
- independencia del dominio respecto a tecnologías externas;
- facilidad para realizar pruebas;
- posibilidad de sustituir componentes de infraestructura;
- menor impacto ante cambios tecnológicos.

---

## 6. Relación entre estilo y enfoque

La arquitectura del proyecto queda definida de la siguiente manera:

| Elemento | Selección |
|---|---|
| **Estilo arquitectónico** | Arquitectura en capas |
| **Organización de la aplicación** | Monolito modular |
| **Enfoque arquitectónico** | Clean Architecture |
| **Comunicación** | API REST |
| **Persistencia principal** | PostgreSQL |
| **Caché** | Redis |
| **Procesamiento asíncrono** | Cola de eventos |

Por lo tanto, **Clean Architecture no reemplaza la arquitectura en capas ni el monolito modular**, sino que complementa la solución definiendo cómo deben organizarse las responsabilidades y dependencias internas.

---

## 7. Beneficios para la solución

La aplicación de Clean Architecture permite:

- mantener independientes las reglas del negocio;
- reducir el acoplamiento con tecnologías externas;
- facilitar el mantenimiento y evolución del sistema;
- mejorar la capacidad de prueba;
- permitir cambios de infraestructura con menor impacto;
- mantener una estructura organizada conforme crezca la plataforma.