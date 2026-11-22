using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TransportPlatform.Shared.Infrastructure.Health;

namespace TransportPlatform.Shared.Infrastructure;

/// <summary>Registers bounded connectivity checks for required infrastructure services.</summary>
public static class DependencyHealthRegistration
{
    public static IServiceCollection AddDependencyHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("postgresql", CreateCheck(configuration, "POSTGRES_HOST", "POSTGRES_PORT", "postgres", 5432), HealthStatus.Unhealthy, timeout: TimeSpan.FromSeconds(3))
            .AddCheck("redis", CreateCheck(configuration, "REDIS_HOST", "REDIS_PORT", "redis", 6379), HealthStatus.Unhealthy, timeout: TimeSpan.FromSeconds(3))
            .AddCheck("rabbitmq", CreateCheck(configuration, "RABBITMQ_HOST", "RABBITMQ_PORT", "rabbitmq", 5672), HealthStatus.Unhealthy, timeout: TimeSpan.FromSeconds(3));
        return services;
    }

    private static TcpDependencyHealthCheck CreateCheck(IConfiguration configuration, string hostKey, string portKey, string defaultHost, int defaultPort)
    {
        var endpoint = DependencyEndpoint.FromConfiguration(configuration, hostKey, portKey, defaultHost, defaultPort);
        return new TcpDependencyHealthCheck(endpoint.Host, endpoint.Port);
    }
}
