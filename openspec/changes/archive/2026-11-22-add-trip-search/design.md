# Design

## Context

Ver `proposal.md` para la motivación y `specs/trip-search/spec.md` para el contrato observable. El repositorio ya contiene los proyectos .NET 8 de MI02 `CompaniesAgencies`, MI03 `TransportScheduling`, MI04 `Reservations` y MI06 `QueriesReports`, con `DbContext` vacíos y esquemas `companies_agencies`, `transport_scheduling` y `reservations`; aún no hay entidades de dominio ni pantallas de búsqueda. Los límites de módulo siguen el diseño de módulos y C4: MI06 es dueño de la consulta pública, MI03 de rutas/vehículos/viajes y MI04 de la disponibilidad y `TripSeat`.

## Goals / Non-Goals

**Goals:**

- Crear los modelos mínimos y migraciones necesarias para búsquedas reales y semillas reproducibles en los esquemas propietarios.
- Mantener Clean Architecture y el acceso entre módulos por sus contratos públicos, sin lecturas directas de tablas ajenas.
- Entregar búsqueda pública versionada, validada, documentada en Swagger y consumida desde las dos pantallas del pasajero.
- Medir el objetivo EQ-01 desde 200 VUs y observar una rampa hasta 2.000 VUs; medir también la efectividad EQ-08.

**Non-Goals:**

- Implementar autenticación, autorización de usuario, mapa de asientos, retenciones, pagos o escrituras de reservas.
- Implementar CRUD administrativo de empresas, agencias, rutas, vehículos o viajes.
- Crear una proyección de lectura eventual o una réplica de lectura para la búsqueda.

## Decisions

### Propiedad de entidades y contratos entre módulos

- **MI02 / `companies_agencies`:** `Company` y `Agency`; la agencia guarda `CompanyId`. El módulo expone datos públicos mínimos mediante su contrato de módulo.
- **MI03 / `transport_scheduling`:** `Route`, `VehicleModel`, `SeatLayout`, `Vehicle` y `Trip`. `Trip` conserva `CompanyId` y `AgencyId` como identificadores, sin EF navigation ni FK hacia el esquema de MI02. Persistirá salidas/llegadas UTC, tarifa y referencias a ruta, vehículo y empresa/agencia.
- **MI04 / `reservations`:** propietario de `TripSeat` y sus estados. `ISeatAvailabilityService` permanece como contrato canónico interno de MI04; un contrato público `IReservationsApi` expone una consulta agregada/batch de cantidades libres sin acoplar consumidores a entidades o repositorios privados de MI04.
- **MI03:** consume `IReservationsApi` para agregar el conteo de disponibilidad al resultado de viajes de su propio contrato público `ITransportSchedulingApi`. No lee tablas de MI04 ni modifica `TripSeat`.
- **MI06:** el caso de uso coordina consultas por `ITransportSchedulingApi` y el contrato público de datos de MI02. Así recibe itinerario, modelo y disponibilidad agregada desde MI03 y evita acoplarse directamente a los detalles de MI04. Las consultas son por lote para evitar un round-trip por viaje.
- Los contratos públicos `ICompaniesAgenciesApi`, `ITransportSchedulingApi` e `IReservationsApi` y sus DTO neutrales viven en `Shared.Contracts`, sin dependencia de módulos ni frameworks. Así las aplicaciones se comunican por interfaces sin referencias de proyecto entre módulos (regla comprobada por `tests/Architecture`). Cada módulo conserva Domain sin referencias externas; Application depende solo de su Domain y contratos; Infrastructure implementa persistencia; Presentation valida/transporta la petición y delega al caso de uso.

La alternativa de colocar `TripSeat` dentro de MI03 se descarta porque `diseño-interno-de-modulos.md` (§§ 2 y 5) y C4 Nivel 3 asignan su disponibilidad a MI04. La selección evita que MI03 y MI04 escriban el mismo inventario; MI03 obtiene su conteo exclusivamente por el contrato público de MI04.

### Datos, planos e inicialización

Las migraciones pertenecen al `DbContext` de cada esquema y se ejecutan en orden de propietario: MI02, MI03 y MI04. En el entorno `Development`, el Host aplica esas migraciones al arrancar y, después, ejecuta el sembrado idempotente; así `docker compose up` deja los datos listos cuando configura ese entorno. En otros entornos no se ejecutan migraciones automáticamente. No se crean foreign keys entre esquemas. Los vínculos entre semillas usan IDs estables y coherentes; los contratos verifican referencias cuando las consultas cruzan límites. MI04 crea los `TripSeat` iniciales a partir de IDs de viajes y de los planos incluidos en la semilla, conservando copia del número y ubicación del asiento para su futuro ciclo de vida.

Los modelos iniciales son Toyota Yaris (4), Toyota Avanza (6), Minivan (7), Van (15), Bus 1 piso (44) y Bus 2 pisos (52). `SeatLayout` registra número de asiento, fila, columna y piso cuando corresponda. Para Yaris, Avanza, Minivan y Van se usa la distribución de `design/stitch/DESIGN.md`; para los buses se guardan 44/52 asientos numerados, distribuidos en uno/dos pisos, sin diseñar ni exponer todavía un mapa visual.

La semilla crea varias empresas y agencias en Ayacucho, Lima, Huanta, Andahuaylas y Cusco, y viajes con salidas dentro de los siguientes siete días. Las fechas se almacenan UTC y se interpretan/representan en `America/Lima`. Los datos son demostrativos, deterministas e idempotentes; no representan operación productiva.

### Consulta pública y criterios de resultados

La Presentation de MI06 expone `GET /api/v1/trips/search`. `origin`, `destination` y `date` son requeridos; `date` usa `yyyy-MM-dd` como fecha de servicio local. Los filtros opcionales son `companyId`, `agencyId`, `departureFrom`, `departureTo`, `minPrice` y `maxPrice`, que materializan RF02. Los campos textuales se normalizan para búsqueda sin distinción de mayúsculas y espacios externos; los horarios se reciben/entregan con zona explícita y el front-end presenta hora local de Lima.

La respuesta presenta la forma `TripSearchResult[]`: identificadores públicos y nombres de empresa/agencia, origen/destino, salida/llegada, `vehicleModelName`, precio en PEN y `availableSeats`. No se exponen datos personales ni el plano. Los criterios sin coincidencias devuelven 200 y lista vacía. Una petición mal formada produce ProblemDetails 400; los fallos inesperados producen ProblemDetails sanitizado 500. La operación y sus esquemas/códigos se documentan en OpenAPI. El acceso anónimo se limita a esta lectura de datos públicos.

### Redis y evidencia de cache

MI06 usa cache-aside: clave versionada a partir de ruta normalizada, fecha, filtros y versión de formato; TTL 60 segundos. El caso de uso añade `X-Cache: MISS` en lectura/origen y `HIT` en respuesta cacheada. Se cachea el resultado agregado completo, incluido `availableSeats`, con frescura máxima de 60 segundos; no se cachea un mapa ni se lee la caché para autorizar una reserva. Si Redis no está disponible, la consulta puede ir a PostgreSQL por los contratos propietarios y devuelve `MISS`, ya que Redis no es fuente de verdad.

La indisponibilidad de Redis no debe retener la búsqueda hasta el timeout de conexión. La configuración usa `connectTimeout`, `syncTimeout` y `asyncTimeout` de 500 ms y un presupuesto de 300 ms para cada operación de caché. `RedisTripSearchCache`, registrado como singleton para compartir el estado entre solicitudes del proceso, abre su circuit breaker durante 30 s después de un fallo o timeout; mientras está abierto omite Redis y permite que el caso de uso consulte el origen mediante los contratos de módulo hasta PostgreSQL. Esta es la aplicación a la caché de la táctica 5 (Retry y Circuit Breaker) de `docs/02-arquitectura-software/tacticas-arquitectonicas.md`, adaptada para evitar esperas repetidas a una dependencia de optimización. `tests/Unit/RedisTripSearchCacheTests.cs` / `FailedGetOpensBreakerAndImmediateGetAndSetSkipDistributedCache` verifica que el primer `GetAsync` fallido abre el circuito y que un segundo `GetAsync` y un `SetAsync` inmediatos no vuelven a llamar al `IDistributedCache`. `tests/Integration/TripSearchRedisCacheTests.cs` / `SearchFallsBackToSourceWhenRedisIsUnavailable` cubre además el fallback de la búsqueda. No hay una prueba que verifique el cierre del breaker después de los 30 s.

Los índices B-Tree elegidos para la consulta son ruta por origen/destino, viaje por ruta/fecha de salida y asientos por viaje/estado. `docs/evidencias/bd-y-cache.md` documenta la consulta reproducible y el procedimiento para obtener `EXPLAIN ANALYZE` antes y después de los índices, y registra los planes de 6.1 junto con la medición real de latencia con Redis activado/desactivado de 6.2.

### Frontend

Se implementan rutas del pasajero para `design/stitch/pasajero/03-inicio-busqueda/` y `04-resultados/`, usando componentes React reutilizables, tokens y estados de `design/stitch/DESIGN.md`, no copia literal de `code.html`. El selector inicial ofrece las cinco ciudades sembradas, el formulario conserva ruta/fecha/filtros, resultados consultan la API real y muestran skeleton durante espera y estado vacío ante lista vacía. El conteo es informativo y no habilita una reserva en este change.

### Pruebas de carga

`tests/performance/search-load.js` usa etapas ascendentes hasta 2.000 VUs. La medición de aceptación conserva EQ-01 (p95 ≤ 2 s y errores técnicos < 1 % con al menos 200 concurrentes); la parte hasta 2.000 permite observar la curva y reportar el punto de degradación sin agregar un umbral no aprobado para ese máximo. El escenario repetitivo reporta el porcentaje de `X-Cache: HIT` frente al 80 % de EQ-08. La ejecución guarda `--summary-export` bajo `docs/evidencias/`.

## Risks / Trade-offs

- **Conteo desactualizado hasta 60 s** → presentarlo como informativo; el flujo transaccional de reservas vuelve a comprobar disponibilidad en MI04 y no forma parte de esta interfaz.
- **Composición de resultados entre cuatro módulos puede elevar latencia** → contratos públicos batch, índices en esquemas propietarios, cache de MI06 y medición sin/con Redis; no llamar un contrato por viaje.
- **El cambio de disponibilidad MI04 no invalida claves en forma inmediata** → tolerar el TTL documentado; no introducir invalidación distribuida ni eventos de reserva dentro del alcance.
- **Un test de 2.000 VUs puede exceder el entorno local** → separar el criterio de EQ-01 de la observación de escalabilidad, y guardar el summary junto con entorno, versión de datos y comando ejecutado.
- **Layouts de buses no tienen geometría detallada en Stitch** → guardar el conteo y los asientos numerados por piso; el refinamiento geométrico para el futuro mapa queda fuera de este cambio.

## Migration Plan

1. Crear migraciones de MI02, MI03 y MI04 en ese orden; en `Development`, el Host las aplica al arrancar y luego siembra IDs estables para empresas/agencias, rutas/modelos/vehículos/viajes y `TripSeat`. En otros entornos se aplican mediante el proceso operativo de despliegue.
2. Desplegar MI06, contratos públicos, endpoint y Redis; ante rollback, retirar el endpoint/consumidor y revertir solo las tablas, índices y semillas nuevas en sus esquemas. Las claves Redis incluyen versión y pueden expirar por TTL.
3. Desplegar el frontend cuando el endpoint esté operativo; rollback del frontend vuelve a la pantalla previa y no altera datos.

## Open Questions

Ninguna que bloquee la planificación. La carga de EQ-01 se acepta en el umbral documentado desde 200 VUs; hasta 2.000 VUs se reporta como rampa exploratoria, según la decisión acordada.
