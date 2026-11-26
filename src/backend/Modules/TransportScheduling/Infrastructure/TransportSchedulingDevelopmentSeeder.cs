using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure;

public static class TransportSchedulingDevelopmentSeeder
{
    public static async Task SeedAsync(
        TransportSchedulingDbContext db,
        IReadOnlyCollection<Route> routes,
        IReadOnlyCollection<Fare> fares,
        IReadOnlyCollection<VehicleModel> models,
        IReadOnlyCollection<Vehicle> vehicles,
        IReadOnlyCollection<Trip> trips,
        CancellationToken cancellationToken = default)
    {
        await AddMissingAsync(db, db.Routes, routes, item => item.Id, cancellationToken);
        var routeIds = routes.Select(route => route.Id).ToArray();
        var existingRoutes = await db.Routes.Where(route => routeIds.Contains(route.Id)).ToListAsync(cancellationToken);
        var seededRoutes = routes.ToDictionary(route => route.Id);
        foreach (var existing in existingRoutes)
        {
            var seed = seededRoutes[existing.Id];
            existing.Update(seed.OriginCity, seed.DestinationCity);
        }
        await db.SaveChangesAsync(cancellationToken);

        await AddMissingAsync(db, db.Fares, fares, item => item.Id, cancellationToken);
        var fareIds = fares.Select(fare => fare.Id).ToArray();
        var existingFares = await db.Fares.Where(fare => fareIds.Contains(fare.Id)).ToListAsync(cancellationToken);
        var seededFares = fares.ToDictionary(fare => fare.Id);
        foreach (var existing in existingFares)
        {
            var seed = seededFares[existing.Id];
            existing.Update(seed.ValidFrom, seed.ValidTo, seed.Price, seed.Currency);
        }
        await db.SaveChangesAsync(cancellationToken);
        await AddMissingAsync(db, db.VehicleModels, models, item => item.Id, cancellationToken);
        var modelIds = models.Select(model => model.Id).ToArray();
        var existingModels = await db.VehicleModels.Where(model => modelIds.Contains(model.Id)).ToListAsync(cancellationToken);
        var seededModels = models.ToDictionary(model => model.Id);
        foreach (var existing in existingModels)
        {
            existing.SetCategory(seededModels[existing.Id].Category);
        }
        await db.SaveChangesAsync(cancellationToken);
        var vehicleIds = vehicles.Select(vehicle => vehicle.Id).ToArray();
        var existingVehicles = await db.Vehicles.Where(vehicle => vehicleIds.Contains(vehicle.Id)).ToListAsync(cancellationToken);
        var seededVehicles = vehicles.ToDictionary(vehicle => vehicle.Id);
        foreach (var existing in existingVehicles)
        {
            var seed = seededVehicles[existing.Id];
            existing.UpdateProfile(seed.Plate, seed.DisplayName, seed.VehicleModelId);
        }
        await db.SaveChangesAsync(cancellationToken);
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
