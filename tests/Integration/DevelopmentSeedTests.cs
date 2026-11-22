using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using TransportPlatform.Host;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class DevelopmentSeedTests
{
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

        var services = new ServiceCollection();
        services.AddDbContext<CompaniesAgenciesDbContext>(options => options.UseNpgsql(postgres.GetConnectionString(),
            provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "companies_agencies")));
        services.AddDbContext<TransportSchedulingDbContext>(options => options.UseNpgsql(postgres.GetConnectionString(),
            provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "transport_scheduling")));
        services.AddDbContext<ReservationsDbContext>(options => options.UseNpgsql(postgres.GetConnectionString(),
            provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "reservations")));
        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<CompaniesAgenciesDbContext>().Database.MigrateAsync();
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
        Assert.Equal(10, await companies.Agencies.CountAsync());
        Assert.Equal(5, await scheduling.Routes.CountAsync());
        Assert.Equal(6, await scheduling.VehicleModels.CountAsync());
        Assert.Equal(10, await scheduling.Vehicles.CountAsync());
        Assert.Equal(70, await scheduling.Trips.CountAsync());
        Assert.Equal(1_120, await reservations.TripSeats.CountAsync());
        Assert.Equal(new[] { "Andahuaylas", "Ayacucho", "Cusco", "Huanta", "Lima" },
            await companies.Agencies.Select(agency => agency.City).Distinct().OrderBy(city => city).ToArrayAsync());

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
        Assert.Equal(70, trips.Length);
        Assert.Equal(70, seatCounts.Count);
        Assert.All(trips, trip => Assert.Equal(capacitiesByVehicle[trip.VehicleId], seatCounts[trip.Id]));
    }
}
