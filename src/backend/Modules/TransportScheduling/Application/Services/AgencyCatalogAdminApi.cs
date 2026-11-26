using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.UseCases;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Shared.Contracts.Administration;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.TransportScheduling.Application.Services;

public sealed class AgencyCatalogAdminApi(
    IAgencyCatalogRepository repository,
    IAgencyScopeValidator scopeValidator,
    IVehicleModelAdministrationRepository models,
    ITripSchedulingCoordinator coordinator,
    IQueriesReportsCacheControl cacheControl) : IAgencyCatalogAdminApi
{
    public async Task<IReadOnlyList<RouteAdminDto>> GetRoutesAsync(AdminAgencyScope scope, CancellationToken ct = default) =>
        (await new GetRoutesUseCase(repository, scopeValidator).ExecuteAsync(Map(scope), ct)).Select(ToDto).ToArray();
    public async Task<RouteAdminDto?> GetRouteAsync(Guid id, AdminAgencyScope scope, CancellationToken ct = default) =>
        (await repository.GetRouteAsync(id, scope.CompanyId, ct)) is { } route ? ToDto(route) : null;
    public async Task<RouteAdminDto> CreateRouteAsync(SaveRouteRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var route = await new SaveRouteUseCase(repository, scopeValidator).ExecuteAsync(null, Map(scope), new(request.OriginCity, request.DestinationCity), ct); await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(route); }
    public async Task<RouteAdminDto?> UpdateRouteAsync(Guid id, SaveRouteRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var route = await new SaveRouteUseCase(repository, scopeValidator).ExecuteAsync(id, Map(scope), new(request.OriginCity, request.DestinationCity), ct); if (route is null) return null; await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(route); }
    public async Task<RouteAdminDto?> SetRouteActiveAsync(Guid id, bool active, AdminAgencyScope scope, CancellationToken ct = default)
    { var route = await new SetRouteActiveUseCase(repository, scopeValidator).ExecuteAsync(id, Map(scope), active, ct); if (route is null) return null; await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(route); }

    public async Task<IReadOnlyList<VehicleAdminDto>> GetVehiclesAsync(AdminAgencyScope scope, CancellationToken ct = default) =>
        await ToVehicleDtos(await new GetVehiclesUseCase(repository, scopeValidator).ExecuteAsync(Map(scope), ct), ct);
    public async Task<VehicleAdminDto?> GetVehicleAsync(Guid id, AdminAgencyScope scope, CancellationToken ct = default)
    { var vehicle = await repository.GetVehicleAsync(id, scope.CompanyId, scope.AgencyId, ct); return vehicle is null ? null : (await ToVehicleDtos([vehicle], ct))[0]; }
    public async Task<VehicleAdminDto> CreateVehicleAsync(SaveVehicleRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var vehicle = await new SaveVehicleUseCase(repository, scopeValidator, models).ExecuteAsync(null, Map(scope), new(request.Plate, request.DisplayName, request.VehicleModelId), ct); await cacheControl.InvalidateTripSearchAsync(ct); return (await ToVehicleDtos([vehicle], ct))[0]; }
    public async Task<VehicleAdminDto?> UpdateVehicleAsync(Guid id, SaveVehicleRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var vehicle = await new SaveVehicleUseCase(repository, scopeValidator, models).ExecuteAsync(id, Map(scope), new(request.Plate, request.DisplayName, request.VehicleModelId), ct); if (vehicle is null) return null; await cacheControl.InvalidateTripSearchAsync(ct); return (await ToVehicleDtos([vehicle], ct))[0]; }
    public async Task<VehicleAdminDto?> SetVehicleActiveAsync(Guid id, bool active, AdminAgencyScope scope, CancellationToken ct = default)
    { var vehicle = await new SetVehicleActiveUseCase(repository, scopeValidator).ExecuteAsync(id, Map(scope), active, ct); if (vehicle is null) return null; await cacheControl.InvalidateTripSearchAsync(ct); return (await ToVehicleDtos([vehicle], ct))[0]; }

    public async Task<IReadOnlyList<FareAdminDto>> GetFaresAsync(Guid routeId, AdminAgencyScope scope, CancellationToken ct = default) =>
        (await new GetFaresUseCase(repository, scopeValidator).ExecuteAsync(routeId, Map(scope), ct)).Select(ToDto).ToArray();
    public async Task<FareAdminDto> CreateFareAsync(Guid routeId, SaveFareRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var fare = await new SaveFareUseCase(repository, scopeValidator).ExecuteAsync(routeId, null, Map(scope), FareInput(request), ct); await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(fare); }
    public async Task<FareAdminDto?> UpdateFareAsync(Guid routeId, Guid id, SaveFareRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var fare = await new SaveFareUseCase(repository, scopeValidator).ExecuteAsync(routeId, id, Map(scope), FareInput(request), ct); if (fare is null) return null; await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(fare); }
    public async Task<FareAdminDto?> SetFareActiveAsync(Guid routeId, Guid id, bool active, AdminAgencyScope scope, CancellationToken ct = default)
    { var fare = await new SetFareActiveUseCase(repository, scopeValidator).ExecuteAsync(routeId, id, Map(scope), active, ct); if (fare is null) return null; await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(fare); }

    public async Task<IReadOnlyList<TripAdminDto>> GetTripsAsync(AdminAgencyScope scope, CancellationToken ct = default) =>
        (await new GetTripsUseCase(repository, scopeValidator).ExecuteAsync(Map(scope), ct)).Select(ToDto).ToArray();
    public async Task<TripAdminDto> ScheduleTripAsync(ScheduleTripRequest request, AdminAgencyScope scope, CancellationToken ct = default)
    { var trip = await new ScheduleTripUseCase(repository, scopeValidator, models, coordinator).ExecuteAsync(Map(scope), new(request.RouteId, request.VehicleId, request.DepartureAtUtc, request.ArrivalAtUtc), ct); await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(trip); }
    public async Task<TripAdminDto?> CancelTripAsync(Guid id, AdminAgencyScope scope, CancellationToken ct = default)
    { var trip = await new CancelTripUseCase(repository, scopeValidator, coordinator).ExecuteAsync(id, Map(scope), ct); await cacheControl.InvalidateTripSearchAsync(ct); return ToDto(trip); }

    private async Task<IReadOnlyList<VehicleAdminDto>> ToVehicleDtos(IReadOnlyList<Vehicle> items, CancellationToken ct)
    {
        var ids = items.Select(x => x.VehicleModelId).Distinct().ToArray();
        var modelDtos = (await models.GetVehicleModelsAsync(false, ct)).Where(model => ids.Contains(model.Id)).ToDictionary(model => model.Id);
        return items.Select(x =>
        {
            modelDtos.TryGetValue(x.VehicleModelId, out var model);
            return new VehicleAdminDto(x.Id, x.CompanyId, x.AgencyId, x.VehicleModelId, x.Plate, x.DisplayName,
                model?.Name ?? "", x.IsActive, model?.SeatLayout.Seats.Select(seat => new TransportPlatform.Shared.Contracts.Trips.SeatPositionDto(seat.Number, seat.Row, seat.Column, seat.Floor)).ToArray());
        }).ToArray();
    }
    private static AgencyOperationScope Map(AdminAgencyScope x) => new(x.CompanyId, x.AgencyId);
    private static SaveFareInput FareInput(SaveFareRequest x) => new(Enum.Parse<FareType>(x.Type, true), x.ValidFrom, x.ValidTo, x.Price, x.Currency);
    private static RouteAdminDto ToDto(Route x) => new(x.Id, x.CompanyId, x.OriginCity, x.DestinationCity, x.IsActive);
    private static FareAdminDto ToDto(Fare x) => new(x.Id, x.CompanyId, x.RouteId, x.Type.ToString(), x.ValidFrom, x.ValidTo, x.Price, x.Currency, x.IsActive);
    private static TripAdminDto ToDto(Trip x) => new(x.Id, x.CompanyId, x.AgencyId, x.RouteId, x.VehicleId, x.OriginCitySnapshot, x.DestinationCitySnapshot, x.VehicleDisplayNameSnapshot, x.VehicleModelNameSnapshot, x.DepartureAtUtc, x.ArrivalAtUtc, x.Price, x.Currency, x.Status.ToString());
}
