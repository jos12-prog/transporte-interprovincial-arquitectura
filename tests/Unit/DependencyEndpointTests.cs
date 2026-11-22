using Microsoft.Extensions.Configuration;
using TransportPlatform.Shared.Infrastructure.Health;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class DependencyEndpointTests
{
    [Fact]
    public void UsesDefaultsWhenEnvironmentConfigurationIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var endpoint = DependencyEndpoint.FromConfiguration(
            configuration, "REDIS_HOST", "REDIS_PORT", "redis", 6379);

        Assert.Equal(new DependencyEndpoint("redis", 6379), endpoint);
    }

    [Fact]
    public void UsesConfiguredHostAndValidPort()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RABBITMQ_HOST"] = "localhost",
                ["RABBITMQ_PORT"] = "5673",
            })
            .Build();

        var endpoint = DependencyEndpoint.FromConfiguration(
            configuration, "RABBITMQ_HOST", "RABBITMQ_PORT", "rabbitmq", 5672);

        Assert.Equal(new DependencyEndpoint("localhost", 5673), endpoint);
    }

    [Theory]
    [InlineData("not-a-port")]
    [InlineData("0")]
    [InlineData("65536")]
    public void UsesDefaultPortWhenConfiguredPortIsInvalid(string configuredPort)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["POSTGRES_PORT"] = configuredPort })
            .Build();

        var endpoint = DependencyEndpoint.FromConfiguration(
            configuration, "POSTGRES_HOST", "POSTGRES_PORT", "postgres", 5432);

        Assert.Equal(new DependencyEndpoint("postgres", 5432), endpoint);
    }
}
