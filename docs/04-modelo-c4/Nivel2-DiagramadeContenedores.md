# Modelo C4 — Nivel 2: Diagrama de Contenedores

## 1. Propósito

El Diagrama de Contenedores (Nivel 2 del Modelo C4) descompone el sistema en sus unidades de software ejecutables y de almacenamiento de datos desplegables. Muestra las responsabilidades de alto nivel de cada contenedor, las decisiones tecnológicas clave adoptadas (según los ADRs del proyecto) y los protocolos de comunicación utilizados para su interacción.

En este nivel se hace explícito el enfoque de **Monolito Modular** para el backend (ADR01), desacoplado de la **Aplicación Web SPA** en el frontend (ADR02) y respaldado por una base de datos relacional robusta (ADR04).

---

## 2. Contenedores de la Solución

| Contenedor | Tipo | Tecnología | Responsabilidad Principal |
|---|---|---|---|
| **Aplicación Web (SPA)** | Frontend / Cliente Web | React 18+, TypeScript, Vite, Tailwind CSS | Proporciona la interfaz gráfica responsive y dinámica para pasajeros, agentes de ventanilla, administradores de agencia y superadministradores. |
| **Backend API (Monolito Modular)** | Servidor de Aplicaciones / API | ASP.NET Core 8+ (C#), REST API | Ejecuta la lógica del negocio, coordina casos de uso, aplica políticas de seguridad multiempresa y gestiona las transacciones de reservas y ventas. |
| **Base de Datos Principal** | Base de Datos Relacional | PostgreSQL 16+ | Fuente única de verdad. Garantiza consistencia transaccional (ACID), aislamiento multiempresa, control de concurrencia y persistencia de auditoría. |
| **Caché Distribuida** | Almacén en Memoria | Redis | Almacena temporalmente resultados de consultas frecuentes (rutas, horarios, tarifas) aliviando la carga sobre PostgreSQL (RC12). |
| **Cola de Mensajería** | Broker de Mensajes | RabbitMQ | Cola durable para el procesamiento asíncrono y desacoplado de notificaciones (WhatsApp) y eventos secundarios (RC13). |

---

## 3. Diagrama de Contenedores

![Diagrama de Contenedores (Nivel 2)](../../img/diagrama-contenedores-c4.svg)

*Figura 1. Diagrama de Contenedores de la Plataforma de Transporte Interprovincial (C4 — Nivel 2).*

---

## 4. Descripción Detallada de Contenedores

### 4.1. Aplicación Web (SPA)
* **Tecnología:** React con TypeScript, empaquetado con Vite. Estilizado con Tailwind CSS y componentes accesibles de shadcn/ui.
* **Ejecución:** Se descarga como recursos estáticos (HTML/JS/CSS) y se ejecuta en el navegador web del cliente (desktop, tablet o móvil).
* **Responsabilidades:**
  * Renderizado del catálogo de rutas, horarios, precios y estado de buses en tiempo real.
  * Selector interactivo de asientos (distribución gráfica de 1 o 2 pisos según bus) y selector numérico de cupos.
  * Pasarela visual para ingreso de datos de pasajeros y selección de método de pago.
  * Módulo de ventanilla optimizado con atajos de teclado para agentes de ventas.
  * Paneles administrativos para configuración de frecuencias, unidades y visualización de reportes.
  * Almacenamiento seguro del token de sesión (JWT en cookie HttpOnly).

### 4.2. Backend API (Monolito Modular)
* **Tecnología:** ASP.NET Core 8+, C#, arquitectura orientada a dominios (Clean Architecture).
* **Ejecución:** Proceso de servidor unificado alojado en contenedores Linux (Docker).
* **Responsabilidades:**
  * Exposición de endpoints RESTful seguros mediante HTTPS.
  * Autenticación y autorización basada en ASP.NET Core Identity y Claims (validando rol, empresa y agencia).
  * Coordinación de los 7 módulos de negocio (MI01 a MI07) en un único proceso en memoria.
  * Gestión de concurrencia y transacciones ACID en la reserva de asientos para evitar sobreventas.
  * Verificación criptográfica e idempotente de webhooks provenientes de la pasarela de pagos Culqi.
  * Publicación de eventos en RabbitMQ para el despacho asíncrono de notificaciones a través de WhatsApp.

### 4.3. Base de Datos Relacional Principal
* **Tecnología:** PostgreSQL 16+.
* **Responsabilidades:**
  * Almacenamiento persistente estructurado con integridad referencial estricta.
  * Soporte de transacciones serializables o con bloqueo pesimista (`SELECT ... FOR UPDATE`) durante la reserva simultánea de asientos.
  * Esquema particionado lógicamente por empresa y agencia para garantizar el aislamiento multiempresa.
  * Índices optimizados (B-Tree, índice único parcial `idx_unique_active_seat`) para búsquedas rápidas y prevención de sobreventa.
  * Tablas inmutables para el registro cronológico de auditoría (logs de seguridad y transacciones).

### 4.4. Caché Distribuida (Redis)
* **Tecnología:** Redis.
* **Responsabilidades:**
  * Almacenamiento en memoria de rutas, horarios, frecuencias y catálogos de consulta recurrente.
  * Alivio sustancial del tráfico de lectura a la base de datos durante campañas de alta demanda (Semana Santa, Fiestas Patrias).

### 4.5. Cola de Mensajería (RabbitMQ)
* **Tecnología:** RabbitMQ.
* **Responsabilidades:**
  * Recepción de eventos de dominio (p. ej. `ReservaConfirmadaEvent`).
  * Desacoplamiento temporal del canal de notificaciones externas (WhatsApp Business Cloud API).
  * Reintentos automáticos y tolerancia a fallos transitorios en servicios externos.

---

## 5. Decisiones Arquitectónicas Relacionadas

* **DA-01 — Monolito Modular:** Consolida los módulos en un único backend desplegable, reduciendo drásticamente la sobrecarga de red y complejidad operativa en la fase inicial, manteniendo límites estrictos de código.
* **DA-02 — Clean Architecture:** Separa el dominio y la aplicación de los detalles técnicos de frameworks e infraestructura.
* **DA-03 — API REST:** Canal desacoplado y estándar sobre HTTPS para clientes web.
* **DA-04 / DA-07 — PostgreSQL:** Provee soporte ACID maduro, indispensable para el requerimiento DA02/DA03 (evitar sobreventa y garantizar consistencia de reservas).
* **DA-09 — Redis:** Caché de lectura obligatoria (RC12) para soportar alta demanda sin degradar la base de datos.
* **DA-10 — RabbitMQ:** Cola durable de eventos para procesar notificaciones asíncronas tolerantes a fallos (RC13).
* **DA-11 — Culqi mediante Adapter:** Integración de pagos desacoplada mediante `IPaymentGateway`.
* **DA-12 — WhatsApp Business Cloud API:** Canal oficial de mensajería para confirmaciones y alertas preventivas.

---

## 6. Seguridad y Canales de Comunicación

1. **Cliente Web → Backend API:**
   * Protocolo: HTTPS / TLS 1.3 con cifrado extremo a extremo.
   * Autenticación: Tokens JWT transportados mediante cookies protegidas con flags `HttpOnly`, `Secure` y `SameSite=Strict`.
   * Protección contra ataques comunes: CORS configurado rígidamente, protección CSRF y rate limiting en endpoints de búsqueda y login.

2. **Backend API → PostgreSQL:**
   * Conexión por TCP mediante el driver oficial Npgsql con cifrado TLS activo.
   * Uso de Connection Pooling para optimizar reutilización de conexiones en picos de demanda.
   * Credenciales administradas mediante variables de entorno seguras (Secrets Manager).

3. **Backend API → Pasarela de Pagos:**
   * Comunicación saliente HTTPS con autenticación por API Keys / Secret Token.
   * Comunicación entrante (Webhooks) validada con firmas HMAC SHA-256 en las cabeceras HTTP y verificación de idempotencia contra la base de datos.
