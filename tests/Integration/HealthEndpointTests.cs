using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class HealthEndpointTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("health")
        .WithUsername("test")
        .WithPassword("test-password")
        .Build();
    private readonly RedisContainer _redis = new RedisBuilder().WithImage("redis:7").Build();
    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder().WithImage("rabbitmq:3-management").Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _rabbitMq.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask(), _rabbitMq.DisposeAsync().AsTask());
    }

    [Fact]
    public async Task HealthIsHealthyWhenAllContainersAreReachable()
    {
        var redisPort = _redis.GetMappedPublicPort(6379);
        using var environment = OverrideContainerEnvironment(redisPort);
        using var factory = CreateFactory(redisPort);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            (int)response.StatusCode == StatusCodes.Status200OK,
            $"Expected HTTP 200 but received {(int)response.StatusCode}. Health payload: {body}");
        Assert.Contains("\"postgresql\":\"healthy\"", body);
        Assert.Contains("\"redis\":\"healthy\"", body);
        Assert.Contains("\"rabbitmq\":\"healthy\"", body);
        Assert.DoesNotContain("test-password", body);
        Assert.DoesNotContain("Host=", body);
    }

    [Fact]
    public async Task HealthIsUnavailableWhenOneContainerEndpointCannotBeReached()
    {
        using var environment = OverrideContainerEnvironment(redisPort: 1);
        using var factory = CreateFactory(redisPort: 1);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, (int)response.StatusCode);
        Assert.Contains("\"redis\":\"unhealthy\"", body);
        Assert.DoesNotContain("test-password", body);
        Assert.DoesNotContain("Host=", body);
    }

    private WebApplicationFactory<Program> CreateFactory(int redisPort)
    {
        var values = GetContainerEnvironment(redisPort);
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(values)));
    }

    private IDisposable OverrideContainerEnvironment(int redisPort) => new EnvironmentScope(GetContainerEnvironment(redisPort));

    private Dictionary<string, string?> GetContainerEnvironment(int redisPort) => new()
    {
        ["POSTGRES_HOST"] = _postgres.Hostname,
        ["POSTGRES_PORT"] = _postgres.GetMappedPublicPort(5432).ToString(),
        ["POSTGRES_CONNECTION_STRING"] = _postgres.GetConnectionString(),
        ["REDIS_HOST"] = _redis.Hostname,
        ["REDIS_PORT"] = redisPort.ToString(),
        ["RABBITMQ_HOST"] = _rabbitMq.Hostname,
        ["RABBITMQ_PORT"] = _rabbitMq.GetMappedPublicPort(5672).ToString(),
    };

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _previousValues;

        public EnvironmentScope(IReadOnlyDictionary<string, string?> values)
        {
            _previousValues = values.Keys.ToDictionary(
                key => key,
                key => Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process));

            foreach (var (key, value) in values)
            {
                Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
            }
        }

        public void Dispose()
        {
            foreach (var (key, value) in _previousValues)
            {
                Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
            }
        }
    }
}
