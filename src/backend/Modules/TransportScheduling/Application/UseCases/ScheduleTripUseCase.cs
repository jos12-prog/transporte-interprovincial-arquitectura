using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class ScheduleTripUseCase(
    IAgencyCatalogRepository repository,
    IAgencyScopeValidator scopeValidator,
    IVehicleModelAdministrationRepository vehicleModels,
    ITripSchedulingCoordinator coordinator)
{
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);

    public async Task<Trip> ExecuteAsync(AgencyOperationScope scope, ScheduleTripInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        if (input.RouteId == Guid.Empty || input.VehicleId == Guid.Empty || input.ArrivalAtUtc <= input.DepartureAtUtc)
            throw new AgencyCatalogValidationException("A route, vehicle and valid departure/arrival interval are required.");

        var route = await repository.GetRouteAsync(input.RouteId, scope.CompanyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Route was not found.");
        if (!route.IsActive)
            throw new AgencyCatalogConflictException("An inactive route cannot be used for a new trip.");

        var vehicle = await repository.GetVehicleAsync(input.VehicleId, scope.CompanyId, scope.AgencyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Vehicle was not found in the active agency.");
        if (!vehicle.IsActive)
            throw new AgencyCatalogConflictException("An inactive vehicle cannot be used for a new trip.");

        var model = await vehicleModels.GetVehicleModelAsync(vehicle.VehicleModelId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Vehicle model was not found.");
        if (!model.IsActive)
            throw new AgencyCatalogConflictException("An inactive vehicle model cannot be used for a new trip.");

        var serviceDate = DateOnly.FromDateTime(input.DepartureAtUtc.ToOffset(LimaOffset).DateTime);
        var fares = await repository.GetFaresAsync(route.Id, scope.CompanyId, cancellationToken);
        var fare = Fare.ResolveFor(serviceDate, fares);
        if (fare is null)
            throw new AgencyCatalogConflictException("La ruta no tiene tarifa vigente para esa fecha");

        if (await repository.HasOverlappingScheduledTripAsync(vehicle.Id, input.DepartureAtUtc, input.ArrivalAtUtc, cancellationToken: cancellationToken))
            throw new AgencyCatalogConflictException("The vehicle is already assigned to a trip during this time.");

        var trip = new Trip(Guid.NewGuid(), scope.CompanyId, scope.AgencyId, route.Id, vehicle.Id,
            input.DepartureAtUtc, input.ArrivalAtUtc, fare.Price,
            route.OriginCity, route.DestinationCity, vehicle.DisplayName, model.Name, fare.Currency);
        await coordinator.CreateTripWithSeatsAsync(trip, model.SeatLayout, cancellationToken);
        return trip;
    }
}

public sealed class CancelTripUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator, ITripSchedulingCoordinator coordinator)
{
    public async Task<Trip> ExecuteAsync(Guid tripId, AgencyOperationScope scope, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        var trip = await repository.GetTripAsync(tripId, scope.CompanyId, scope.AgencyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Trip was not found.");
        if (trip.Status == TripStatus.Cancelled)
            return trip;

        trip.Cancel();
        if (!await coordinator.CancelTripAndReleaseHoldsAsync(trip, cancellationToken))
            throw new AgencyCatalogConflictException("The trip has confirmed seats and cannot be cancelled.");
        return trip;
    }
}
