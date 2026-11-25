# platform-administration Specification

## Purpose

Permite al superadministrador mantener el catálogo común de empresas, agencias y modelos de vehículos de la plataforma, con autorización central y protección de los datos y planos ya utilizados.

## Requirements

### Requirement: Administración de empresas
La API SHALL permitir al rol técnico `super_admin` registrar, consultar y actualizar empresas de transporte, incluyendo su estado activo.

#### Scenario: Crear una empresa
- **WHEN** un superadministrador envía los datos válidos de una empresa
- **THEN** la API crea la empresa y responde HTTP 201 con su identificador y estado.

#### Scenario: Validación de empresa
- **WHEN** los datos requeridos de una empresa son inválidos o incumplen una restricción de unicidad definida
- **THEN** la API responde HTTP 400 para datos inválidos o HTTP 409 para conflicto, mediante `ProblemDetails`, y no persiste cambios parciales.

#### Scenario: Acceso sin autorización de superadministrador
- **WHEN** una solicitud sin autenticación o con cualquier rol distinto de `super_admin` invoca la administración de empresas
- **THEN** la API responde HTTP 401 o HTTP 403, respectivamente, mediante `ProblemDetails`, sin revelar ni modificar información administrativa.

### Requirement: Administración de agencias por empresa
La API SHALL permitir al rol técnico `super_admin` registrar, consultar y actualizar agencias pertenecientes a una empresa, incluyendo su estado activo.

#### Scenario: Crear agencia asociada
- **WHEN** un superadministrador registra una agencia y referencia una empresa existente
- **THEN** la API crea la agencia asociada únicamente a esa empresa y responde HTTP 201.

#### Scenario: Empresa de referencia inexistente
- **WHEN** la solicitud de agencia referencia una empresa que no existe
- **THEN** la API responde HTTP 404 con `ProblemDetails` y no crea la agencia.

#### Scenario: Datos de agencia inválidos
- **WHEN** faltan datos obligatorios o la empresa asociada está desactivada
- **THEN** la API responde HTTP 400 con `ProblemDetails` y no persiste la agencia.

### Requirement: Activación y desactivación de empresas y agencias
La API SHALL permitir al rol técnico `super_admin` activar o desactivar empresas y agencias sin eliminarlas físicamente.

#### Scenario: Desactivar una empresa o agencia
- **WHEN** el superadministrador cambia el estado de una empresa o agencia activa a inactiva
- **THEN** el registro se conserva y los viajes asociados dejan de aparecer en la búsqueda pública.

#### Scenario: Reactivar un registro
- **WHEN** el superadministrador activa una empresa o agencia previamente desactivada
- **THEN** su estado queda activo y los viajes elegibles pueden volver a aparecer en la búsqueda pública.

#### Scenario: Operación repetida sobre estado ya aplicado
- **WHEN** se solicita activar un registro activo o desactivar uno ya inactivo
- **THEN** la operación responde de forma idempotente con el estado final y no duplica efectos secundarios.

### Requirement: Catálogo global de modelos de vehículo
La API SHALL permitir al rol técnico `super_admin` crear, consultar y actualizar los modelos globales de vehículo, incluyendo nombre visible, capacidad y plano numerado de asientos.

#### Scenario: Crear modelo con plano válido
- **WHEN** un superadministrador registra un modelo cuyo plano contiene asientos numerados con posiciones válidas y únicas
- **THEN** el modelo queda disponible en el catálogo global con capacidad coherente con el número de asientos.

#### Scenario: Plano inválido o capacidad incoherente
- **WHEN** un plano contiene números o posiciones duplicados, o su capacidad no coincide con los asientos definidos
- **THEN** la API responde HTTP 400 con `ProblemDetails` y no crea ni modifica el modelo.

#### Scenario: Modelo usado conserva su plano
- **WHEN** un superadministrador intenta cambiar el plano de un modelo asociado a uno o más vehículos
- **THEN** la API responde HTTP 409 con `ProblemDetails` y conserva el plano existente.

#### Scenario: Modelo sin uso admite edición del plano
- **WHEN** un superadministrador cambia el plano de un modelo que todavía no está asociado a vehículos y el nuevo plano es válido
- **THEN** la API guarda el nuevo plano y su capacidad coherente.

### Requirement: Disponibilidad de modelos para nuevos vehículos
Un modelo de vehículo desactivado SHALL permanecer consultable como referencia histórica y SHALL NOT poder asignarse a vehículos nuevos.

#### Scenario: Intentar usar un modelo desactivado
- **WHEN** una operación de alta de vehículo intenta asignar un modelo desactivado
- **THEN** el backend rechaza la operación con HTTP 409 y no crea el vehículo.

#### Scenario: Consultar catálogo de modelos activos
- **WHEN** se consulta el catálogo para seleccionar un modelo para un vehículo nuevo
- **THEN** la respuesta ofrece solo modelos activos y conserva la consulta administrativa de modelos inactivos para el superadministrador.
