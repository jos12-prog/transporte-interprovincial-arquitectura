# trip-search Specification

## Purpose

Permitir que pasajeros encuentren y comparen viajes interprovinciales públicos por ruta y fecha, con resultados de disponibilidad agregada y tiempos de respuesta verificables.

## Requirements

### Requirement: Búsqueda pública de viajes
La API SHALL exponer `GET /api/v1/trips/search` sin autenticación, con origen, destino y fecha de servicio obligatorios, y filtros opcionales por empresa, agencia, horario y tarifa.

#### Scenario: Buscar viajes para una fecha válida
- **WHEN** el cliente envía origen, destino y fecha de servicio válidos, con filtros opcionales
- **THEN** la API devuelve HTTP 200 con los viajes que satisfacen los criterios, sin exigir identidad ni sesión.

#### Scenario: Parámetros ausentes o inválidos
- **WHEN** falta un parámetro obligatorio o el origen, destino, fecha o filtro tiene un formato no válido
- **THEN** la API devuelve HTTP 400 con un ProblemDetails que identifica el error de validación sin incluir trazas internas.

#### Scenario: Consultar datos de diferentes empresas
- **WHEN** la búsqueda coincide con servicios públicos de más de una empresa o agencia
- **THEN** la API devuelve únicamente los datos públicos de cada servicio coincidente, sin aplicar el ámbito de un usuario autenticado ni exponer información administrativa o personal.

### Requirement: Resultados comparables y disponibilidad agregada
Cada resultado SHALL identificar la empresa, agencia, hora de salida y llegada, nombre del modelo del vehículo, precio y cantidad agregada de asientos libres; una búsqueda sin coincidencias SHALL devolver una lista vacía.

#### Scenario: Mostrar resultados coincidentes
- **WHEN** existen viajes programados para la ruta y fecha solicitadas
- **THEN** cada resultado incluye empresa, agencia, horarios, modelo, tarifa y conteo de asientos libres obtenido mediante el contrato público de disponibilidad.

#### Scenario: No hay viajes disponibles
- **WHEN** no existe viaje coincidente o ninguno tiene asientos libres
- **THEN** la API devuelve HTTP 200 con una lista vacía y la interfaz muestra el estado sin resultados.

#### Scenario: El conteo de asientos está desactualizado
- **WHEN** el resultado proviene de una búsqueda cacheada y el estado de asientos cambió después de calcularse
- **THEN** el conteo se identifica como informativo y no autoriza ni confirma una reserva; la disponibilidad transaccional se verifica por el flujo de reserva fuera de este cambio.

### Requirement: Caché limitada a búsquedas
Las respuestas de búsqueda SHALL poder servirse desde Redis con una vigencia máxima de 60 segundos e informar `X-Cache: HIT` o `X-Cache: MISS`; el mapa de asientos y las operaciones de reserva SHALL NOT depender de esa caché.

#### Scenario: Primera búsqueda y repetición
- **WHEN** una consulta válida se realiza por primera vez y luego se repite dentro de su vigencia
- **THEN** la primera respuesta informa `X-Cache: MISS` y la repetición informa `X-Cache: HIT`.

#### Scenario: Expira la entrada de búsqueda
- **WHEN** una consulta se repite después de 60 segundos desde que se almacenó
- **THEN** la API obtiene la respuesta actual desde su fuente de datos, informa `X-Cache: MISS` y renueva la entrada de búsqueda.

#### Scenario: Consultar el mapa de asientos
- **WHEN** un flujo solicita el mapa o una operación transaccional de disponibilidad
- **THEN** obtiene los estados vigentes de la fuente transaccional y no los toma de la caché de resultados.

### Requirement: Rendimiento y efectividad de caché de búsqueda
La búsqueda SHALL cumplir el objetivo EQ-01 de percentil 95 no mayor de 2 segundos y menos de 1 % de errores técnicos durante una prueba con al menos 200 usuarios concurrentes; las consultas repetitivas cacheables SHALL alcanzar al menos 80 % de respuestas atendidas desde caché conforme EQ-08.

#### Scenario: Medir la búsqueda bajo carga
- **WHEN** una prueba progresiva mide búsquedas con 200 o más usuarios concurrentes
- **THEN** el percentil 95 no supera 2 segundos y los errores técnicos permanecen por debajo de 1 % en el tramo sujeto a EQ-01.

#### Scenario: Medir consultas repetitivas
- **WHEN** la carga repite consultas configuradas para caché
- **THEN** al menos 80 % de esas solicitudes se atiende mediante Redis durante la prueba definida en EQ-08.
