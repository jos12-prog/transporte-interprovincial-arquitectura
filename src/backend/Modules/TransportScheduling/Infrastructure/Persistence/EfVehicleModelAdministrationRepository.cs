using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.UseCases;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

internal sealed class EfVehicleModelAdministrationRepository(TransportSchedulingDbContext dbContext)
    : IVehicleModelAdministrationRepository
{
    public async Task<IReadOnlyList<VehicleModel>> GetVehicleModelsAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.VehicleModels.AsNoTracking();
        if (activeOnly) query = query.Where(model => model.IsActive);
        return await query.OrderBy(model => model.Name).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetVehicleModelIdsInUseAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Vehicles.AsNoTracking()
            .Select(vehicle => vehicle.VehicleModelId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

    public Task<VehicleModel?> GetVehicleModelAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.VehicleModels.SingleOrDefaultAsync(model => model.Id == id, cancellationToken);

    public Task<bool> VehicleModelNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
        dbContext.VehicleModels.AnyAsync(model => model.Name.ToLower() == name.ToLower()
            && (!excludingId.HasValue || model.Id != excludingId.Value), cancellationToken);

    public Task<bool> IsVehicleModelInUseAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Vehicles.AnyAsync(vehicle => vehicle.VehicleModelId == id, cancellationToken);

    public async Task AddVehicleModelAsync(VehicleModel model, CancellationToken cancellationToken = default)
    {
        dbContext.VehicleModels.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveVehicleModelAsync(VehicleModel model, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.VehicleModels
            .FromSqlInterpolated($"SELECT * FROM transport_scheduling.vehicle_models WHERE id = {model.Id} FOR UPDATE")
            .IgnoreQueryFilters()
            .SingleAsync(cancellationToken);

        var entry = dbContext.Entry(model);
        var previousLayout = entry.Property(item => item.SeatLayout).OriginalValue;
        var layoutChanged = previousLayout is not null && !previousLayout.Seats.SequenceEqual(model.SeatLayout.Seats);
        if (layoutChanged && await IsVehicleModelInUseAsync(model.Id, cancellationToken))
        {
            throw new VehicleModelConflictException("The seat layout of a vehicle model in use cannot be changed.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
