# agency-catalog-administration Specification

## Purpose

Permite al Administrador de Agencia mantener rutas, tarifas, vehículos y viajes dentro de su empresa y agencias autorizadas, respetando el inventario de asientos y la continuidad de los viajes ya creados.

## Requirements

### Requirement: Ámbito de administración de agencia
La API SHALL permitir estas operaciones únicamente a `agency_admin` autenticados y SHALL validar la empresa del token y la agencia activa seleccionada mediante `X-Agency-Id` y `AgencyScope`.

#### Scenario: Empresa y agencia autorizadas
- **WHEN** el administrador opera con una agencia activa asignada en su token y un registro de la empresa de su token
- **THEN** la operación puede continuar y los datos compartidos de ruta/tarifa se guardan bajo `company_id` del token, mientras que vehículos y viajes quedan asociados a la agencia seleccionada.

#### Scenario: Agencia fuera del ámbito
- **WHEN** el administrador usa una agencia no asignada, inactiva o de otra empresa
- **THEN** la API responde HTTP 403 con `ProblemDetails` y no consulta ni modifica datos fuera del ámbito.

#### Scenario: Sin autenticación o rol incorrecto
- **WHEN** una solicitud carece de token válido o pertenece a un rol distinto de `agency_admin`
- **THEN** la API responde HTTP 401 o HTTP 403, respectivamente, con `ProblemDetails`.

### Requirement: Administración de rutas y vehículos
La API SHALL permitir crear, consultar, editar y desactivar rutas por empresa y vehículos por agencia; no SHALL borrar físicamente estos registros.

#### Scenario: Crear o editar una ruta
- **WHEN** el administrador envía datos válidos para una ruta de su empresa
- **THEN** la API persiste los datos bajo el `company_id` del token y responde con el recurso actualizado.

#### Scenario: Desactivar ruta o vehículo
- **WHEN** el administrador desactiva una ruta o vehículo existente
- **THEN** el registro se conserva, deja de ofrecerse para viajes nuevos y los viajes existentes continúan con sus datos.

#### Scenario: Registrar vehículo
- **WHEN** el administrador registra placa, nombre visible y un modelo activo del catálogo global
- **THEN** el vehículo queda asociado a la empresa y agencia autorizadas; placa duplicada responde HTTP 409 con `ProblemDetails`.

#### Scenario: Usar el nombre visible del vehículo
- **WHEN** un pasajero consulta resultados de viaje o el mapa de asientos de un viaje con vehículo registrado
- **THEN** la API incluye el nombre visible del vehículo junto con el modelo para que ambas pantallas lo presenten.

#### Scenario: Modelo inactivo o cambio de modelo en uso
- **WHEN** se registra un vehículo con un modelo inactivo, o se intenta cambiar el modelo de un vehículo que ya tiene viajes
- **THEN** la API responde HTTP 409 con `ProblemDetails` y conserva el vehículo sin cambios.

### Requirement: Tarifas con vigencia y precio de viaje
La API SHALL administrar tarifas base y especiales por fecha para rutas de la empresa, impedir periodos solapados del mismo tipo y copiar al viaje el precio vigente al programarlo.

#### Scenario: Tarifa especial vigente
- **WHEN** se crea un viaje cuya fecha local de servicio tiene una tarifa especial activa y vigente para su ruta
- **THEN** se copia el precio especial a `Trip` y ese precio se mantiene aunque cambien tarifas después.

#### Scenario: Aplicar tarifa base
- **WHEN** no existe tarifa especial vigente para la fecha pero existe una tarifa base activa y vigente
- **THEN** se copia a `Trip` el precio base vigente y un único precio aplica a todos sus asientos.

#### Scenario: No existe una tarifa vigente
- **WHEN** se programa un viaje para una ruta y fecha sin tarifa especial ni base vigente
- **THEN** la API responde HTTP 409 con `ProblemDetails` cuyo detalle es `La ruta no tiene tarifa vigente para esa fecha` y no crea el viaje.

#### Scenario: Periodo de tarifas solapado
- **WHEN** se registra o edita una tarifa cuyo periodo se cruza con otra tarifa del mismo tipo y ruta
- **THEN** la API responde HTTP 409 con `ProblemDetails` y no cambia ninguna tarifa.

### Requirement: Programación de viajes y asientos
La API SHALL permitir programar viajes para vehículos activos de la agencia y crear sus asientos conforme al plano del modelo, conservando la propiedad de `TripSeat` en MI04.

#### Scenario: Programar viaje válido
- **WHEN** se crea un viaje con ruta activa de la empresa, vehículo activo de la agencia y tarifa vigente
- **THEN** el viaje queda `Scheduled`, guarda la tarifa resuelta y tiene un `TripSeat` por cada asiento del plano del modelo.

#### Scenario: Vehículo ocupado en el horario
- **WHEN** dos viajes programados del mismo vehículo tienen intervalos horarios que se solapan
- **THEN** la segunda operación responde HTTP 409 con `ProblemDetails` y no crea un viaje ni asientos parciales.

#### Scenario: Datos inválidos
- **WHEN** faltan datos obligatorios, la llegada no es posterior a la salida o las referencias pertenecen a otro ámbito
- **THEN** la API responde HTTP 400 para validación o HTTP 403 para ámbito no autorizado, con `ProblemDetails` y sin persistencia parcial.

### Requirement: Cancelación administrativa de viajes
La API SHALL cancelar un viaje programado sin borrarlo físicamente, preservar su registro y rechazar la operación si tiene asientos confirmados.

#### Scenario: Cancelar viaje sin ventas confirmadas
- **WHEN** el administrador cancela un viaje que no tiene asientos confirmados
- **THEN** el viaje pasa a `Cancelled`, se liberan sus holds activos y deja de aparecer en la búsqueda pública.

#### Scenario: Cancelar viaje con asiento confirmado
- **WHEN** el viaje tiene al menos un asiento en estado confirmado/ocupado
- **THEN** la API responde HTTP 409 con `ProblemDetails`, mantiene el viaje `Scheduled` y no libera ni altera sus asientos.

#### Scenario: Cancelación repetida
- **WHEN** se intenta cancelar un viaje ya cancelado
- **THEN** la API responde de forma idempotente sin crear efectos secundarios ni modificar datos de reservas.

### Requirement: Interfaz del Administrador de Agencia
La interfaz SHALL ofrecer al rol `agency_admin` las pantallas de rutas, vehículos, horarios/viajes y tarifas según Stitch, y SHALL mantener accesibles la sesión y la navegación según el diseño aprobado.

#### Scenario: Administrar catálogo y consultar plano
- **WHEN** un administrador abre las pantallas del catálogo
- **THEN** puede realizar las operaciones autorizadas, ver el plano del vehículo mediante `SeatLayoutView` y recibe estados claros de carga, vacío, validación y conflicto.

#### Scenario: Shell de personal y acceso desde login pasajero
- **WHEN** una persona autenticada usa el shell de personal o abre el login de pasajero
- **THEN** el shell coloca nombre, rol y `Cerrar sesión` al pie de la barra lateral y deja título/insignia en la barra superior; el login pasajero ofrece `¿Eres personal de agencia?` hacia `/personal/login`.

### Requirement: Selector de agencia y navegación con módulo activo
La interfaz SHALL mostrar las agencias asignadas en el selector superior del grupo `.top .agency` y SHALL indicar el módulo actual en la navegación lateral mediante `NavLink` y `aria-current="page"`.

#### Scenario: Seleccionar agencia asignada
- **WHEN** un administrador carga una agencia previamente guardada en `sessionStorage` que todavía pertenece a su lista asignada
- **THEN** el selector la muestra como activa, conserva la selección en `sessionStorage` y pasa su identificador y datos públicos a la pantalla hija.

#### Scenario: Primera selección y estados del selector
- **WHEN** no hay una selección guardada válida o la consulta de agencias está cargando, vacía o falla
- **THEN** se selecciona la primera agencia asignada cuando exista; en los demás casos se muestra el estado de carga, vacío o error accesible correspondiente.

#### Scenario: Navegar por un módulo
- **WHEN** el administrador abre una ruta del módulo de vehículos, tarifas, rutas o viajes
- **THEN** el enlace correspondiente mantiene `aria-current="page"` también en sus rutas hijas y el enlace Dashboard no aparece activo en una ruta hija.
