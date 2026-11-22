using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Application.Services;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.Reservations.Infrastructure;

/// <summary>Registers persistence for the Reservations module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddReservationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<ReservationsDbContext>(options => options.UseNpgsql(connectionString,
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "reservations")));
        services.AddScoped<ISeatAvailabilityService, EfSeatAvailabilityService>();
        services.AddScoped<IReservationsApi, ReservationsApi>();
        return services;
    }
}
