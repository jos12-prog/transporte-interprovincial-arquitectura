# SOLID y patrones en el Sprint 2

Este documento enlaza los principios con código comprobable en el repositorio. Algunos principios se reflejan en límites y contratos, no en una demostración formal de que todo el diseño los satisface. Los ejemplos son del esqueleto y los flujos de búsqueda y retención implementados.

## Relación arquitectónica

La solución organiza cada módulo como `Domain`, `Application`, `Infrastructure` y `Presentation`. Los casos de uso dependen de contratos de Aplicación; Infraestructura implementa esos contratos con EF Core, Redis y servicios de hospedaje. Los módulos se comunican mediante interfaces públicas en `Shared.Contracts`, evitando referencias directas desde el dominio o la aplicación a otros módulos. Esta combinación corresponde al monolito modular y aplica la inversión de dependencias de Clean Architecture.

La prueba [LayeringTests.cs](../../tests/Architecture/LayeringTests.cs) usa NetArchTest para comprobar que los assemblies Domain no dependan de EF Core, ASP.NET, Infrastructure, Presentation ni otros módulos, y que Application no dependa de Infrastructure, Presentation ni otros módulos. La prueba valida esas reglas de ensamblados; no pretende demostrar por sí sola todos los principios SOLID.

## Principios SOLID

### S — Responsabilidad única

`SearchTripsUseCase` coordina y valida la búsqueda, consulta contratos de módulos y usa la caché; el acceso a Redis queda separado en `RedisTripSearchCache` y la lectura persistente en `EfTripSearchReadRepository`.

```csharp
// src/backend/Modules/QueriesReports/Application/UseCases/SearchTripsUseCase.cs
public sealed class SearchTripsUseCase(
    ITransportSchedulingApi transportSchedulingApi,
    ICompaniesAgenciesApi companiesAgenciesApi,
    ITripSearchCache tripSearchCache)
```

La clase tiene responsabilidades propias de orquestación de la búsqueda. Redis y EF Core son responsabilidades de implementaciones de infraestructura.

### O — Abierto/cerrado

`SearchTripsUseCase` trabaja con `ITripSearchCache`. Se puede sustituir la implementación de caché desde el registro de infraestructura sin cambiar el caso de uso:

```csharp
// src/backend/Modules/QueriesReports/Infrastructure/InfrastructureRegistration.cs
services.AddSingleton<ITripSearchCache, RedisTripSearchCache>();
```

Esto permite extender el comportamiento a través de una implementación del contrato. El ejemplo no afirma que exista hoy una segunda implementación de producción.

### L — Sustitución de Liskov

`EfReservationRepository` implementa `IReservationRepository`; los casos de uso consumen el contrato. Las implementaciones deben conservar las precondiciones y resultados esperados del contrato (por ejemplo, respetar cancelación y representar conflictos como los errores de dominio previstos).

```csharp
// src/backend/Modules/Reservations/Infrastructure/Persistence/EfReservationRepository.cs
internal sealed class EfReservationRepository(ReservationsDbContext dbContext)
    : IReservationRepository
```

La existencia de la implementación prueba el punto de sustitución estructural; el contrato no cuenta actualmente con varias implementaciones equivalentes de persistencia.

### I — Segregación de interfaces

Los contratos separan capacidades. La búsqueda de lectura expone búsqueda y plano de asientos, mientras que el repositorio de reservas expone lectura de asientos, creación, liberación y expiración de holds. Ninguno expone el `DbContext` ni métodos genéricos de persistencia.

```csharp
// src/backend/Modules/TransportScheduling/Application/Contracts/ITripSearchReadRepository.cs
Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(...);
Task<SeatLayoutDto?> GetSeatLayoutAsync(...);
```

```csharp
// src/backend/Modules/Reservations/Application/Contracts/IReservationRepository.cs
Task CreateHoldAsync(...);
Task<bool> CancelHoldAsync(...);
Task<int> ExpireHoldsAsync(...);
```

### D — Inversión de dependencias

El caso de uso de retención recibe `IReservationRepository` y `TimeProvider`; no crea un repositorio EF ni accede directamente a PostgreSQL. El registro conecta el contrato con `EfReservationRepository` en Infraestructura.

```csharp
// src/backend/Modules/Reservations/Infrastructure/InfrastructureRegistration.cs
services.AddScoped<IReservationRepository, EfReservationRepository>();
```

La regla se refuerza con `LayeringTests` de NetArchTest descrita arriba.

## Patrones implementados

### Repository

Hay contratos específicos de lectura y escritura y una implementación de persistencia para reservas:

```csharp
// src/backend/Modules/Reservations/Application/Contracts/IReservationRepository.cs
public interface IReservationRepository
{
    Task CreateHoldAsync(Reservation reservation, CancellationToken cancellationToken = default);
```

```csharp
// src/backend/Modules/TransportScheduling/Application/Contracts/ITripSearchReadRepository.cs
public interface ITripSearchReadRepository
{
    Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(...);
```

La implementación de `IReservationRepository` se llama `EfReservationRepository` y está en `src/backend/Modules/Reservations/Infrastructure/Persistence/EfReservationRepository.cs`.

### Adapter / Gateway

No se encontraron clases `CulqiPaymentAdapter`, `WhatsAppNotificationAdapter` ni contratos `IPaymentGateway`/`INotificationGateway` implementados en `src/backend`. Pagos externos y WhatsApp están fuera del alcance del Sprint 2. Por eso no se presenta una integración externa ficticia como ejemplo ya implementado.

Sí está implementado el límite de módulos por contratos Gateway/API. Por ejemplo, `TransportSchedulingApi` implementa `ITransportSchedulingApi`; el registro enlaza ambas piezas. Es una fachada/adaptador interno del módulo y no un adaptador hacia un proveedor externo:

```csharp
// src/backend/Modules/TransportScheduling/Application/Services/TransportSchedulingApi.cs
public sealed class TransportSchedulingApi(...) : ITransportSchedulingApi
```

```csharp
// src/backend/Shared/TransportPlatform.Shared.Contracts/Modules/ITransportSchedulingApi.cs
public interface ITransportSchedulingApi
```

### Contratos entre módulos (`Shared.Contracts`)

Las APIs públicas permiten que un módulo consulte a otro sin referenciar sus tablas ni su implementación concreta. `QueriesReports` coordina `ITransportSchedulingApi` e `ICompaniesAgenciesApi`; `TransportSchedulingApi` obtiene conteos mediante `IReservationsApi`.

```csharp
// src/backend/Shared/TransportPlatform.Shared.Contracts/Modules/IReservationsApi.cs
public interface IReservationsApi
{
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(...);
}
```

Los tipos compartidos de búsqueda están en `Shared.Contracts/Trips`. NetArchTest comprueba que Domain y Application no referencien assemblies de otros módulos.

### Use Case

`CreateReservationHoldUseCase` valida entrada, selecciona los asientos solicitados, genera el token opaco, construye la reserva y solicita su persistencia mediante el repositorio:

```csharp
// src/backend/Modules/Reservations/Application/UseCases/CreateReservationHoldUseCase.cs
public sealed class CreateReservationHoldUseCase(IReservationRepository repository, TimeProvider timeProvider)
```

El endpoint traduce los resultados/errores del caso de uso a HTTP y ProblemDetails, sin implementar el proceso de retención.

### Inyección de dependencias

Los módulos registran contratos y clases concretas en sus extensiones de infraestructura. Ejemplos: `IReservationRepository → EfReservationRepository`, `ITripSearchCache → RedisTripSearchCache`, y `ITripSearchReadRepository → EfTripSearchReadRepository`.

```csharp
// src/backend/Modules/QueriesReports/Infrastructure/InfrastructureRegistration.cs
services.AddSingleton<ITripSearchCache, RedisTripSearchCache>();
services.AddScoped<ISearchTripsUseCase, SearchTripsUseCase>();
```

### Cache-Aside

`SearchTripsUseCase` intenta leer la caché, obtiene la búsqueda desde los contratos de módulo ante un miss y luego almacena el resultado. El TTL configurado para búsqueda es 60 segundos. El mapa de asientos no pasa por esta caché.

```csharp
// src/backend/Modules/QueriesReports/Application/UseCases/SearchTripsUseCase.cs
var cached = await tripSearchCache.GetAsync(cacheKey, cancellationToken);
if (cached is not null) return new TripSearchExecutionResult(cached, CacheHit: true);
```

La implementación Redis está en `src/backend/Modules/QueriesReports/Infrastructure/Caching/RedisTripSearchCache.cs`.

### Circuit Breaker

`RedisTripSearchCache` limita la operación de caché a 300 ms y, ante una excepción, abre el breaker por 30 segundos. Mientras está abierto devuelve miss/no escribe y la búsqueda sigue con PostgreSQL.

```csharp
// src/backend/Modules/QueriesReports/Infrastructure/Caching/RedisTripSearchCache.cs
private static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(30);
```

La prueba `FailedGetOpensBreakerAndImmediateGetAndSetSkipDistributedCache` de `tests/Unit/RedisTripSearchCacheTests.cs` cubre la apertura y las llamadas inmediatas. `bd-y-cache.md` registra el resultado real con Redis detenido y la relación con la Táctica 5.

### Background Worker con `SKIP LOCKED`

`ReservationHoldExpirationWorker` corre en segundo plano cada 10 segundos, crea un scope de DI y ejecuta `ExpireHoldsAsync`. La consulta del repositorio selecciona reservas vencidas con `FOR UPDATE SKIP LOCKED`, permitiendo que un trabajador concurrente omita filas ya bloqueadas.

```csharp
// src/backend/Modules/Reservations/Infrastructure/Persistence/EfReservationRepository.cs
... ORDER BY expires_at_utc, id FOR UPDATE SKIP LOCKED
```

### Bloqueo pesimista con `FOR UPDATE`

Al crear el hold, `EfReservationRepository` abre una transacción y bloquea las filas `TripSeat` solicitadas antes de verificar estado y marcarlas retenidas. La verificación de concurrencia está respaldada además por el índice único parcial de la base.

```csharp
// src/backend/Modules/Reservations/Infrastructure/Persistence/EfReservationRepository.cs
... ORDER BY id FOR UPDATE
```

El resultado real de la prueba de concurrencia EQ-02 está en `docs/evidencias/concurrencia-eq02.md` y su JSON de k6 asociado.
