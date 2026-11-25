using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace TransportPlatform.Integration.Tests;

[Collection(HostIntegrationCollection.Name)]
public sealed class PlatformAdministrationApiTests
{
    [Fact]
    public async Task SuperAdminCanManagePlatformCatalogAndInactiveCompanyIsExcludedFromSearch()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:16")
            .WithDatabase("platform_admin").WithUsername("test").WithPassword("test-password").Build();
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

        using var anonymous = await client.GetAsync("/api/v1/admin/companies");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("application/problem+json", anonymous.Content.Headers.ContentType?.MediaType);

        var agentLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            identifier = "agente@andesexpress.test",
            password = "Integration-agent-password"
        });
        var agentToken = await agentLogin.Content.ReadFromJsonAsync<AuthResponse>();
        using var forbidden = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/companies");
        forbidden.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agentToken!.AccessToken);
        using var forbiddenResponse = await client.SendAsync(forbidden);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal("application/problem+json", forbiddenResponse.Content.Headers.ContentType?.MediaType);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            identifier = "superadmin@rutaandina.test",
            password = "Integration-superadmin-password"
        });
        var superAdmin = await login.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", superAdmin!.AccessToken);

        var companies = await client.GetFromJsonAsync<CompanyDto[]>("/api/v1/admin/companies");
        Assert.NotEmpty(companies!);
        var newTaxId = $"29{Random.Shared.Next(100_000_000, 999_999_999)}";
        using var invalidCompany = await client.PostAsJsonAsync("/api/v1/admin/companies", new
        {
            name = "", taxId = "bad", contactName = (string?)null, contactPhone = (string?)null
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidCompany.StatusCode);
        Assert.Equal("application/problem+json", invalidCompany.Content.Headers.ContentType?.MediaType);

        using var createCompany = await client.PostAsJsonAsync("/api/v1/admin/companies", new
        {
            name = "Empresa de prueba", taxId = newTaxId, contactName = "Contacto", contactPhone = "999000111"
        });
        Assert.Equal(HttpStatusCode.Created, createCompany.StatusCode);
        var company = await createCompany.Content.ReadFromJsonAsync<CompanyDto>();
        using var duplicateCompany = await client.PostAsJsonAsync("/api/v1/admin/companies", new
        {
            name = "Duplicada", taxId = newTaxId, contactName = (string?)null, contactPhone = (string?)null
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicateCompany.StatusCode);

        using var createAgency = await client.PostAsJsonAsync($"/api/v1/admin/companies/{company!.CompanyId}/agencies", new
        {
            name = "Agencia de prueba", city = "Huanta", address = "Centro", phone = (string?)null
        });
        Assert.Equal(HttpStatusCode.Created, createAgency.StatusCode);
        var agency = await createAgency.Content.ReadFromJsonAsync<AgencyDto>();
        using var disableAgency = await client.PatchAsJsonAsync($"/api/v1/admin/agencies/{agency!.AgencyId}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.OK, disableAgency.StatusCode);
        Assert.False((await disableAgency.Content.ReadFromJsonAsync<AgencyDto>())!.IsActive);

        var models = await client.GetFromJsonAsync<ModelDto[]>("/api/v1/admin/vehicle-models");
        var standardModels = models!;
        Assert.Equal(6, standardModels.Count(model => model.IsInUse));
        Assert.Equal(new[] { "Bus 1 piso", "Bus 2 pisos", "Minivan", "Toyota Avanza", "Toyota Yaris", "Van" },
            standardModels.Select(model => model.Name).OrderBy(name => name));
        Assert.All(standardModels, model => Assert.All(model.Seats, seat =>
        {
            Assert.NotNull(seat.Row);
            Assert.NotNull(seat.Column);
        }));
        var usedModel = Assert.Single(standardModels, model => model.Name == "Bus 1 piso");
        var changedLayout = Enumerable.Range(1, usedModel.Capacity).Select(number => new
        {
            number, floor = 1, row = 100 + (number - 1) / 4, column = (number - 1) % 4 + 1
        }).ToArray();
        using var frozenLayout = await client.PutAsJsonAsync($"/api/v1/admin/vehicle-models/{usedModel.VehicleModelId}", new
        {
            name = usedModel.Name, category = usedModel.Category, seats = changedLayout
        });
        Assert.Equal(HttpStatusCode.Conflict, frozenLayout.StatusCode);
        Assert.Equal("application/problem+json", frozenLayout.Content.Headers.ContentType?.MediaType);
        using var invalidModel = await client.PostAsJsonAsync("/api/v1/admin/vehicle-models", new
        {
            name = "Modelo inválido", category = "Familiar", seats = Array.Empty<object>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidModel.StatusCode);

        var serviceDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)).DateTime);
        var searchPath = $"/api/v1/trips/search?origin=Ayacucho&destination=Lima&date={serviceDate:yyyy-MM-dd}";
        using var warmCache = await client.GetAsync(searchPath);
        Assert.Equal(HttpStatusCode.OK, warmCache.StatusCode);
        var search = await warmCache.Content.ReadFromJsonAsync<SearchTripDto[]>();
        Assert.NotEmpty(search!);
        var affectedCompany = search![0].CompanyId;
        using var deactivateCompany = await client.PatchAsJsonAsync($"/api/v1/admin/companies/{affectedCompany}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.OK, deactivateCompany.StatusCode);
        using var afterDeactivation = await client.GetAsync(searchPath);
        Assert.Equal("MISS", Assert.Single(afterDeactivation.Headers.GetValues("X-Cache")));
        var remaining = await afterDeactivation.Content.ReadFromJsonAsync<SearchTripDto[]>();
        Assert.DoesNotContain(remaining!, trip => trip.CompanyId == affectedCompany);
        using var reactivateCompany = await client.PatchAsJsonAsync($"/api/v1/admin/companies/{affectedCompany}/status", new { isActive = true });
        Assert.Equal(HttpStatusCode.OK, reactivateCompany.StatusCode);

        using var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, swaggerResponse.StatusCode);
        using var swagger = JsonDocument.Parse(await swaggerResponse.Content.ReadAsStringAsync());
        Assert.True(swagger.RootElement.GetProperty("paths").TryGetProperty("/api/v1/admin/companies", out _));
        Assert.True(swagger.RootElement.GetProperty("paths").TryGetProperty("/api/v1/admin/vehicle-models", out _));
    }

    private sealed record AuthResponse(string AccessToken);
    private sealed record CompanyDto(Guid CompanyId, bool IsActive);
    private sealed record AgencyDto(Guid AgencyId, bool IsActive);
    private sealed record ModelDto(Guid VehicleModelId, string Name, string? Category, int Capacity, IReadOnlyList<SeatDto> Seats, bool IsInUse);
    private sealed record SeatDto(int Number, int Floor, int? Row, int? Column);
    private sealed record SearchTripDto(Guid CompanyId, Guid AgencyId);
}
