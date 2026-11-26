using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class AgencyCatalogDomainTests
{
    [Fact]
    public void FareValidityIncludesBothBoundsAndCanBeDeactivated()
    {
        var fare = NewFare(FareType.Base, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        Assert.True(fare.AppliesTo(new DateOnly(2026, 11, 1)));
        Assert.True(fare.AppliesTo(new DateOnly(2026, 11, 30)));
        Assert.False(fare.AppliesTo(new DateOnly(2026, 12, 1)));

        fare.SetActive(false);
        Assert.False(fare.AppliesTo(new DateOnly(2026, 11, 15)));
    }

    [Fact]
    public void FarePeriodsOverlapInclusivelyOnlyForSameRouteAndType()
    {
        var fare = NewFare(FareType.Special, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10));
        var touching = NewFare(FareType.Special, new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 20), fare.RouteId);
        var separateType = NewFare(FareType.Base, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10), fare.RouteId);
        var separateRoute = NewFare(FareType.Special, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10));

        Assert.True(fare.Overlaps(touching));
        Assert.False(fare.Overlaps(separateType));
        Assert.False(fare.Overlaps(separateRoute));
    }

    [Fact]
    public void FareResolutionPrefersSpecialAndFallsBackToBase()
    {
        var date = new DateOnly(2026, 11, 15);
        var route = Guid.NewGuid();
        var baseFare = NewFare(FareType.Base, new DateOnly(2026, 11, 1), null, route);
        var specialFare = NewFare(FareType.Special, new DateOnly(2026, 11, 14), new DateOnly(2026, 11, 16), route);

        Assert.Same(specialFare, Fare.ResolveFor(date, [baseFare, specialFare]));
        Assert.Same(baseFare, Fare.ResolveFor(new DateOnly(2026, 11, 17), [baseFare, specialFare]));
        Assert.Null(Fare.ResolveFor(new DateOnly(2026, 10, 31), [baseFare, specialFare]));
    }

    [Fact]
    public void FareRejectsInvalidPeriodAndNonPositivePrice()
    {
        Assert.Throws<ArgumentException>(() => NewFare(
            FareType.Base, new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Fare(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FareType.Base,
            new DateOnly(2026, 11, 1), null, 0));
    }

    [Fact]
    public void RouteAndVehicleExposeCompanyAndEditableActiveProfiles()
    {
        var companyId = Guid.NewGuid();
        var route = new Route(Guid.NewGuid(), companyId, " Ayacucho ", "Lima");
        route.SetActive(false);
        route.Update("Huanta", "San Miguel");

        var vehicle = new Vehicle(Guid.NewGuid(), companyId, Guid.NewGuid(), Guid.NewGuid(), "abc-123", "Toyota Avanza");
        vehicle.SetActive(false);
        vehicle.UpdateProfile("abc-456", "Avanza Ejecutivo", vehicle.VehicleModelId);

        Assert.Equal(companyId, route.CompanyId);
        Assert.Equal("Huanta", route.OriginCity);
        Assert.False(route.IsActive);
        Assert.Equal("ABC-456", vehicle.Plate);
        Assert.Equal("Avanza Ejecutivo", vehicle.DisplayName);
        Assert.False(vehicle.IsActive);
    }

    [Fact]
    public void TripSnapshotsDisplayDataAndTransitionsToCancelled()
    {
        var trip = new Trip(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.Parse("2026-11-25T13:00:00Z"), DateTimeOffset.Parse("2026-11-25T18:00:00Z"), 75m,
            "Ayacucho", "Lima", "Toyota Avanza Ejecutivo", "Toyota Avanza");

        Assert.Equal(TripStatus.Scheduled, trip.Status);
        Assert.Equal("Ayacucho", trip.OriginCitySnapshot);
        Assert.Equal("Lima", trip.DestinationCitySnapshot);
        Assert.Equal("Toyota Avanza Ejecutivo", trip.VehicleDisplayNameSnapshot);
        Assert.Equal("Toyota Avanza", trip.VehicleModelNameSnapshot);

        trip.Cancel();
        Assert.Equal(TripStatus.Cancelled, trip.Status);
    }

    private static Fare NewFare(FareType type, DateOnly from, DateOnly? to, Guid? routeId = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), routeId ?? Guid.NewGuid(), type, from, to, 100m);
}
