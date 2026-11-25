using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.TransportScheduling.Domain.Catalog;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class TripSearchDomainTests
{
    [Fact]
    public void AgencyRequiresACompanyAndKeepsPublicLocation()
    {
        var companyId = Guid.NewGuid();
        var agency = new Agency(Guid.NewGuid(), companyId, " Terminal Centro ", " Ayacucho ");

        Assert.Equal(companyId, agency.CompanyId);
        Assert.Equal("Terminal Centro", agency.Name);
        Assert.Equal("Ayacucho", agency.City);
        Assert.Throws<ArgumentException>(() => new Agency(Guid.NewGuid(), Guid.Empty, "Agency", "Lima"));
    }

    [Fact]
    public void CompanyNormalizesAdministrativeDataAndCanBeDeactivated()
    {
        var company = new Company(Guid.NewGuid(), " Andes Express ", "20123456789", " Contacto ", " 999 123 456 ");

        Assert.Equal("Andes Express", company.Name);
        Assert.Equal("20123456789", company.TaxId);
        Assert.Equal("Contacto", company.ContactName);
        Assert.Equal("999 123 456", company.ContactPhone);
        Assert.True(company.IsActive);

        company.SetActive(false);
        Assert.False(company.IsActive);
        Assert.Throws<ArgumentException>(() => company.UpdateProfile("Andes Express", "123", null, null));
    }

    [Fact]
    public void AgencyCanUpdateItsAdministrativeDataAndStatus()
    {
        var agency = new Agency(Guid.NewGuid(), Guid.NewGuid(), "Terminal Centro", "Ayacucho", " Jr. Lima 123 ", " 066 123456 ");

        Assert.Equal("Jr. Lima 123", agency.Address);
        Assert.Equal("066 123456", agency.Phone);
        Assert.True(agency.IsActive);

        agency.UpdateProfile("Terminal Norte", "Huanta", "Av. Principal 10", null);
        agency.SetActive(false);

        Assert.Equal("Terminal Norte", agency.Name);
        Assert.Equal("Huanta", agency.City);
        Assert.Null(agency.Phone);
        Assert.False(agency.IsActive);
    }

    [Fact]
    public void RouteRejectsSameOriginAndDestination()
    {
        Assert.Throws<ArgumentException>(() => new Route(Guid.NewGuid(), "Ayacucho", " ayacucho "));
    }

    [Fact]
    public void TripStoresUtcAndRequiresArrivalAfterDeparture()
    {
        var localDeparture = new DateTimeOffset(2026, 11, 23, 20, 0, 0, TimeSpan.FromHours(-5));
        var localArrival = localDeparture.AddHours(8);
        var trip = new Trip(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            localDeparture, localArrival, 60m);

        Assert.Equal(localDeparture.ToUniversalTime(), trip.DepartureAtUtc);
        Assert.Equal(localArrival.ToUniversalTime(), trip.ArrivalAtUtc);
        Assert.Equal("PEN", trip.Currency);
        Assert.Throws<ArgumentException>(() => new Trip(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), localDeparture, localDeparture, 60m));
    }

    [Fact]
    public void VehicleModelCapacityMustMatchItsSeatLayout()
    {
        var layout = new SeatLayout(new[] { new SeatPosition(1, 1, 1), new SeatPosition(2, 1, 2) });

        Assert.Throws<ArgumentException>(() => new VehicleModel(Guid.NewGuid(), "Model", 3, layout));
    }

    [Fact]
    public void VehicleModelCanBeRenamedAndDeactivatedWithoutChangingItsLayout()
    {
        var layout = new SeatLayout(new[] { new SeatPosition(1, 1, 1), new SeatPosition(2, 1, 2) });
        var model = new VehicleModel(Guid.NewGuid(), "Modelo original", 2, layout);

        model.UpdateProfile("Modelo actualizado", 2, layout);
        model.SetActive(false);

        Assert.Equal("Modelo actualizado", model.Name);
        Assert.Equal(2, model.Capacity);
        Assert.Same(layout, model.SeatLayout);
        Assert.False(model.IsActive);
    }

    [Fact]
    public void VehicleModelRejectsInvalidLayoutUpdates()
    {
        var initialLayout = new SeatLayout(new[] { new SeatPosition(1, 1, 1) });
        var model = new VehicleModel(Guid.NewGuid(), "Modelo", 1, initialLayout);
        var invalidLayout = new SeatLayout(new[] { new SeatPosition(1, 1, 1), new SeatPosition(2, 1, 2) });

        Assert.Throws<ArgumentException>(() => model.UpdateProfile("Modelo", 3, invalidLayout));
        Assert.Same(initialLayout, model.SeatLayout);
    }

    [Fact]
    public void StandardModelsHaveTheExpectedSeatCountsAndFloors()
    {
        var models = StandardVehicleModels.Create();

        Assert.Equal(new[] { 4, 6, 7, 15, 44, 52 }, models.Select(model => model.Capacity));
        Assert.All(models, model =>
        {
            Assert.Equal(model.Capacity, model.SeatLayout.Seats.Select(seat => seat.Number).Distinct().Count());
            Assert.Equal(Enumerable.Range(1, model.Capacity), model.SeatLayout.Seats.Select(seat => seat.Number));
        });

        var doubleDeck = models.Single(model => model.Name == "Bus 2 pisos");
        Assert.Equal(12, doubleDeck.SeatLayout.Seats.Count(seat => seat.Floor == 1));
        Assert.Equal(40, doubleDeck.SeatLayout.Seats.Count(seat => seat.Floor == 2));
        Assert.Equal(new[] { "Sedán", "Familiar", "Familiar", "Transporte", "Bus", "Bus" },
            models.Select(model => model.Category));
    }

    [Fact]
    public void SeatLayoutRejectsDuplicateSeatNumbersAndPositions()
    {
        Assert.Throws<ArgumentException>(() => new SeatLayout(new[]
        {
            new SeatPosition(1, 1, 1),
            new SeatPosition(1, 1, 2)
        }));

        Assert.Throws<ArgumentException>(() => new SeatLayout(new[]
        {
            new SeatPosition(1, 1, 1),
            new SeatPosition(2, 1, 1)
        }));
    }

    [Fact]
    public void TripSeatIsCreatedWithTheSpecifiedAvailabilityState()
    {
        var tripSeat = new TripSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.Equal(TripSeatStatus.Available, tripSeat.Status);
        Assert.Throws<ArgumentOutOfRangeException>(() => new TripSeat(Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), 0));
    }
}
