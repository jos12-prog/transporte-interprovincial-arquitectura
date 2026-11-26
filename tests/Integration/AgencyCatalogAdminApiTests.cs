using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

[Collection(HostIntegrationCollection.Name)]
public sealed class AgencyCatalogAdminApiTests
{
    [Fact]
    public async Task AgencyAdminCanScheduleWithFareAndCancelWhileAgentIsForbidden()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:16").WithDatabase("agency_catalog")
            .WithUsername("test").WithPassword("test-password").Build();
        await using var redis = new RedisBuilder().WithImage("redis:7").Build();
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
        using var factory = new IntegrationWebApplicationFactory(settings);
        using var client = factory.CreateClient();
        using var anonymous = await client.GetAsync($"/api/v1/admin/agencies/{Guid.NewGuid()}/routes");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("application/problem+json", anonymous.Content.Headers.ContentType?.MediaType);

        var adminLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = "admin.agencia@andesexpress.test", password = "Integration-agencyadmin-password" });
        var admin = await adminLogin.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin!.AccessToken);
        var agencies = await client.GetFromJsonAsync<AgencyDto[]>("/api/v1/staff/agencies");
        var agencyId = agencies!.First().AgencyId;

        var agentLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = "agente@andesexpress.test", password = "Integration-agent-password" });
        var agent = await agentLogin.Content.ReadFromJsonAsync<AuthResponse>();
        using var agentRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/admin/agencies/{agencyId}/routes");
        agentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agent!.AccessToken);
        using var agentResponse = await client.SendAsync(agentRequest);
        Assert.Equal(HttpStatusCode.Forbidden, agentResponse.StatusCode);
        Assert.Equal("application/problem+json", agentResponse.Content.Headers.ContentType?.MediaType);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);
        using var outOfScope = await client.GetAsync($"/api/v1/admin/agencies/{Guid.NewGuid()}/routes");
        Assert.Equal(HttpStatusCode.Forbidden, outOfScope.StatusCode);

        var prefix = $"[Prueba] {Guid.NewGuid():N}";
        using var routeResponse = await client.PostAsJsonAsync($"/api/v1/admin/agencies/{agencyId}/routes", new { originCity = prefix + " Origen", destinationCity = prefix + " Destino" });
        Assert.Equal(HttpStatusCode.Created, routeResponse.StatusCode);
        var route = await routeResponse.Content.ReadFromJsonAsync<RouteDto>();
        Assert.NotNull(route);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).DateTime);
        using var fareResponse = await client.PostAsJsonAsync($"/api/v1/admin/agencies/{agencyId}/routes/{route!.RouteId}/fares", new { type = "Base", validFrom = today.AddDays(-1), validTo = (DateOnly?)null, price = 80m, currency = "PEN" });
        Assert.Equal(HttpStatusCode.Created, fareResponse.StatusCode);

        var models = await client.GetFromJsonAsync<ModelDto[]>($"/api/v1/admin/agencies/{agencyId}/vehicle-models");
        var model = models!.First();
        using var vehicleResponse = await client.PostAsJsonAsync($"/api/v1/admin/agencies/{agencyId}/vehicles", new { plate = $"TST-{Guid.NewGuid():N}"[..12].ToUpperInvariant(), displayName = "[Prueba] Toyota Avanza", vehicleModelId = model.VehicleModelId });
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        var vehicle = await vehicleResponse.Content.ReadFromJsonAsync<VehicleDto>();

        var departure = DateTimeOffset.UtcNow.AddDays(2).AddHours(1);
        var scheduleBody = new { routeId = route.RouteId, vehicleId = vehicle!.VehicleId, departureAtUtc = departure, arrivalAtUtc = departure.AddHours(9) };
        using var scheduleResponses = new HttpResponseMessageList(await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => client.PostAsJsonAsync($"/api/v1/admin/agencies/{agencyId}/trips", scheduleBody))));
        Assert.Single(scheduleResponses.Items, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Equal(19, scheduleResponses.Items.Count(response => response.StatusCode == HttpStatusCode.Conflict));
        var tripResponse = scheduleResponses.Items.Single(response => response.StatusCode == HttpStatusCode.Created);
        var trip = await tripResponse.Content.ReadFromJsonAsync<TripDto>();
        Assert.Equal(80m, trip!.Price);
        Assert.All(scheduleResponses.Items.Where(response => response.StatusCode == HttpStatusCode.Conflict), response =>
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType));
        var map = await client.GetFromJsonAsync<SeatMapDto>($"/api/v1/trips/{trip.TripId}/seats");
        var seatId = map!.Seats[0].TripSeatId;
        using var holdResponse = await client.PostAsJsonAsync("/api/v1/reservations/holds", new { tripId = trip.TripId, tripSeatIds = new[] { seatId } });
        Assert.Equal(HttpStatusCode.OK, holdResponse.StatusCode);
        using var cancelResponse = await client.PostAsync($"/api/v1/admin/agencies/{agencyId}/trips/{trip.TripId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        Assert.Equal("Cancelled", (await cancelResponse.Content.ReadFromJsonAsync<TripDto>())!.Status);
        await using var scope = factory.Services.CreateAsyncScope();
        var reservations = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        Assert.Equal(TripSeatStatus.Available, await reservations.TripSeats.Where(seat => seat.Id == seatId).Select(seat => seat.Status).SingleAsync());
        Assert.Equal(ReservationStatus.Cancelled, await reservations.Reservations.Where(item => item.TripId == trip.TripId).Select(item => item.Status).SingleAsync());
    }

    private sealed record AuthResponse(string AccessToken);
    private sealed record AgencyDto(Guid AgencyId, string AgencyName, string City);
    private sealed record RouteDto(Guid RouteId, bool IsActive);
    private sealed record ModelDto(Guid VehicleModelId, string Name);
    private sealed record VehicleDto(Guid VehicleId);
    private sealed record TripDto(Guid TripId, decimal Price, string Status);
    private sealed record SeatMapDto(Guid TripId, SeatDto[] Seats);
    private sealed record SeatDto(Guid TripSeatId, int Number, string Status);
    private sealed class HttpResponseMessageList(IReadOnlyList<HttpResponseMessage> items) : IDisposable
    {
        public IReadOnlyList<HttpResponseMessage> Items { get; } = items;
        public void Dispose() { foreach (var item in Items) item.Dispose(); }
    }
}
