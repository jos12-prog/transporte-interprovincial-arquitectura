using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure;

/// <summary>Registers persistence for the CompaniesAgencies module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddCompaniesAgenciesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<CompaniesAgenciesDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
