using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Infrastructure;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.TransportScheduling.Domain.Catalog;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;
using TransportPlatform.Modules.TransportScheduling.Infrastructure;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;
using SchedulingRoute = TransportPlatform.Modules.TransportScheduling.Domain.Entities.Route;

namespace TransportPlatform.Host;

public static class DevelopmentDataSeeder
{
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);
    private static readonly string[] Cities = ["Ayacucho", "Lima", "Huanta", "Andahuaylas", "Cusco"];
    private static readonly (string Origin, string Destination, int DurationHours, decimal Price)[] RouteSeeds =
    [
        ("Ayacucho", "Lima", 9, 75m),
        ("Lima", "Huanta", 10, 82m),
        ("Huanta", "Andahuaylas", 6, 48m),
        ("Andahuaylas", "Cusco", 8, 65m),
        ("Cusco", "Ayacucho", 13, 110m)
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var companies = new[]
        {
            new Company(Id("company:andes-express"), "Andes Express"),
            new Company(Id("company:wari-bus"), "Wari Bus")
        };
        var agencies = companies.SelectMany(company => Cities.Select(city =>
            new Agency(Id($"agency:{company.Id:N}:{city}"), company.Id, $"{company.Name} - {city}", city))).ToArray();
        await CompaniesAgenciesDevelopmentSeeder.SeedAsync(
            scope.ServiceProvider.GetRequiredService<CompaniesAgenciesDbContext>(), companies, agencies, cancellationToken);
        await DevelopmentIdentitySeeder.SeedAsync(scope.ServiceProvider, cancellationToken);

        var models = StandardVehicleModels.Create();
        var modelByName = models.ToDictionary(model => model.Name);
        var routes = RouteSeeds.Select(seed => new SchedulingRoute(Id($"route:{seed.Origin}:{seed.Destination}"), seed.Origin, seed.Destination)).ToArray();
        var vehicles = (from company in companies
                        from city in Cities
                        let agency = agencies.Single(item => item.CompanyId == company.Id && item.City == city)
                        let model = ModelFor(city, company.Name, modelByName)
                        select new Vehicle(Id($"vehicle:{company.Id:N}:{city}"), company.Id, agency.Id, model.Id,
                            $"{(company.Name == "Andes Express" ? "AEX" : "WAR")}-{Array.IndexOf(Cities, city) + 1:D2}-{(company.Name == "Andes Express" ? "01" : "02")}"))
            .ToArray();
        var trips = new List<Trip>();
        var tripSeats = new List<TripSeat>();
        var serviceDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(LimaOffset).DateTime);
        for (var day = 0; day < 7; day++)
        {
            var date = serviceDate.AddDays(day);
            foreach (var company in companies)
            foreach (var routeSeed in RouteSeeds)
            {
                var route = routes.Single(item => item.OriginCity == routeSeed.Origin && item.DestinationCity == routeSeed.Destination);
                var agency = agencies.Single(item => item.CompanyId == company.Id && item.City == routeSeed.Origin);
                var vehicle = vehicles.Single(item => item.AgencyId == agency.Id);
                var model = modelByName.Values.Single(item => item.Id == vehicle.VehicleModelId);
                var tripId = Id($"trip:{company.Id:N}:{route.Id:N}:{date:yyyyMMdd}");
                var departure = new DateTimeOffset(date.ToDateTime(new TimeOnly(8, company.Name == "Andes Express" ? 0 : 30)), LimaOffset);
                var trip = new Trip(tripId, company.Id, agency.Id, route.Id, vehicle.Id,
                    departure, departure.AddHours(routeSeed.DurationHours), routeSeed.Price + (company.Name == "Wari Bus" ? 5m : 0m));
                trips.Add(trip);
                tripSeats.AddRange(model.SeatLayout.Seats.Select(seat => new TripSeat(
                    Id($"trip-seat:{tripId:N}:{seat.Number}"), company.Id, agency.Id, tripId, seat.Number,
                    TripSeatStatus.Available, seat.Row, seat.Column, seat.Floor)));
            }
        }

        await TransportSchedulingDevelopmentSeeder.SeedAsync(
            scope.ServiceProvider.GetRequiredService<TransportSchedulingDbContext>(), routes, models, vehicles, trips, cancellationToken);
        await ReservationsDevelopmentSeeder.SeedAsync(
            scope.ServiceProvider.GetRequiredService<ReservationsDbContext>(), tripSeats, cancellationToken);
    }

    private static VehicleModel ModelFor(string city, string company, IReadOnlyDictionary<string, VehicleModel> models) => city switch
    {
        "Ayacucho" => models[company == "Andes Express" ? "Bus 1 piso" : "Bus 2 pisos"],
        "Lima" => models["Toyota Avanza"],
        "Huanta" => models["Minivan"],
        "Andahuaylas" => models["Van"],
        "Cusco" => models["Toyota Yaris"],
        _ => throw new InvalidOperationException($"No vehicle model seed is configured for {city}.")
    };

    private static Guid Id(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(bytes.AsSpan(0, 16));
    }
}
