# Spec Delta

## MODIFIED Requirements

### Requirement: Búsqueda pública de viajes
La API SHALL exponer `GET /api/v1/trips/search` sin autenticación, con origen, destino y fecha de servicio obligatorios, y filtros opcionales por empresa, agencia, horario y tarifa; SHALL excluir viajes asociados a una empresa o agencia inactiva.

#### Scenario: Buscar viajes para una fecha válida
- **WHEN** el cliente envía origen, destino y fecha de servicio válidos, con filtros opcionales
- **THEN** la API devuelve HTTP 200 con los viajes que satisfacen los criterios y cuyas empresas y agencias están activas, sin exigir identidad ni sesión.

#### Scenario: Parámetros ausentes o inválidos
- **WHEN** falta un parámetro obligatorio o el origen, destino, fecha o filtro tiene un formato no válido
- **THEN** la API devuelve HTTP 400 con un ProblemDetails que identifica el error de validación sin incluir trazas internas.

#### Scenario: Consultar datos de diferentes empresas
- **WHEN** la búsqueda coincide con servicios públicos de más de una empresa o agencia
- **THEN** la API devuelve únicamente los datos públicos de cada servicio coincidente con empresa y agencia activas, sin aplicar el ámbito de un usuario autenticado ni exponer información administrativa o personal.

#### Scenario: Empresa o agencia inactiva
- **WHEN** un viaje coincidente pertenece a una empresa o agencia desactivada
- **THEN** el viaje no aparece en los resultados de búsqueda pública.
