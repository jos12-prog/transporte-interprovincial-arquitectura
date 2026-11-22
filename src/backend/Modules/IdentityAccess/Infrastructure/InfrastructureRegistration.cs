using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure;

/// <summary>Registers persistence for the IdentityAccess module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddIdentityAccessInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<IdentityAccessDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
