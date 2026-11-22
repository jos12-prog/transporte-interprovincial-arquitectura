using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure;

/// <summary>Registers persistence for the TransportScheduling module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddTransportSchedulingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<TransportSchedulingDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
