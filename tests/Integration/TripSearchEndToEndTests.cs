using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using TransportPlatform.Modules.QueriesReports.Application.UseCases;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

[Collection(HostIntegrationCollection.Name)]
public sealed class TripSearchEndToEndTests
{
    [Fact]
    public async Task SeededSearchSupportsFiltersCacheAndConcurrentReadOnlyRequests()
    {
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("tripsearch_e2e")
            .WithUsername("test")
            .WithPassword("test-password")
            .Build();
        await using var redis = new RedisBuilder().WithImage("redis:7").Build();
        await Task.WhenAll(postgres.StartAsync(), redis.StartAsync());

        var configuration = new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgres.GetConnectionString(),
            ["POSTGRES_HOST"] = postgres.Hostname,
            ["POSTGRES_PORT"] = postgres.GetMappedPublicPort(5432).ToString(),
            ["REDIS_HOST"] = redis.Hostname,
            ["REDIS_PORT"] = redis.GetMappedPublicPort(6379).ToString(),
            ["RABBITMQ_HOST"] = "127.0.0.1",
            ["RABBITMQ_PORT"] = "1"
        };
        using var factory = new IntegrationWebApplicationFactory(configuration);
        using var client = factory.CreateClient();

        var serviceDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).DateTime);
        var basePath = $"/api/v1/trips/search?origin=Ayacucho&destination=Lima&date={serviceDate:yyyy-MM-dd}";
        using var missResponse = await client.GetAsync(basePath);
        Assert.Equal(200, (int)missResponse.StatusCode);
        Assert.Equal("MISS", Assert.Single(missResponse.Headers.GetValues("X-Cache")));
        var initialTrips = await missResponse.Content.ReadFromJsonAsync<TripSearchResult[]>();
        Assert.NotNull(initialTrips);
        Assert.Equal(2, initialTrips.Length);
        Assert.Equal(new[] { 44, 52 }, initialTrips.Select(trip => trip.AvailableSeats).OrderBy(count => count).ToArray());
        Assert.Contains(initialTrips, trip => trip.CompanyName == "Andes Express" && trip.AgencyCity == "Ayacucho");
        Assert.Contains(initialTrips, trip => trip.CompanyName == "Wari Bus" && trip.VehicleModelName == "Bus 2 pisos");

        using var hitResponse = await client.GetAsync(basePath);
        Assert.Equal(200, (int)hitResponse.StatusCode);
        Assert.Equal("HIT", Assert.Single(hitResponse.Headers.GetValues("X-Cache")));

        var andesTrip = Assert.Single(initialTrips, trip => trip.CompanyName == "Andes Express");
        var filteredPath = $"{basePath}&companyId={andesTrip.CompanyId}&agencyId={andesTrip.AgencyId}&departureFrom=08%3A00&departureTo=08%3A00&minPrice=75&maxPrice=75";
        using var filteredResponse = await client.GetAsync(filteredPath);
        Assert.Equal(200, (int)filteredResponse.StatusCode);
        var filteredTrips = await filteredResponse.Content.ReadFromJsonAsync<TripSearchResult[]>();
        var filteredTrip = Assert.Single(filteredTrips!);
        Assert.Equal("Andes Express", filteredTrip.CompanyName);
        Assert.Equal(75m, filteredTrip.Price);

        var concurrentPath = $"{basePath}&companyId={andesTrip.CompanyId}&minPrice=74&maxPrice=76";
        var tripIds = initialTrips.Select(trip => trip.TripId).ToArray();
        var seatsBefore = await ReadSeatStateAsync(factory.Services, tripIds);
        var concurrentResponses = await Task.WhenAll(Enumerable.Range(0, 16)
            .Select(_ => client.GetAsync(concurrentPath)));
        try
        {
            Assert.All(concurrentResponses, response => Assert.Equal(200, (int)response.StatusCode));
            foreach (var response in concurrentResponses)
            {
                var results = await response.Content.ReadFromJsonAsync<TripSearchResult[]>();
                Assert.Single(results!);
                Assert.Equal(44, results![0].AvailableSeats);
            }
        }
        finally
        {
            foreach (var response in concurrentResponses)
            {
                response.Dispose();
            }
        }

        var seatsAfter = await ReadSeatStateAsync(factory.Services, tripIds);
        Assert.Equal(seatsBefore, seatsAfter);
    }

    private static async Task<SeatSnapshot[]> ReadSeatStateAsync(IServiceProvider services, IReadOnlyCollection<Guid> tripIds)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().TripSeats
            .AsNoTracking()
            .Where(seat => tripIds.Contains(seat.TripId))
            .OrderBy(seat => seat.TripId)
            .ThenBy(seat => seat.SeatNumber)
            .Select(seat => new SeatSnapshot(seat.TripId, seat.SeatNumber, seat.Status))
            .ToArrayAsync();
    }

    private sealed record SeatSnapshot(Guid TripId, int SeatNumber, TripSeatStatus Status);

}
