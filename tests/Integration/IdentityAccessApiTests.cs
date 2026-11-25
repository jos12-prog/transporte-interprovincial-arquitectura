using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Testcontainers.PostgreSql;
using Xunit;

namespace TransportPlatform.Integration.Tests;

[Collection(HostIntegrationCollection.Name)]
public sealed class IdentityAccessApiTests
{
    [Fact]
    public async Task RegistrationLoginProfileAgenciesAndBothRateLimitsUseTheSpecifiedContracts()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:16")
            .WithDatabase("identity_access").WithUsername("test").WithPassword("test-password").Build();
        await postgres.StartAsync();

        var values = new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgres.GetConnectionString(),
            ["IDENTITY_SEED_SUPER_ADMIN_PASSWORD"] = "CI-superadmin-password",
            ["IDENTITY_SEED_AGENCY_ADMIN_PASSWORD"] = "CI-agencyadmin-password",
            ["IDENTITY_SEED_AGENT_PASSWORD"] = "CI-agent-password"
        };
        using var factory = new IntegrationWebApplicationFactory(values);
            using var client = factory.CreateClient();

            using var swagger = await client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
            using var swaggerJson = JsonDocument.Parse(await swagger.Content.ReadAsStringAsync());
            Assert.True(swaggerJson.RootElement.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));

            var unique = Guid.NewGuid().ToString("N");
            var dni = (RandomNumberGenerator.GetInt32(10_000_000, 99_999_999)).ToString();
            var registration = new
            {
                fullName = "Pasajero de integración",
                dni,
                email = $"passenger-{unique}@example.test",
                phoneNumber = "999111222",
                password = "passenger-password"
            };
            using var invalid = await client.PostAsJsonAsync("/api/v1/auth/register", new { registration.fullName, registration.email, registration.phoneNumber, registration.password, dni = "123" });
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);

            using var registered = await client.PostAsJsonAsync("/api/v1/auth/register", registration);
            Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
            using var duplicate = await client.PostAsJsonAsync("/api/v1/auth/register", registration);
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

            using var anonymousMe = await client.GetAsync("/api/v1/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousMe.StatusCode);
            await AssertProblemDetailsAsync(anonymousMe, StatusCodes.Status401Unauthorized);
            using var passengerLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = registration.email, password = registration.password });
            Assert.Equal(HttpStatusCode.OK, passengerLogin.StatusCode);
            using var passengerLoginJson = JsonDocument.Parse(await passengerLogin.Content.ReadAsStringAsync());
            var passengerToken = passengerLoginJson.RootElement.GetProperty("accessToken").GetString()!;
            Assert.Equal("passenger", passengerLoginJson.RootElement.GetProperty("user").GetProperty("role").GetString());
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", passengerToken);
            using var me = await client.GetAsync("/api/v1/auth/me");
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
            using var meJson = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
            Assert.Equal("passenger", meJson.RootElement.GetProperty("role").GetString());
            using var passengerAgencies = await client.GetAsync("/api/v1/staff/agencies");
            Assert.Equal(HttpStatusCode.Forbidden, passengerAgencies.StatusCode);
            client.DefaultRequestHeaders.Authorization = null;

            using var agentLogin = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = "agente@andesexpress.test", password = values["IDENTITY_SEED_AGENT_PASSWORD"] });
            Assert.Equal(HttpStatusCode.OK, agentLogin.StatusCode);
            using var agentLoginJson = JsonDocument.Parse(await agentLogin.Content.ReadAsStringAsync());
            Assert.Equal("agent", agentLoginJson.RootElement.GetProperty("user").GetProperty("role").GetString());
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", agentLoginJson.RootElement.GetProperty("accessToken").GetString());
            using var agencyList = await client.GetAsync("/api/v1/staff/agencies");
            Assert.Equal(HttpStatusCode.OK, agencyList.StatusCode);
            using var agencyListJson = JsonDocument.Parse(await agencyList.Content.ReadAsStringAsync());
            var assignedAgencyId = agencyListJson.RootElement[0].GetProperty("agencyId").GetGuid();
            using var assignedAgency = await client.GetAsync($"/api/v1/staff/agencies/{assignedAgencyId}");
            Assert.Equal(HttpStatusCode.OK, assignedAgency.StatusCode);
            using var outsideAgency = await client.GetAsync($"/api/v1/staff/agencies/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.Forbidden, outsideAgency.StatusCode);
            await AssertProblemDetailsAsync(outsideAgency, StatusCodes.Status403Forbidden);
            client.DefaultRequestHeaders.Authorization = null;

            var account = $"locked-{unique}@example.test";
            for (var attempt = 1; attempt <= 5; attempt++)
            {
                using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = account, password = "wrong-password" });
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
            using var locked = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = account, password = "wrong-password" });
            Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
            Assert.Equal("application/problem+json", locked.Content.Headers.ContentType?.MediaType);
            Assert.True(locked.Headers.Contains("Retry-After"));

            for (var requestNumber = 9; requestNumber <= 20; requestNumber++)
            {
                using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = $"invalid-{requestNumber}-{unique}@example.test", password = "wrong-password" });
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
            using var ipLimited = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = $"over-ip-{unique}@example.test", password = "wrong-password" });
            Assert.Equal(HttpStatusCode.TooManyRequests, ipLimited.StatusCode);
    }

    private static async Task AssertProblemDetailsAsync(HttpResponseMessage response, int expectedStatus)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expectedStatus, problem.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("traceId").GetString()));
    }
}
