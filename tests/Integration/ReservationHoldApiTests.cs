using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Shared.Contracts.Trips;
using Xunit;

namespace TransportPlatform.Integration.Tests;

[Collection(HostIntegrationCollection.Name)]
public sealed class ReservationHoldApiTests
{
    [Fact]
    public async Task PublicMapAndHoldEndpointsReturnExpectedContractsAndStatuses()
    {
        await using var environment = await ReservationTestEnvironment.StartAsync();
        var trip = await environment.FindSeedTripAsync();
        var mapResponse = await environment.Client.GetAsync($"/api/v1/trips/{trip.TripId}/seats");
        Assert.Equal(HttpStatusCode.OK, mapResponse.StatusCode);
        var mapJson = JsonDocument.Parse(await mapResponse.Content.ReadAsStringAsync());
        var map = mapJson.RootElement;
        Assert.Equal(trip.TripId, map.GetProperty("tripId").GetGuid());
        Assert.Equal(44, map.GetProperty("seats").GetArrayLength());
        Assert.DoesNotContain("passenger", mapJson.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        var seats = map.GetProperty("seats").EnumerateArray().Take(11).ToArray();
        var seatIds = seats.Take(10).Select(seat => seat.GetProperty("tripSeatId").GetGuid()).ToArray();

        using var holdResponse = await environment.Client.PostAsJsonAsync("/api/v1/reservations/holds", new
        {
            tripId = trip.TripId,
            tripSeatIds = seatIds
        });
        Assert.Equal(HttpStatusCode.OK, holdResponse.StatusCode);
        var hold = await holdResponse.Content.ReadFromJsonAsync<HoldResponse>();
        Assert.NotNull(hold);
        Assert.NotEmpty(hold.HoldToken);
        Assert.Equal(DateTimeOffset.UtcNow.AddMinutes(10).Date, hold.ExpiresAtUtc.Date);

        using var elevenSeatResponse = await environment.Client.PostAsJsonAsync("/api/v1/reservations/holds", new
        {
            tripId = trip.TripId,
            tripSeatIds = seats.Select(seat => seat.GetProperty("tripSeatId").GetGuid()).ToArray()
        });
        Assert.Equal(HttpStatusCode.BadRequest, elevenSeatResponse.StatusCode);
        Assert.Equal("application/problem+json", elevenSeatResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(1, await environment.CountActiveReservationsAsync());

        using var conflictedResponse = await environment.Client.PostAsJsonAsync("/api/v1/reservations/holds", new
        {
            tripId = trip.TripId,
            tripSeatIds = new[] { seatIds[0] }
        });
        Assert.Equal(HttpStatusCode.Conflict, conflictedResponse.StatusCode);

        using var wrongToken = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/reservations/holds/{hold.ReservationId}");
        wrongToken.Headers.Add("X-Hold-Token", "invalid-token");
        using var wrongTokenResponse = await environment.Client.SendAsync(wrongToken);
        Assert.Equal(HttpStatusCode.NotFound, wrongTokenResponse.StatusCode);

        using var release = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/reservations/holds/{hold.ReservationId}");
        release.Headers.Add("X-Hold-Token", hold.HoldToken);
        using var releasedResponse = await environment.Client.SendAsync(release);
        Assert.Equal(HttpStatusCode.NoContent, releasedResponse.StatusCode);
        Assert.Empty(await releasedResponse.Content.ReadAsByteArrayAsync());
        Assert.Equal(0, await environment.CountActiveReservationsAsync());

        using var missingMap = await environment.Client.GetAsync($"/api/v1/trips/{Guid.NewGuid()}/seats");
        Assert.Equal(HttpStatusCode.NotFound, missingMap.StatusCode);
        Assert.Contains("application/problem+json", missingMap.Content.Headers.ContentType?.MediaType);

        using var swaggerResponse = await environment.Client.GetAsync("/swagger/v1/swagger.json");
        using var swagger = JsonDocument.Parse(await swaggerResponse.Content.ReadAsStringAsync());
        var paths = swagger.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/trips/{tripId}/seats", out _));
        var deleteOperation = paths.GetProperty("/api/v1/reservations/holds/{id}").GetProperty("delete");
        Assert.True(deleteOperation.GetProperty("responses").TryGetProperty("204", out _));
    }

    [Fact]
    public async Task FiftyParallelHoldsForSameSeatProduceOneSuccessAndFortyNineConflicts()
    {
        await using var environment = await ReservationTestEnvironment.StartAsync();
        var trip = await environment.FindSeedTripAsync();
        var mapResponse = await environment.Client.GetAsync($"/api/v1/trips/{trip.TripId}/seats");
        mapResponse.EnsureSuccessStatusCode();
        var map = await mapResponse.Content.ReadFromJsonAsync<TripSeatMapResponse>();
        var availableSeatId = map!.Seats.First(seat => seat.Status == "Available").TripSeatId;

        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = Enumerable.Range(0, 50).Select(async _ =>
        {
            await barrier.Task;
            return await environment.Client.PostAsJsonAsync("/api/v1/reservations/holds", new
            {
                tripId = trip.TripId,
                tripSeatIds = new[] { availableSeatId }
            });
        }).ToArray();
        barrier.SetResult();
        var responses = await Task.WhenAll(requests);
        try
        {
            Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.OK));
            Assert.Equal(49, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
            Assert.Single(await environment.GetActiveReservationSeatCountsAsync());
            Assert.All(await environment.GetActiveReservationSeatCountsAsync(), count => Assert.Equal(1, count));
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    [Fact]
    public async Task ExpirationWorkerReleasesExpiredSeat()
    {
        await using var environment = await ReservationTestEnvironment.StartAsync();
        var trip = await environment.FindSeedTripAsync();
        var seatId = await environment.FindAvailableTripSeatAsync(trip.TripId);
        await environment.InsertExpiredHoldAsync(trip, seatId);
        await using var scope = environment.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<TransportPlatform.Modules.Reservations.Application.Contracts.IReservationRepository>();

        var expired = await repository.ExpireHoldsAsync(DateTimeOffset.UtcNow);

        Assert.Equal(1, expired);
        Assert.Equal(TripSeatStatus.Available, await environment.ReadSeatStatusAsync(seatId));
        Assert.Equal(ReservationStatus.Expired, await environment.ReadReservationStatusAsync());
    }

    private sealed record HoldResponse(Guid ReservationId, string HoldToken, DateTimeOffset ExpiresAtUtc);
    private sealed record TripSeatMapResponse(Guid TripId, SeatResponse[] Seats);
    private sealed record SeatResponse(Guid TripSeatId, string Status);
    private sealed record SeedTrip(Guid TripId, Guid CompanyId, Guid AgencyId);

    private sealed class ReservationTestEnvironment : IAsyncDisposable
    {
        private readonly PostgreSqlContainer _postgres;
        private readonly RedisContainer _redis;
        public IntegrationWebApplicationFactory Factory { get; }
        public HttpClient Client { get; }

        private ReservationTestEnvironment(PostgreSqlContainer postgres, RedisContainer redis,
            IntegrationWebApplicationFactory factory)
        {
            _postgres = postgres;
            _redis = redis;
            Factory = factory;
            Client = factory.CreateClient();
        }

        public static async Task<ReservationTestEnvironment> StartAsync()
        {
            var postgres = new PostgreSqlBuilder().WithImage("postgres:16").WithDatabase("reservation_api")
                .WithUsername("test").WithPassword("test-password").Build();
            var redis = new RedisBuilder().WithImage("redis:7").Build();
            await Task.WhenAll(postgres.StartAsync(), redis.StartAsync());
            var settings = new Dictionary<string, string?>
            {
                ["POSTGRES_CONNECTION_STRING"] = postgres.GetConnectionString(),
                ["POSTGRES_HOST"] = postgres.Hostname,
                ["POSTGRES_PORT"] = postgres.GetMappedPublicPort(5432).ToString(),
                ["REDIS_HOST"] = redis.Hostname,
                ["REDIS_PORT"] = redis.GetMappedPublicPort(6379).ToString(),
                ["RABBITMQ_HOST"] = "127.0.0.1",
                ["RABBITMQ_PORT"] = "1"
            };
            var factory = new IntegrationWebApplicationFactory(settings);
            _ = factory.Server;
            return new ReservationTestEnvironment(postgres, redis, factory);
        }

        public async Task<SeedTrip> FindSeedTripAsync()
        {
            var date = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).DateTime);
            var response = await Client.GetAsync($"/api/v1/trips/search?origin=Ayacucho&destination=Lima&date={date:yyyy-MM-dd}");
            response.EnsureSuccessStatusCode();
            var trips = await response.Content.ReadFromJsonAsync<ScheduledTripSummary[]>();
            Assert.NotEmpty(trips!);
            var trip = trips![0];
            return new SeedTrip(trip.TripId, trip.CompanyId, trip.AgencyId);
        }

        public async Task<int> CountActiveReservationsAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().Reservations
                .CountAsync(reservation => reservation.Status == ReservationStatus.Held);
        }

        public async Task<int[]> GetActiveReservationSeatCountsAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().ReservationSeats
                .Where(seat => seat.Status == ReservationStatus.Held)
                .GroupBy(seat => seat.TripSeatId).Select(group => group.Count()).ToArrayAsync();
        }

        public async Task<Guid> FindAvailableTripSeatAsync(Guid tripId)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().TripSeats
                .Where(seat => seat.TripId == tripId && seat.Status == TripSeatStatus.Available)
                .Select(seat => seat.Id).FirstAsync();
        }

        public async Task InsertExpiredHoldAsync(SeedTrip trip, Guid seatId)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
            var now = DateTimeOffset.UtcNow.AddMinutes(-11);
            var reservation = new Reservation(Guid.NewGuid(), trip.CompanyId, trip.AgencyId, trip.TripId,
                Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("test-token"))), [seatId], now);
            var seat = await db.TripSeats.SingleAsync(item => item.Id == seatId);
            seat.Hold();
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
        }

        public async Task<TripSeatStatus> ReadSeatStatusAsync(Guid id)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().TripSeats
                .Where(seat => seat.Id == id).Select(seat => seat.Status).SingleAsync();
        }

        public async Task<ReservationStatus> ReadReservationStatusAsync()
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().Reservations
                .OrderByDescending(item => item.CreatedAtUtc).Select(item => item.Status).FirstAsync();
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
            await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
        }
    }
}
