using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

namespace TransportPlatform.Modules.Reservations.Infrastructure;

/// <summary>Registers persistence for the Reservations module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddReservationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<ReservationsDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
