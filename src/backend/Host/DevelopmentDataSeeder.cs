using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.Reservations.Infrastructure;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
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

    // Tarifas planas aproximadas para viajes interprovinciales desde Ayacucho; se usan valores redondos como seed didáctico, no como tarifas comerciales.
    private static readonly (string City, TimeSpan Duration, decimal Fare)[] ProvincialDestinations =
    [
        ("Huanta", TimeSpan.FromHours(1), 10m),
        ("San Miguel", TimeSpan.FromHours(2.5), 20m),
        ("Cangallo", TimeSpan.FromHours(2), 15m),
        ("Vilcas Huamán", TimeSpan.FromHours(4), 25m),
        ("Puquio", TimeSpan.FromHours(5), 40m),
        ("Coracora", TimeSpan.FromHours(9), 60m),
        ("Huancapi", TimeSpan.FromHours(4), 25m),
        ("Querobamba", TimeSpan.FromHours(6), 40m)
    ];

    private sealed record RouteSeed(string Origin, string Destination, TimeSpan Duration, decimal Fare);
    private sealed record AgencySeed(string CompanyName, string City, string Name, string Code);
    private sealed record VehicleSeed(string ModelName, string DisplayName, string ModelCode, int Sequence);

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var companies = new[]
        {
            new Company(Id("company:andes-express"), "Andes Express", "20601234567"),
            new Company(Id("company:wari-bus"), "Wari Bus", "20587654321")
        };
        var agencySeeds = new[]
        {
            new AgencySeed("Andes Express", "Ayacucho", "Terminal Huamanga", "HMG"),
            new AgencySeed("Andes Express", "Huanta", "Agencia Huanta", "HUA"),
            new AgencySeed("Andes Express", "Puquio", "Agencia Puquio", "PUQ"),
            new AgencySeed("Wari Bus", "Ayacucho", "Terminal Huamanga", "HMG"),
            new AgencySeed("Wari Bus", "San Miguel", "Agencia San Miguel", "SMG"),
            new AgencySeed("Wari Bus", "Coracora", "Agencia Coracora", "COR")
        };
        var agencies = agencySeeds.Select(seed =>
        {
            var company = companies.Single(item => item.Name == seed.CompanyName);
            return new Agency(Id($"agency:{company.Id:N}:{seed.City}"), company.Id, seed.Name, seed.City);
        }).ToArray();
        await CompaniesAgenciesDevelopmentSeeder.SeedAsync(
            scope.ServiceProvider.GetRequiredService<CompaniesAgenciesDbContext>(), companies, agencies, cancellationToken);
        await DevelopmentIdentitySeeder.SeedAsync(scope.ServiceProvider, cancellationToken);

        var models = StandardVehicleModels.Create();
        var modelByName = models.ToDictionary(model => model.Name);
        var routeSeeds = CreateRouteSeeds();
        var routes = (from company in companies
                      from seed in routeSeeds
                      select new SchedulingRoute(Id($"route:{company.Id:N}:{seed.Origin}:{seed.Destination}"), company.Id, seed.Origin, seed.Destination))
            .ToArray();
        var serviceDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(LimaOffset).DateTime);
        var fares = (from company in companies
                     from seed in routeSeeds
                     let route = routes.Single(item => item.CompanyId == company.Id && item.OriginCity == seed.Origin && item.DestinationCity == seed.Destination)
                     select new Fare(Id($"fare:{company.Id:N}:{route.Id:N}:base"), company.Id, route.Id, FareType.Base,
                         serviceDate.AddDays(-30), null, seed.Fare))
            .ToArray();

        var vehicles = agencySeeds.SelectMany(seed =>
        {
            var company = companies.Single(item => item.Name == seed.CompanyName);
            var agency = agencies.Single(item => item.CompanyId == company.Id && item.City == seed.City);
            return CreateVehicleSeeds(seed.City).Select(vehicleSeed =>
            {
                var model = modelByName[vehicleSeed.ModelName];
                var platePrefix = company.Name == "Andes Express" ? "AEX" : "WAR";
                var plate = $"{platePrefix}-{seed.Code}-{vehicleSeed.ModelCode}{vehicleSeed.Sequence:D2}";
                return new Vehicle(Id($"vehicle:{company.Id:N}:{seed.City}:{vehicleSeed.ModelName}:{vehicleSeed.Sequence:D2}"),
                    company.Id, agency.Id, model.Id, plate, vehicleSeed.DisplayName);
            });
        }).ToArray();

        var trips = new List<Trip>();
        var tripSeats = new List<TripSeat>();
        foreach (var company in companies)
        {
            for (var day = 0; day < 7; day++)
            {
                var date = serviceDate.AddDays(day);
                var ayacuchoAgency = agencies.Single(item => item.CompanyId == company.Id && item.City == "Ayacucho");

                foreach (var destination in ProvincialDestinations)
                {
                    var route = routes.Single(item => item.CompanyId == company.Id && item.OriginCity == "Ayacucho" && item.DestinationCity == destination.City);
                    var modelAndSequence = OutboundVehicle(destination.City);
                    var departures = destination.City switch
                    {
                        "Huanta" => new[] { new TimeOnly(6, 0), new TimeOnly(9, 0), new TimeOnly(14, 0) },
                        "San Miguel" or "Coracora" => new[] { new TimeOnly(7, 0), new TimeOnly(16, 0) },
                        _ => new[] { new TimeOnly(8, 0) }
                    };
                    var vehicle = FindVehicle(vehicles, company.Id, ayacuchoAgency.Id, modelAndSequence.Model, modelAndSequence.Sequence);
                    foreach (var departureTime in departures)
                        AddTrip(company, ayacuchoAgency, route, vehicle, date, departureTime, destination.Duration);
                }

                var limaRoute = routes.Single(item => item.CompanyId == company.Id && item.OriginCity == "Ayacucho" && item.DestinationCity == "Lima");
                var firstBusModel = company.Name == "Andes Express" ? "Bus 1 piso" : "Bus 2 pisos";
                var secondBusModel = company.Name == "Andes Express" ? "Bus 2 pisos" : "Bus 1 piso";
                AddTrip(company, ayacuchoAgency, limaRoute,
                    FindVehicle(vehicles, company.Id, ayacuchoAgency.Id, firstBusModel, 1), date, new TimeOnly(6, 0), TimeSpan.FromHours(9));
                AddTrip(company, ayacuchoAgency, limaRoute,
                    FindVehicle(vehicles, company.Id, ayacuchoAgency.Id, secondBusModel, 1), date, new TimeOnly(20, 0), TimeSpan.FromHours(9));

                foreach (var agencySeed in agencySeeds.Where(item => item.CompanyName == company.Name && item.City != "Ayacucho"))
                {
                    var agency = agencies.Single(item => item.CompanyId == company.Id && item.City == agencySeed.City);
                    var route = routes.Single(item => item.CompanyId == company.Id && item.OriginCity == agencySeed.City && item.DestinationCity == "Ayacucho");
                    var vehicle = FindVehicle(vehicles, company.Id, agency.Id, "Toyota Yaris", 1);
                    var departures = agencySeed.City == "Huanta"
                        ? new[] { new TimeOnly(6, 0), new TimeOnly(9, 0), new TimeOnly(14, 0) }
                        : [new TimeOnly(8, 0)];
                    var duration = ProvincialDestinations.Single(item => item.City == agencySeed.City).Duration;
                    foreach (var departureTime in departures)
                        AddTrip(company, agency, route, vehicle, date, departureTime, duration);
                }
            }
        }

        await TransportSchedulingDevelopmentSeeder.SeedAsync(
            scope.ServiceProvider.GetRequiredService<TransportSchedulingDbContext>(), routes, fares, models, vehicles, trips, cancellationToken);
        await ReservationsDevelopmentSeeder.SeedAsync(
            scope.ServiceProvider.GetRequiredService<ReservationsDbContext>(), tripSeats, cancellationToken);

        void AddTrip(Company company, Agency agency, SchedulingRoute route, Vehicle vehicle, DateOnly date, TimeOnly time, TimeSpan duration)
        {
            var fare = Fare.ResolveFor(date, fares.Where(item => item.RouteId == route.Id))
                ?? throw new InvalidOperationException($"No Development base fare exists for route {route.Id}.");
            var model = modelByName.Values.Single(item => item.Id == vehicle.VehicleModelId);
            var departure = new DateTimeOffset(date.ToDateTime(time), LimaOffset);
            var tripId = Id($"trip:{company.Id:N}:{agency.Id:N}:{route.Id:N}:{date:yyyyMMdd}:{time:HHmm}");
            trips.Add(new Trip(tripId, company.Id, agency.Id, route.Id, vehicle.Id, departure,
                departure.Add(duration), fare.Price, route.OriginCity, route.DestinationCity,
                vehicle.DisplayName, model.Name, fare.Currency));
            tripSeats.AddRange(model.SeatLayout.Seats.Select(seat => new TripSeat(
                Id($"trip-seat:{tripId:N}:{seat.Number}"), company.Id, agency.Id, tripId, seat.Number,
                TripSeatStatus.Available, seat.Row, seat.Column, seat.Floor)));
        }
    }

    private static IReadOnlyList<RouteSeed> CreateRouteSeeds()
    {
        var routes = new List<RouteSeed>();
        foreach (var (city, duration, fare) in ProvincialDestinations)
        {
            routes.Add(new RouteSeed("Ayacucho", city, duration, fare));
            routes.Add(new RouteSeed(city, "Ayacucho", duration, fare));
        }
        routes.Add(new RouteSeed("Ayacucho", "Lima", TimeSpan.FromHours(9), 80m));
        return routes;
    }

    private static IReadOnlyList<VehicleSeed> CreateVehicleSeeds(string city)
    {
        var vehicles = new List<VehicleSeed>();
        var countPerCarModel = city == "Ayacucho" ? 2 : 1;
        AddCar("Toyota Yaris", "Toyota Yaris", "Y", countPerCarModel);
        AddCar("Toyota Avanza", "Toyota Avanza", "A", countPerCarModel);
        AddCar("Minivan", "Hyundai H-1", "H", countPerCarModel);
        AddCar("Van", "Toyota Hiace", "V", countPerCarModel);
        if (city == "Ayacucho")
        {
            vehicles.Add(new VehicleSeed("Bus 1 piso", "Bus Ayacucho 1 piso", "B1", 1));
            vehicles.Add(new VehicleSeed("Bus 2 pisos", "Bus Ayacucho 2 pisos", "B2", 1));
        }
        return vehicles;

        void AddCar(string model, string displayName, string code, int count)
        {
            for (var sequence = 1; sequence <= count; sequence++)
                vehicles.Add(new VehicleSeed(model, displayName, code, sequence));
        }
    }

    private static (string Model, int Sequence) OutboundVehicle(string destination) => destination switch
    {
        "Huanta" => ("Toyota Yaris", 1),
        "San Miguel" => ("Toyota Avanza", 1),
        "Cangallo" => ("Minivan", 1),
        "Vilcas Huamán" => ("Van", 1),
        "Puquio" => ("Toyota Yaris", 2),
        "Coracora" => ("Toyota Avanza", 2),
        "Huancapi" => ("Minivan", 2),
        "Querobamba" => ("Van", 2),
        _ => throw new InvalidOperationException($"No car seed is configured for {destination}.")
    };

    private static Vehicle FindVehicle(IReadOnlyCollection<Vehicle> vehicles, Guid companyId, Guid agencyId, string model, int sequence) =>
        vehicles.Single(vehicle => vehicle.CompanyId == companyId && vehicle.AgencyId == agencyId &&
            vehicle.DisplayName == DisplayNameForModel(model) && vehicle.Plate.EndsWith(ModelCode(model) + sequence.ToString("D2"), StringComparison.Ordinal));

    private static string DisplayNameForModel(string model) => model switch
    {
        "Minivan" => "Hyundai H-1",
        "Van" => "Toyota Hiace",
        "Bus 1 piso" => "Bus Ayacucho 1 piso",
        "Bus 2 pisos" => "Bus Ayacucho 2 pisos",
        _ => model
    };

    private static string ModelCode(string model) => model switch
    {
        "Toyota Yaris" => "Y",
        "Toyota Avanza" => "A",
        "Minivan" => "H",
        "Van" => "V",
        "Bus 1 piso" => "B1",
        "Bus 2 pisos" => "B2",
        _ => throw new InvalidOperationException($"No plate code is configured for {model}.")
    };

    private static Guid Id(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(bytes.AsSpan(0, 16));
    }
}
