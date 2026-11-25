using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

namespace TransportPlatform.Host;

internal static class DevelopmentDatabaseMigrator
{
    public static async Task ApplyAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CompaniesAgenciesDbContext>()
            .Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>()
            .Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<TransportSchedulingDbContext>()
            .Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>()
            .Database.MigrateAsync(cancellationToken);
        await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider, cancellationToken);
    }
}
