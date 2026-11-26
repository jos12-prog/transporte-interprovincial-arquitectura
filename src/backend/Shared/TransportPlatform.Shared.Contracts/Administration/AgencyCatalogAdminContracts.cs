using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Shared.Contracts.Administration;

public sealed record AdminAgencyScope(Guid CompanyId, Guid AgencyId);

public sealed record RouteAdminDto(Guid RouteId, Guid CompanyId, string OriginCity, string DestinationCity, bool IsActive);
public sealed record SaveRouteRequest(string OriginCity, string DestinationCity);

public sealed record VehicleAdminDto(Guid VehicleId, Guid CompanyId, Guid AgencyId, Guid VehicleModelId,
    string Plate, string DisplayName, string VehicleModelName, bool IsActive,
    IReadOnlyList<SeatPositionDto>? SeatLayout = null);
public sealed record SaveVehicleRequest(string Plate, string DisplayName, Guid VehicleModelId);

public sealed record FareAdminDto(Guid FareId, Guid CompanyId, Guid RouteId, string Type,
    DateOnly ValidFrom, DateOnly? ValidTo, decimal Price, string Currency, bool IsActive);
public sealed record SaveFareRequest(string Type, DateOnly ValidFrom, DateOnly? ValidTo, decimal Price, string Currency = "PEN");

public sealed record TripAdminDto(Guid TripId, Guid CompanyId, Guid AgencyId, Guid RouteId, Guid VehicleId,
    string Origin, string Destination, string VehicleDisplayName, string VehicleModelName,
    DateTimeOffset DepartureAtUtc, DateTimeOffset ArrivalAtUtc, decimal Price, string Currency, string Status);
public sealed record ScheduleTripRequest(Guid RouteId, Guid VehicleId, DateTimeOffset DepartureAtUtc, DateTimeOffset ArrivalAtUtc);

public interface IAgencyCatalogAdminApi
{
    Task<IReadOnlyList<RouteAdminDto>> GetRoutesAsync(AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<RouteAdminDto?> GetRouteAsync(Guid routeId, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<RouteAdminDto> CreateRouteAsync(SaveRouteRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<RouteAdminDto?> UpdateRouteAsync(Guid routeId, SaveRouteRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<RouteAdminDto?> SetRouteActiveAsync(Guid routeId, bool isActive, AdminAgencyScope scope, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VehicleAdminDto>> GetVehiclesAsync(AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<VehicleAdminDto?> GetVehicleAsync(Guid vehicleId, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<VehicleAdminDto> CreateVehicleAsync(SaveVehicleRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<VehicleAdminDto?> UpdateVehicleAsync(Guid vehicleId, SaveVehicleRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<VehicleAdminDto?> SetVehicleActiveAsync(Guid vehicleId, bool isActive, AdminAgencyScope scope, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FareAdminDto>> GetFaresAsync(Guid routeId, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<FareAdminDto> CreateFareAsync(Guid routeId, SaveFareRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<FareAdminDto?> UpdateFareAsync(Guid routeId, Guid fareId, SaveFareRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<FareAdminDto?> SetFareActiveAsync(Guid routeId, Guid fareId, bool isActive, AdminAgencyScope scope, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TripAdminDto>> GetTripsAsync(AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<TripAdminDto> ScheduleTripAsync(ScheduleTripRequest request, AdminAgencyScope scope, CancellationToken cancellationToken = default);
    Task<TripAdminDto?> CancelTripAsync(Guid tripId, AdminAgencyScope scope, CancellationToken cancellationToken = default);
}
