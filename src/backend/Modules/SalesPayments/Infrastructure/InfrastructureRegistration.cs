using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.SalesPayments.Infrastructure.Persistence;

namespace TransportPlatform.Modules.SalesPayments.Infrastructure;

/// <summary>Registers persistence for the SalesPayments module.</summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddSalesPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["POSTGRES_CONNECTION_STRING"]
            ?? "Host=postgres;Port=5432;Database=transport;Username=transport;Password=local-development-only";
        services.AddDbContext<SalesPaymentsDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
