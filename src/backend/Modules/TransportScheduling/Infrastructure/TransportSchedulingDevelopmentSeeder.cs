using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure;

public static class TransportSchedulingDevelopmentSeeder
{
    public static async Task SeedAsync(
        TransportSchedulingDbContext db,
        IReadOnlyCollection<Route> routes,
        IReadOnlyCollection<VehicleModel> models,
        IReadOnlyCollection<Vehicle> vehicles,
        IReadOnlyCollection<Trip> trips,
        CancellationToken cancellationToken = default)
    {
        await AddMissingAsync(db, db.Routes, routes, item => item.Id, cancellationToken);
        await AddMissingAsync(db, db.VehicleModels, models, item => item.Id, cancellationToken);
        await AddMissingAsync(db, db.Vehicles, vehicles, item => item.Id, cancellationToken);
        await AddMissingAsync(db, db.Trips, trips, item => item.Id, cancellationToken);
    }

    private static async Task AddMissingAsync<TEntity>(
        TransportSchedulingDbContext db,
        DbSet<TEntity> set,
        IReadOnlyCollection<TEntity> seeds,
        Func<TEntity, Guid> key,
        CancellationToken cancellationToken) where TEntity : class
    {
        var ids = (await set.Select(item => EF.Property<Guid>(item, "Id")).ToListAsync(cancellationToken)).ToHashSet();
        set.AddRange(seeds.Where(item => !ids.Contains(key(item))));
        await db.SaveChangesAsync(cancellationToken);
    }
}
