using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.Services;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;
using TransportPlatform.Shared.Contracts.Administration;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure;

/// <summary>Registers persistence for the TransportScheduling module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddTransportSchedulingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<TransportSchedulingDbContext>(options => options.UseNpgsql(connectionString,
            postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "transport_scheduling")));
        services.AddScoped<ITripSearchReadRepository, EfTripSearchReadRepository>();
        services.AddScoped<IAgencyCatalogRepository, EfAgencyCatalogRepository>();
        services.AddScoped<IAgencyScopeValidator, AgencyScopeValidator>();
        services.AddScoped<IAgencyCatalogAdminApi, AgencyCatalogAdminApi>();
        services.AddScoped<IVehicleModelAdministrationRepository, EfVehicleModelAdministrationRepository>();
        services.AddScoped<TransportSchedulingApi>();
        services.AddScoped<ITransportSchedulingApi>(provider => provider.GetRequiredService<TransportSchedulingApi>());
        services.AddScoped<ITransportSchedulingAdminApi>(provider => provider.GetRequiredService<TransportSchedulingApi>());
        return services;
    }
}
