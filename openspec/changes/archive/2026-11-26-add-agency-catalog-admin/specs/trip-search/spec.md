# Spec Delta

## MODIFIED Requirements

### Requirement: Búsqueda pública de viajes
La API SHALL exponer `GET /api/v1/trips/search` sin autenticación, con origen, destino y fecha de servicio obligatorios, y filtros opcionales por empresa, agencia, horario y tarifa; SHALL excluir viajes asociados a una empresa o agencia inactiva y viajes cuyo estado sea `Cancelled`.

#### Scenario: Buscar viajes para una fecha válida
- **WHEN** el cliente envía origen, destino y fecha de servicio válidos, con filtros opcionales
- **THEN** la API devuelve HTTP 200 con los viajes que satisfacen los criterios y cuyas empresas y agencias están activas, sin exigir identidad ni sesión.

#### Scenario: Consultar datos de diferentes empresas
- **WHEN** la búsqueda coincide con servicios públicos de más de una empresa o agencia
- **THEN** la API devuelve únicamente los datos públicos de cada servicio coincidente con empresa y agencia activas, sin aplicar el ámbito de un usuario autenticado ni exponer información administrativa o personal.

#### Scenario: Empresa o agencia inactiva
- **WHEN** un viaje coincidente pertenece a una empresa o agencia desactivada
- **THEN** el viaje no aparece en los resultados de búsqueda pública.

#### Scenario: Viaje cancelado
- **WHEN** un viaje coincidente tiene estado `Cancelled`
- **THEN** el viaje no aparece en los resultados de búsqueda pública, aunque la ruta, vehículo, empresa y agencia continúen activos.

#### Scenario: Parámetros ausentes o inválidos
- **WHEN** falta un parámetro obligatorio o el origen, destino, fecha o filtro tiene un formato no válido
- **THEN** la API devuelve HTTP 400 con un ProblemDetails que identifica el error de validación sin incluir trazas internas.

### Requirement: Resultados comparables y disponibilidad agregada
Cada resultado SHALL identificar la empresa, agencia, hora de salida y llegada, nombre del modelo y nombre visible del vehículo, precio y cantidad agregada de asientos libres; una búsqueda sin coincidencias SHALL devolver una lista vacía.

#### Scenario: Mostrar resultados coincidentes
- **WHEN** existen viajes programados para la ruta y fecha solicitadas
- **THEN** cada resultado incluye empresa, agencia, horarios, modelo, nombre visible del vehículo, tarifa y conteo de asientos libres obtenido mediante el contrato público de disponibilidad.

#### Scenario: No hay viajes disponibles
- **WHEN** no existe viaje coincidente o ninguno tiene asientos libres
- **THEN** la API devuelve HTTP 200 con una lista vacía y la interfaz muestra el estado sin resultados.

#### Scenario: El conteo de asientos está desactualizado
- **WHEN** el resultado proviene de una búsqueda cacheada y el estado de asientos cambió después de calcularse
- **THEN** el conteo se identifica como informativo y no autoriza ni confirma una reserva; la disponibilidad transaccional se verifica por el flujo de reserva fuera de este cambio.

## ADDED Requirements

### Requirement: Invalidación de búsqueda tras cambios administrativos
Las mutaciones de rutas, tarifas, vehículos y viajes que puedan cambiar los resultados SHALL invalidar la caché de búsqueda después de confirmar su transacción.

#### Scenario: Consultar después de una mutación confirmada
- **WHEN** una mutación administrativa que afecta la búsqueda se confirma y se ejecuta una nueva consulta
- **THEN** la API obtiene resultados actualizados, sin depender de una entrada anterior a la mutación.

#### Scenario: Mutación no confirmada
- **WHEN** una mutación falla y su transacción se revierte
- **THEN** la operación no invalida la caché como si el cambio se hubiera confirmado.
