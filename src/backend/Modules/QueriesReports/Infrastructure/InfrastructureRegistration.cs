using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.QueriesReports.Infrastructure.Persistence;

namespace TransportPlatform.Modules.QueriesReports.Infrastructure;

/// <summary>Registers persistence for the QueriesReports module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddQueriesReportsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<QueriesReportsDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
