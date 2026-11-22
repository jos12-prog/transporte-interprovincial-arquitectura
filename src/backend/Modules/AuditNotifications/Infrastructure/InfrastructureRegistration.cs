using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.AuditNotifications.Infrastructure.Persistence;

namespace TransportPlatform.Modules.AuditNotifications.Infrastructure;

/// <summary>Registers persistence for the AuditNotifications module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddAuditNotificationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<AuditNotificationsDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
