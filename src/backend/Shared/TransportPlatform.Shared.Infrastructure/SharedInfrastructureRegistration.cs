using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TransportPlatform.Shared.Infrastructure;

/// <summary>Registers cross-cutting infrastructure for the host.</summary>
public static class SharedInfrastructureRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddDependencyHealthChecks(configuration);
        return services;
    }

    public static ConfigureHostBuilder UsePlatformSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, logger) => logger
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());
        return host;
    }
}
