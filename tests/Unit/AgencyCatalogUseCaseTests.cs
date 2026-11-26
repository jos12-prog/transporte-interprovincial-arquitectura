using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.Services;
using TransportPlatform.Modules.TransportScheduling.Application.UseCases;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;
using TransportPlatform.Shared.Contracts.Administration;
using TransportPlatform.Shared.Contracts.Modules;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class AgencyCatalogUseCaseTests
{
    private static readonly AgencyOperationScope Scope = new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task SaveRouteCreatesCompanyScopedRoute()
    {
        var repository = new AgencyCatalogRepositoryStub();
        var route = await new SaveRouteUseCase(repository, new AgencyScopeValidatorStub()).ExecuteAsync(
            null, Scope, new SaveRouteInput("Ayacucho", "Lima"));

        Assert.Equal(Scope.CompanyId, route.CompanyId);
        Assert.Equal("Ayacucho", route.OriginCity);
        Assert.Same(route, repository.AddedRoute);
    }

    [Fact]
    public async Task SaveRouteRejectsInvalidDataAndMissingRoute()
    {
        var useCase = new SaveRouteUseCase(new AgencyCatalogRepositoryStub(), new AgencyScopeValidatorStub());
        await Assert.ThrowsAsync<AgencyCatalogValidationException>(() => useCase.ExecuteAsync(
            null, Scope, new SaveRouteInput(" ", "Lima")));
        await Assert.ThrowsAsync<AgencyCatalogNotFoundException>(() => useCase.ExecuteAsync(
            Guid.NewGuid(), Scope, new SaveRouteInput("Ayacucho", "Lima")));
    }

    [Fact]
    public async Task SaveRouteRejectsUnauthorizedAgencyScope()
    {
        var useCase = new SaveRouteUseCase(new AgencyCatalogRepositoryStub(), new AgencyScopeValidatorStub(false));
        await Assert.ThrowsAsync<AgencyCatalogForbiddenException>(() => useCase.ExecuteAsync(
            null, Scope, new SaveRouteInput("Ayacucho", "Lima")));
    }

    [Fact]
    public async Task SaveVehicleRejectsDuplicatePlateAndInactiveModel()
    {
        var duplicateRepository = new AgencyCatalogRepositoryStub { PlateExists = true };
        var vehicleModels = new VehicleModelRepositoryStub(ActiveModel());
        var useCase = new SaveVehicleUseCase(duplicateRepository, new AgencyScopeValidatorStub(), vehicleModels);
        await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(
            null, Scope, new SaveVehicleInput("abc-123", "Bus", vehicleModels.Model.Id)));

        var inactive = ActiveModel();
        inactive.SetActive(false);
        useCase = new SaveVehicleUseCase(new AgencyCatalogRepositoryStub(), new AgencyScopeValidatorStub(), new VehicleModelRepositoryStub(inactive));
        await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(
            null, Scope, new SaveVehicleInput("abc-123", "Bus", inactive.Id)));
    }

    [Fact]
    public async Task SaveVehicleCannotChangeModelWhenVehicleHasTrips()
    {
        var existing = new Vehicle(Guid.NewGuid(), Scope.CompanyId, Scope.AgencyId, Guid.NewGuid(), "abc-123", "Bus");
        var repository = new AgencyCatalogRepositoryStub { ExistingVehicle = existing, VehicleHasTripsResult = true };
        var model = ActiveModel();
        var useCase = new SaveVehicleUseCase(repository, new AgencyScopeValidatorStub(), new VehicleModelRepositoryStub(model));

        await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(
            existing.Id, Scope, new SaveVehicleInput("abc-123", "Bus", model.Id)));
    }

    [Fact]
    public async Task SaveFareRejectsOverlappingPeriod()
    {
        var repository = new AgencyCatalogRepositoryStub { FareOverlap = true };
        var useCase = new SaveFareUseCase(repository, new AgencyScopeValidatorStub());

        await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(
            Guid.NewGuid(), null, Scope,
            new SaveFareInput(FareType.Special, new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31), 120m)));
    }

    [Fact]
    public async Task InactiveRouteStillAllowsFareDeactivationAndReactivation()
    {
        var route = new Route(Guid.NewGuid(), Scope.CompanyId, "Ayacucho", "Lima");
        var fare = new Fare(Guid.NewGuid(), Scope.CompanyId, route.Id, FareType.Base,
            new DateOnly(2026, 11, 1), null, 80m);
        var repository = new AgencyCatalogRepositoryStub { ExistingRoute = route, Fares = [fare] };
        var api = new AgencyCatalogAdminApi(repository, new AgencyScopeValidatorStub(),
            new VehicleModelRepositoryStub(ActiveModel()), new TripSchedulingCoordinatorStub(), new CacheControlStub());
        var adminScope = new AdminAgencyScope(Scope.CompanyId, Scope.AgencyId);

        route.SetActive(false);
        var deactivatedFare = await api.SetFareActiveAsync(route.Id, fare.Id, false, adminScope);
        var reactivatedFare = await api.SetFareActiveAsync(route.Id, fare.Id, true, adminScope);
        var reactivatedRoute = await api.SetRouteActiveAsync(route.Id, true, adminScope);

        Assert.False(deactivatedFare!.IsActive);
        Assert.True(reactivatedFare!.IsActive);
        Assert.True(reactivatedRoute!.IsActive);
    }

    [Fact]
    public async Task CatalogQueriesValidateScopeBeforeReturningCompanyAndAgencyData()
    {
        var repository = new AgencyCatalogRepositoryStub();
        var scopeValidator = new AgencyScopeValidatorStub();

        Assert.Empty(await new GetRoutesUseCase(repository, scopeValidator).ExecuteAsync(Scope));
        Assert.Empty(await new GetVehiclesUseCase(repository, scopeValidator).ExecuteAsync(Scope));
        Assert.Empty(await new GetFaresUseCase(repository, scopeValidator).ExecuteAsync(Guid.NewGuid(), Scope));
        Assert.Empty(await new GetTripsUseCase(repository, scopeValidator).ExecuteAsync(Scope));
    }

    [Fact]
    public async Task ScheduleTripUsesSpecialFareAndCopiesTripSnapshots()
    {
        var route = new Route(Guid.NewGuid(), Scope.CompanyId, "Ayacucho", "Lima");
        var model = ActiveModel();
        var vehicle = new Vehicle(Guid.NewGuid(), Scope.CompanyId, Scope.AgencyId, model.Id, "ABC-123", "Toyota Avanza Ejecutivo");
        var baseFare = new Fare(Guid.NewGuid(), Scope.CompanyId, route.Id, FareType.Base,
            new DateOnly(2026, 11, 1), null, 80m);
        var specialFare = new Fare(Guid.NewGuid(), Scope.CompanyId, route.Id, FareType.Special,
            new DateOnly(2026, 11, 25), new DateOnly(2026, 11, 26), 95m);
        var repository = new AgencyCatalogRepositoryStub { ExistingRoute = route, ExistingVehicle = vehicle, Fares = [baseFare, specialFare] };
        var coordinator = new TripSchedulingCoordinatorStub();
        var useCase = new ScheduleTripUseCase(repository, new AgencyScopeValidatorStub(), new VehicleModelRepositoryStub(model), coordinator);

        var trip = await useCase.ExecuteAsync(Scope, new ScheduleTripInput(route.Id, vehicle.Id,
            DateTimeOffset.Parse("2026-11-25T13:00:00Z"), DateTimeOffset.Parse("2026-11-25T18:00:00Z")));

        Assert.Equal(95m, trip.Price);
        Assert.Equal("Ayacucho", trip.OriginCitySnapshot);
        Assert.Equal("Toyota Avanza Ejecutivo", trip.VehicleDisplayNameSnapshot);
        Assert.Same(trip, coordinator.CreatedTrip);
        Assert.Equal(model.SeatLayout.Capacity, coordinator.CreatedLayout?.Capacity);
    }

    [Fact]
    public async Task ScheduleTripWithoutFareReturnsSpecifiedConflictAndDoesNotCreateTrip()
    {
        var route = new Route(Guid.NewGuid(), Scope.CompanyId, "Ayacucho", "Lima");
        var model = ActiveModel();
        var vehicle = new Vehicle(Guid.NewGuid(), Scope.CompanyId, Scope.AgencyId, model.Id, "ABC-123", "Bus");
        var repository = new AgencyCatalogRepositoryStub { ExistingRoute = route, ExistingVehicle = vehicle };
        var coordinator = new TripSchedulingCoordinatorStub();
        var useCase = new ScheduleTripUseCase(repository, new AgencyScopeValidatorStub(), new VehicleModelRepositoryStub(model), coordinator);

        var exception = await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(Scope,
            new ScheduleTripInput(route.Id, vehicle.Id, DateTimeOffset.Parse("2026-11-25T13:00:00Z"), DateTimeOffset.Parse("2026-11-25T18:00:00Z"))));

        Assert.Equal("La ruta no tiene tarifa vigente para esa fecha", exception.Message);
        Assert.Null(coordinator.CreatedTrip);
    }

    [Fact]
    public async Task ScheduleTripRejectsVehicleTimeConflict()
    {
        var route = new Route(Guid.NewGuid(), Scope.CompanyId, "Ayacucho", "Lima");
        var model = ActiveModel();
        var vehicle = new Vehicle(Guid.NewGuid(), Scope.CompanyId, Scope.AgencyId, model.Id, "ABC-123", "Bus");
        var fare = new Fare(Guid.NewGuid(), Scope.CompanyId, route.Id, FareType.Base, new DateOnly(2026, 11, 1), null, 80m);
        var repository = new AgencyCatalogRepositoryStub { ExistingRoute = route, ExistingVehicle = vehicle, Fares = [fare], VehicleOverlap = true };
        var coordinator = new TripSchedulingCoordinatorStub();
        var useCase = new ScheduleTripUseCase(repository, new AgencyScopeValidatorStub(), new VehicleModelRepositoryStub(model), coordinator);

        await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(Scope,
            new ScheduleTripInput(route.Id, vehicle.Id, DateTimeOffset.Parse("2026-11-25T13:00:00Z"), DateTimeOffset.Parse("2026-11-25T18:00:00Z"))));
        Assert.Null(coordinator.CreatedTrip);
    }

    [Fact]
    public async Task CancelTripReleasesHoldsOrRejectsConfirmedSeats()
    {
        var trip = NewTrip();
        var repository = new AgencyCatalogRepositoryStub { ExistingTrip = trip };
        var coordinator = new TripSchedulingCoordinatorStub { CancellationAllowed = true };
        var useCase = new CancelTripUseCase(repository, new AgencyScopeValidatorStub(), coordinator);

        var cancelled = await useCase.ExecuteAsync(trip.Id, Scope);
        Assert.Equal(TripStatus.Cancelled, cancelled.Status);
        Assert.Same(trip, coordinator.CancelledTrip);

        var confirmedTrip = NewTrip();
        repository.ExistingTrip = confirmedTrip;
        coordinator.CancellationAllowed = false;
        await Assert.ThrowsAsync<AgencyCatalogConflictException>(() => useCase.ExecuteAsync(confirmedTrip.Id, Scope));
    }

    private static Trip NewTrip() => new(Guid.NewGuid(), Scope.CompanyId, Scope.AgencyId, Guid.NewGuid(), Guid.NewGuid(),
        DateTimeOffset.Parse("2026-11-25T13:00:00Z"), DateTimeOffset.Parse("2026-11-25T18:00:00Z"), 80m);

    private static VehicleModel ActiveModel() => new(Guid.NewGuid(), "Bus", 1,
        new SeatLayout([new SeatPosition(1, 1, 1, 1)]));

    private sealed class AgencyScopeValidatorStub(bool valid = true) : IAgencyScopeValidator
    {
        public Task<bool> IsActiveAgencyOfCompanyAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default) => Task.FromResult(valid);
    }

    private sealed class VehicleModelRepositoryStub(VehicleModel model) : IVehicleModelAdministrationRepository
    {
        public VehicleModel Model => model;
        public Task<IReadOnlyList<VehicleModel>> GetVehicleModelsAsync(bool activeOnly, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<VehicleModel>>([model]);
        public Task<IReadOnlyCollection<Guid>> GetVehicleModelIdsInUseAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Guid>>([]);
        public Task<VehicleModel?> GetVehicleModelAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<VehicleModel?>(id == model.Id ? model : null);
        public Task<bool> VehicleModelNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> IsVehicleModelInUseAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddVehicleModelAsync(VehicleModel vehicleModel, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveVehicleModelAsync(VehicleModel vehicleModel, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TripSchedulingCoordinatorStub : ITripSchedulingCoordinator
    {
        public Trip? CreatedTrip { get; private set; }
        public SeatLayout? CreatedLayout { get; private set; }
        public Trip? CancelledTrip { get; private set; }
        public bool CancellationAllowed { get; set; }

        public Task CreateTripWithSeatsAsync(Trip trip, SeatLayout layout, CancellationToken cancellationToken = default)
        {
            CreatedTrip = trip;
            CreatedLayout = layout;
            return Task.CompletedTask;
        }

        public Task<bool> CancelTripAndReleaseHoldsAsync(Trip trip, CancellationToken cancellationToken = default)
        {
            CancelledTrip = trip;
            return Task.FromResult(CancellationAllowed);
        }
    }

    private sealed class AgencyCatalogRepositoryStub : IAgencyCatalogRepository
    {
        public Route? AddedRoute { get; private set; }
        public Route? ExistingRoute { get; set; }
        public Vehicle? ExistingVehicle { get; set; }
        public bool PlateExists { get; set; }
        public bool VehicleHasTripsResult { get; set; }
        public bool FareOverlap { get; set; }
        public IReadOnlyList<Fare> Fares { get; set; } = [];
        public bool VehicleOverlap { get; set; }
        public Trip? ExistingTrip { get; set; }

        public Task<IReadOnlyList<Route>> GetRoutesAsync(Guid companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Route>>([]);
        public Task<Route?> GetRouteAsync(Guid routeId, Guid companyId, CancellationToken cancellationToken = default) => Task.FromResult(ExistingRoute?.Id == routeId ? ExistingRoute : null);
        public Task<bool> RouteExistsAsync(Guid routeId, Guid companyId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task AddRouteAsync(Route route, CancellationToken cancellationToken = default) { AddedRoute = route; return Task.CompletedTask; }
        public Task SaveRouteAsync(Route route, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Vehicle>>([]);
        public Task<Vehicle?> GetVehicleAsync(Guid vehicleId, Guid companyId, Guid agencyId, CancellationToken cancellationToken = default) => Task.FromResult(ExistingVehicle?.Id == vehicleId ? ExistingVehicle : null);
        public Task<bool> VehiclePlateExistsAsync(Guid companyId, string normalizedPlate, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(PlateExists);
        public Task<bool> VehicleHasTripsAsync(Guid vehicleId, CancellationToken cancellationToken = default) => Task.FromResult(VehicleHasTripsResult);
        public Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Fare>> GetFaresAsync(Guid routeId, Guid companyId, CancellationToken cancellationToken = default) => Task.FromResult(Fares);
        public Task<Fare?> GetFareAsync(Guid fareId, Guid routeId, Guid companyId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Fares.SingleOrDefault(fare => fare.Id == fareId && fare.RouteId == routeId && fare.CompanyId == companyId));
        public Task<bool> FarePeriodOverlapsAsync(Guid routeId, Guid companyId, FareType type, DateOnly validFrom, DateOnly? validTo, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(FareOverlap);
        public Task AddFareAsync(Fare fare, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveFareAsync(Fare fare, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Trip>> GetTripsAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Trip>>([]);
        public Task<Trip?> GetTripAsync(Guid tripId, Guid companyId, Guid agencyId, CancellationToken cancellationToken = default) => Task.FromResult(ExistingTrip?.Id == tripId ? ExistingTrip : null);
        public Task<bool> HasOverlappingScheduledTripAsync(Guid vehicleId, DateTimeOffset departureAtUtc, DateTimeOffset arrivalAtUtc, Guid? excludingTripId = null, CancellationToken cancellationToken = default) => Task.FromResult(VehicleOverlap);
    }

    private sealed class CacheControlStub : IQueriesReportsCacheControl
    {
        public Task InvalidateTripSearchAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
