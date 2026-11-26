using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Testcontainers.PostgreSql;
using TransportPlatform.Host;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;
using TransportPlatform.Modules.IdentityAccess.Infrastructure;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class DevelopmentSeedTests
{
    [Fact]
    public async Task ReseedingDoesNotChangeCompaniesOutsideTheDevelopmentSeed()
    {
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("companyseed")
            .WithUsername("test")
            .WithPassword("test-password")
            .Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<CompaniesAgenciesDbContext>()
            .UseNpgsql(postgres.GetConnectionString(), provider =>
                provider.MigrationsHistoryTable("__EFMigrationsHistory", "companies_agencies"))
            .Options;
        await using var db = new CompaniesAgenciesDbContext(options);
        await db.Database.MigrateAsync();

        var companies = new[]
        {
            new Company(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Andes Express", "20601234567"),
            new Company(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Wari Bus", "20587654321")
        };
        var agencies = new[]
        {
            new Agency(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), companies[0].Id, "Terminal Huamanga", "Ayacucho"),
            new Agency(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), companies[1].Id, "Terminal Wari", "Ayacucho")
        };
        await CompaniesAgenciesDevelopmentSeeder.SeedAsync(db, companies, agencies);

        var extraCompany = new Company(Guid.NewGuid(), "Operador externo", "20999888776", "Contacto original", "999123456");
        extraCompany.SetActive(false);
        db.Companies.Add(extraCompany);
        await db.SaveChangesAsync();

        await CompaniesAgenciesDevelopmentSeeder.SeedAsync(db, companies, agencies);

        var unchanged = await db.Companies.AsNoTracking().SingleAsync(company => company.Id == extraCompany.Id);
        Assert.Equal("Operador externo", unchanged.Name);
        Assert.Equal("20999888776", unchanged.TaxId);
        Assert.Equal("Contacto original", unchanged.ContactName);
        Assert.Equal("999123456", unchanged.ContactPhone);
        Assert.False(unchanged.IsActive);
        Assert.Equal(3, await db.Companies.CountAsync());
    }

    [Fact]
    public async Task DevelopmentSeedCreatesSevenDayCatalogAndIsIdempotent()
    {
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("tripseed")
            .WithUsername("test")
            .WithPassword("test-password")
            .Build();
        await postgres.StartAsync();

        var keyDirectory = Path.Combine(Path.GetTempPath(), $"development-seed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(keyDirectory);
        using var rsa = RSA.Create(2048);
        var privateKeyPath = Path.Combine(keyDirectory, "private.pem");
        var publicKeyPath = Path.Combine(keyDirectory, "public.pem");
        await File.WriteAllTextAsync(privateKeyPath, rsa.ExportRSAPrivateKeyPem());
        await File.WriteAllTextAsync(publicKeyPath, rsa.ExportRSAPublicKeyPem());

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgres.GetConnectionString(),
            ["JWT_ISSUER"] = "development-seed-tests",
            ["JWT_SIGNING_PRIVATE_KEY_PATH"] = privateKeyPath,
            ["JWT_SIGNING_PUBLIC_KEY_PATH"] = publicKeyPath,
            ["LOGIN_ATTEMPT_HMAC_KEY"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            ["IDENTITY_SEED_SUPER_ADMIN_PASSWORD"] = "CI-superadmin-password",
            ["IDENTITY_SEED_AGENCY_ADMIN_PASSWORD"] = "CI-agencyadmin-password",
            ["IDENTITY_SEED_AGENT_PASSWORD"] = "CI-agent-password"
        }).Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCompaniesAgenciesInfrastructure(configuration);
        services.AddIdentityAccessInfrastructure(configuration);
        services.AddDbContext<TransportSchedulingDbContext>(options => options.UseNpgsql(postgres.GetConnectionString(),
            provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "transport_scheduling")));
        services.AddDbContext<ReservationsDbContext>(options => options.UseNpgsql(postgres.GetConnectionString(),
            provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "reservations")));
        services.AddDbContext<IdentityAccessDbContext>(options => options.UseNpgsql(postgres.GetConnectionString(),
            provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "identity_access")));
        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<CompaniesAgenciesDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<TransportSchedulingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().Database.MigrateAsync();
        }

        await DevelopmentDataSeeder.SeedAsync(provider);
        await DevelopmentDataSeeder.SeedAsync(provider);

        await using var verifyScope = provider.CreateAsyncScope();
        var companies = verifyScope.ServiceProvider.GetRequiredService<CompaniesAgenciesDbContext>();
        var scheduling = verifyScope.ServiceProvider.GetRequiredService<TransportSchedulingDbContext>();
        var reservations = verifyScope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        Assert.Equal(2, await companies.Companies.CountAsync());
        Assert.Equal(6, await companies.Agencies.CountAsync());
        Assert.Equal(34, await scheduling.Routes.CountAsync());
        Assert.Equal(34, await scheduling.Fares.CountAsync());
        var routesByCompany = await scheduling.Routes.GroupBy(route => route.CompanyId)
            .Select(group => new { CompanyId = group.Key, Count = group.Count() }).ToArrayAsync();
        Assert.Equal(2, routesByCompany.Length);
        Assert.All(routesByCompany, company => Assert.Equal(17, company.Count));
        var faresByCompany = await scheduling.Fares.GroupBy(fare => fare.CompanyId)
            .Select(group => new { CompanyId = group.Key, Count = group.Count() }).ToArrayAsync();
        Assert.Equal(2, faresByCompany.Length);
        Assert.All(faresByCompany, company => Assert.Equal(17, company.Count));
        Assert.Equal(6, await scheduling.VehicleModels.CountAsync());
        Assert.Equal(36, await scheduling.Vehicles.CountAsync());
        Assert.Equal(238, await scheduling.Trips.CountAsync());
        var expectedSeatCount = await (from trip in scheduling.Trips
                                       join vehicle in scheduling.Vehicles on trip.VehicleId equals vehicle.Id
                                       join model in scheduling.VehicleModels on vehicle.VehicleModelId equals model.Id
                                       select model.Capacity).SumAsync();
        Assert.Equal(expectedSeatCount, await reservations.TripSeats.CountAsync());
        Assert.Equal(new[] { "Ayacucho", "Coracora", "Huanta", "Puquio", "San Miguel" },
            await companies.Agencies.Select(agency => agency.City).Distinct().OrderBy(city => city).ToArrayAsync());
        var seededRouteNames = await scheduling.Routes.Select(route => route.OriginCity + "|" + route.DestinationCity).ToArrayAsync();
        Assert.DoesNotContain(seededRouteNames, route => route.Contains("Cusco", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(seededRouteNames, route => route.Contains("Andahuaylas", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(seededRouteNames, route => route.Contains("Huancayo", StringComparison.OrdinalIgnoreCase));
        var identity = verifyScope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        Assert.Equal(3, await identity.Users.CountAsync());
        Assert.Equal(4, await identity.UserAgencyAssignments.CountAsync());

        var tripDates = await scheduling.Trips.Select(trip => trip.DepartureAtUtc).ToArrayAsync();
        var limaToday = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).DateTime);
        Assert.Equal(7, tripDates.Select(date => DateOnly.FromDateTime(date.UtcDateTime.AddHours(-5))).Distinct().Count());
        Assert.All(tripDates, date => Assert.InRange(
            DateOnly.FromDateTime(date.UtcDateTime.AddHours(-5)).DayNumber - limaToday.DayNumber, 0, 6));

        var capacitiesByVehicle = await (from vehicle in scheduling.Vehicles
                                         join model in scheduling.VehicleModels on vehicle.VehicleModelId equals model.Id
                                         select new { vehicle.Id, model.Capacity })
            .ToDictionaryAsync(item => item.Id, item => item.Capacity);
        var trips = await scheduling.Trips.Select(trip => new { trip.Id, trip.VehicleId }).ToArrayAsync();
        var seatCounts = await reservations.TripSeats
            .GroupBy(seat => seat.TripId)
            .Select(groupSeats => new { TripId = groupSeats.Key, Count = groupSeats.Count() })
            .ToDictionaryAsync(item => item.TripId, item => item.Count);
        Assert.Equal(238, trips.Length);
        Assert.Equal(238, seatCounts.Count);
        Assert.All(trips, trip => Assert.Equal(capacitiesByVehicle[trip.VehicleId], seatCounts[trip.Id]));
        Directory.Delete(keyDirectory, recursive: true);
    }
}
