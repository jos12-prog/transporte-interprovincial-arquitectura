using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;

namespace TransportPlatform.Modules.TransportScheduling.Application.Contracts;

public sealed record SaveRouteInput(string OriginCity, string DestinationCity);
public sealed record SaveVehicleInput(string Plate, string DisplayName, Guid VehicleModelId);
public sealed record SaveFareInput(FareType Type, DateOnly ValidFrom, DateOnly? ValidTo, decimal Price, string Currency = "PEN");
public sealed record ScheduleTripInput(Guid RouteId, Guid VehicleId, DateTimeOffset DepartureAtUtc, DateTimeOffset ArrivalAtUtc);

public sealed record AgencyOperationScope(Guid CompanyId, Guid AgencyId);

public sealed class AgencyCatalogValidationException(string message) : Exception(message);
public sealed class AgencyCatalogNotFoundException(string message) : Exception(message);
public sealed class AgencyCatalogConflictException(string message) : Exception(message);
public sealed class AgencyCatalogForbiddenException(string message) : Exception(message);

public interface IAgencyCatalogRepository
{
    Task<IReadOnlyList<Route>> GetRoutesAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Route?> GetRouteAsync(Guid routeId, Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> RouteExistsAsync(Guid routeId, Guid companyId, CancellationToken cancellationToken = default);
    Task AddRouteAsync(Route route, CancellationToken cancellationToken = default);
    Task SaveRouteAsync(Route route, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetVehicleAsync(Guid vehicleId, Guid companyId, Guid agencyId, CancellationToken cancellationToken = default);
    Task<bool> VehiclePlateExistsAsync(Guid companyId, string normalizedPlate, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task<bool> VehicleHasTripsAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task SaveVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Fare>> GetFaresAsync(Guid routeId, Guid companyId, CancellationToken cancellationToken = default);
    Task<Fare?> GetFareAsync(Guid fareId, Guid routeId, Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> FarePeriodOverlapsAsync(Guid routeId, Guid companyId, FareType type, DateOnly validFrom, DateOnly? validTo, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task AddFareAsync(Fare fare, CancellationToken cancellationToken = default);
    Task SaveFareAsync(Fare fare, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Trip>> GetTripsAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default);
    Task<Trip?> GetTripAsync(Guid tripId, Guid companyId, Guid agencyId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingScheduledTripAsync(Guid vehicleId, DateTimeOffset departureAtUtc, DateTimeOffset arrivalAtUtc, Guid? excludingTripId = null, CancellationToken cancellationToken = default);
}

public interface IAgencyScopeValidator
{
    Task<bool> IsActiveAgencyOfCompanyAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default);
}

public interface ITripSchedulingCoordinator
{
    Task CreateTripWithSeatsAsync(Trip trip, SeatLayout layout, CancellationToken cancellationToken = default);
    Task<bool> CancelTripAndReleaseHoldsAsync(Trip trip, CancellationToken cancellationToken = default);
}
