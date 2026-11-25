# Spec Delta

## Purpose

Permitir que pasajeros y personal se autentiquen con una identidad verificable y que cada sesión solo acceda a las agencias autorizadas para su rol y empresa.

## ADDED Requirements

### Requirement: Registro de pasajeros
La API SHALL permitir registrar pasajeros con nombre, DNI, correo, celular y contraseña; SHALL exigir un DNI de ocho dígitos y contraseña de al menos ocho caracteres, mantener DNI/correo normalizados únicos y nunca guardar la contraseña en texto plano.

#### Scenario: Registro válido
- **WHEN** se envían los datos requeridos con correo y DNI no registrados
- **THEN** se crea una cuenta con rol técnico `passenger`, se almacena el hash de la contraseña y se responde HTTP 201 sin emitir una sesión automáticamente.

#### Scenario: Correo o DNI duplicado
- **WHEN** se intenta registrar un correo o DNI ya asociado a una cuenta
- **THEN** la API responde HTTP 409 con `ProblemDetails` y no crea una segunda cuenta.

#### Scenario: Datos de registro inválidos
- **WHEN** falta un campo requerido, el DNI no tiene ocho dígitos, el correo no es válido o la contraseña tiene menos de ocho caracteres
- **THEN** la API responde HTTP 400 con `ProblemDetails` sin crear la cuenta.

### Requirement: Inicio de sesión y token de acceso
La API SHALL autenticar pasajeros por correo o DNI y personal por correo corporativo; las credenciales válidas SHALL producir un JWT de acceso con vigencia de 60 minutos.

#### Scenario: Pasajero inicia sesión
- **WHEN** un pasajero presenta su correo o DNI y contraseña correctos en `POST /api/v1/auth/login`
- **THEN** la API devuelve un token válido por 60 minutos con su identificador y claim `role=passenger`.

#### Scenario: Personal inicia sesión
- **WHEN** un agente, administrador de agencia o superadministrador presenta sus credenciales válidas en `POST /api/v1/auth/login`
- **THEN** la API devuelve un token con identificador y claim `role` igual a `agent`, `agency_admin` o `super_admin`; los roles con ámbito de tenant incluyen `company_id` y el arreglo `agency_ids` que les corresponde.

#### Scenario: Credenciales incorrectas
- **WHEN** el identificador o contraseña no autentica una cuenta
- **THEN** la API responde HTTP 401 con `ProblemDetails` genérico que no revela si el correo o DNI está registrado.

### Requirement: Perfil autenticado
La API SHALL exponer `GET /api/v1/auth/me` protegido por Bearer para devolver únicamente el perfil de la sesión autenticada.

#### Scenario: Consultar perfil con token válido
- **WHEN** el cliente presenta un JWT vigente
- **THEN** la API responde HTTP 200 con el identificador, datos de perfil, rol y ámbito organizacional de esa cuenta, sin hash de contraseña ni datos de otras cuentas.

#### Scenario: Consultar perfil sin autenticación
- **WHEN** el cliente no presenta un JWT válido
- **THEN** la API responde HTTP 401.

### Requirement: Acceso a agencias asignadas
La API SHALL aplicar autorización por los códigos `agent` y `agency_admin` y ámbito mediante `AgencyScope`; sus endpoints de agencias SHALL exponer únicamente agencias incluidas en el `agency_ids` del token. Los nombres legibles de los roles SHALL usarse solo como etiquetas.

#### Scenario: Consultar agencias propias (RF29)
- **WHEN** un agente o administrador autenticado consulta `GET /api/v1/staff/agencies`
- **THEN** recibe HTTP 200 con las agencias asignadas a su sesión y ninguna agencia ajena a su token.

#### Scenario: Consultar una agencia autorizada
- **WHEN** un agente o administrador autenticado consulta `GET /api/v1/staff/agencies/{agencyId}` y el identificador está autorizado en su token y corresponde a su empresa
- **THEN** recibe HTTP 200 con los datos de esa agencia.

#### Scenario: Solicitar agencia fuera del ámbito
- **WHEN** un usuario autenticado solicita `GET /api/v1/staff/agencies/{agencyId}` y el identificador no pertenece a su `agency_ids` o a su `company_id`
- **THEN** la API responde HTTP 403 con `ProblemDetails` y no revela los datos de la agencia.

#### Scenario: Usuario sin sesión solicita agencias
- **WHEN** un cliente sin JWT válido solicita cualquiera de los endpoints `/api/v1/staff/agencies`
- **THEN** la API responde HTTP 401.

#### Scenario: Rol no autorizado solicita agencias de personal
- **WHEN** un pasajero autenticado solicita cualquiera de los endpoints `/api/v1/staff/agencies`
- **THEN** la API responde HTTP 403 con `ProblemDetails`.

### Requirement: Límites de intentos de login
La API SHALL limitar login a veinte solicitudes por minuto por IP y SHALL bloquear temporalmente un identificador después de cinco fallos de autenticación en cinco minutos; cualquier límite excedido SHALL responder HTTP 429 con `ProblemDetails`.

#### Scenario: Superar el límite por IP
- **WHEN** una IP realiza más de veinte solicitudes de login en un minuto
- **THEN** las solicitudes excedentes reciben HTTP 429 con `ProblemDetails`.

#### Scenario: Superar el límite por identificador
- **WHEN** se acumulan cinco fallos de autenticación para el mismo correo o DNI dentro de cinco minutos
- **THEN** los intentos adicionales para ese identificador reciben HTTP 429 con `ProblemDetails` durante el bloqueo temporal.

### Requirement: Cuentas sembradas en Development
El entorno Development SHALL crear de forma idempotente un superadministrador, un administrador de agencia y un agente asociados a los datos semilla existentes; sus contraseñas SHALL provenir de configuración y almacenarse hasheadas.

#### Scenario: Inicializar cuentas de Development
- **WHEN** la API inicia en Development con los valores de configuración requeridos
- **THEN** existe una cuenta de cada rol solicitado ligada a las empresas/agencias sembradas y repetir el inicio no duplica las cuentas.

#### Scenario: Configuración secreta ausente
- **WHEN** falta una contraseña requerida para una cuenta semilla
- **THEN** la aplicación no crea una cuenta con contraseña por defecto ni registra una contraseña en logs.

### Requirement: Experiencia de autenticación y sesión por rol
La aplicación SHALL ofrecer registro y login diferenciados para pasajero y personal, guardar la sesión únicamente en memoria y proteger las rutas privadas conforme al rol autenticado.

#### Scenario: Autenticar y redirigir por rol
- **WHEN** el login responde correctamente para un usuario
- **THEN** la interfaz mantiene su sesión en memoria, muestra etiquetas legibles en español y redirige según los códigos `passenger`, `agent`, `agency_admin` y `super_admin` a búsqueda, `/agente`, `/admin-agencia` y `/superadmin`, respectivamente.

#### Scenario: Proteger rutas de personal
- **WHEN** una persona sin sesión o con un rol distinto abre `/agente`, `/admin-agencia` o `/superadmin`
- **THEN** la ruta no muestra el contenido protegido y redirige a login o al inicio permitido para su rol.

#### Scenario: Cerrar sesión
- **WHEN** el pasajero selecciona cerrar sesión en el menú del avatar o el personal selecciona `Cerrar sesión` en su shell
- **THEN** la sesión en memoria se elimina y la interfaz navega al login correspondiente.

#### Scenario: Reducir movimiento
- **WHEN** el dispositivo tiene `prefers-reduced-motion: reduce`
- **THEN** las pantallas de acceso conservan cambios de estado comprensibles con movimiento reducido, sin parallax, zoom, desplazamientos ni sacudidas; Google no se muestra.
