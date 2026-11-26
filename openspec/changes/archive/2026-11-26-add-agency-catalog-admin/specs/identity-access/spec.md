# Spec Delta

## MODIFIED Requirements

### Requirement: Inicio de sesión y token de acceso
La API SHALL autenticar pasajeros por correo o DNI y personal por correo corporativo; las credenciales válidas SHALL producir un JWT de acceso con vigencia de 60 minutos y SHALL incluir en el objeto `user` la información pública `fullName` y `email`.

#### Scenario: Pasajero inicia sesión
- **WHEN** un pasajero presenta su correo o DNI y contraseña correctos en `POST /api/v1/auth/login`
- **THEN** la API devuelve un token válido por 60 minutos con su identificador y claim `role=passenger`.

#### Scenario: Personal inicia sesión
- **WHEN** un agente, administrador de agencia o superadministrador presenta sus credenciales válidas en `POST /api/v1/auth/login`
- **THEN** la API devuelve un token con identificador y claim `role` igual a `agent`, `agency_admin` o `super_admin`; los roles con ámbito de tenant incluyen `company_id` y el arreglo `agency_ids` que les corresponde.

#### Scenario: Respuesta pública de sesión
- **WHEN** las credenciales de cualquier rol son válidas y el login responde HTTP 200
- **THEN** el objeto `user` incluye `fullName` y `email` además de `id` y `role`, y SHALL omitir contraseña, hash, secretos y datos de otras cuentas.

#### Scenario: Credenciales incorrectas
- **WHEN** el identificador o contraseña no autentica una cuenta
- **THEN** la API responde HTTP 401 con `ProblemDetails` genérico que no revela si el correo o DNI está registrado.
