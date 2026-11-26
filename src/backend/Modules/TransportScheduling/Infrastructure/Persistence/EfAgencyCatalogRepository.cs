using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

internal sealed class EfAgencyCatalogRepository(TransportSchedulingDbContext db) : IAgencyCatalogRepository
{
    public async Task<IReadOnlyList<Route>> GetRoutesAsync(Guid companyId, CancellationToken ct = default) =>
        await db.Routes.Where(x => x.CompanyId == companyId).OrderBy(x => x.OriginCity).ThenBy(x => x.DestinationCity).ToArrayAsync(ct);
    public Task<Route?> GetRouteAsync(Guid id, Guid companyId, CancellationToken ct = default) =>
        db.Routes.SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);
    public Task<bool> RouteExistsAsync(Guid id, Guid companyId, CancellationToken ct = default) =>
        db.Routes.AnyAsync(x => x.Id == id && x.CompanyId == companyId, ct);
    public async Task AddRouteAsync(Route route, CancellationToken ct = default) { db.Routes.Add(route); await db.SaveChangesAsync(ct); }
    public async Task SaveRouteAsync(Route route, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        _ = await db.Routes.FromSqlInterpolated($"SELECT * FROM transport_scheduling.routes WHERE id = {route.Id} FOR UPDATE").SingleAsync(ct);
        db.Routes.Update(route);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(Guid companyId, Guid agencyId, CancellationToken ct = default) =>
        await db.Vehicles.Where(x => x.CompanyId == companyId && x.AgencyId == agencyId).OrderBy(x => x.DisplayName).ToArrayAsync(ct);
    public Task<Vehicle?> GetVehicleAsync(Guid id, Guid companyId, Guid agencyId, CancellationToken ct = default) =>
        db.Vehicles.SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && x.AgencyId == agencyId, ct);
    public Task<bool> VehiclePlateExistsAsync(Guid companyId, string plate, Guid? excludingId = null, CancellationToken ct = default) =>
        db.Vehicles.AnyAsync(x => x.CompanyId == companyId && x.Plate == plate && (!excludingId.HasValue || x.Id != excludingId), ct);
    public Task<bool> VehicleHasTripsAsync(Guid vehicleId, CancellationToken ct = default) => db.Trips.AnyAsync(x => x.VehicleId == vehicleId, ct);
    public async Task AddVehicleAsync(Vehicle vehicle, CancellationToken ct = default) { db.Vehicles.Add(vehicle); await db.SaveChangesAsync(ct); }
    public async Task SaveVehicleAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        _ = await db.Vehicles.FromSqlInterpolated($"SELECT * FROM transport_scheduling.vehicles WHERE id = {vehicle.Id} FOR UPDATE").SingleAsync(ct);
        db.Vehicles.Update(vehicle);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<Fare>> GetFaresAsync(Guid routeId, Guid companyId, CancellationToken ct = default) =>
        await db.Fares.Where(x => x.RouteId == routeId && x.CompanyId == companyId).OrderBy(x => x.ValidFrom).ToArrayAsync(ct);
    public Task<Fare?> GetFareAsync(Guid id, Guid routeId, Guid companyId, CancellationToken ct = default) =>
        db.Fares.SingleOrDefaultAsync(x => x.Id == id && x.RouteId == routeId && x.CompanyId == companyId, ct);
    public Task<bool> FarePeriodOverlapsAsync(Guid routeId, Guid companyId, FareType type, DateOnly from, DateOnly? to, Guid? excludingId = null, CancellationToken ct = default) =>
        db.Fares.AnyAsync(x => x.RouteId == routeId && x.CompanyId == companyId && x.Type == type && (!excludingId.HasValue || x.Id != excludingId) &&
            (!x.ValidTo.HasValue || x.ValidTo >= from) && (!to.HasValue || x.ValidFrom <= to.Value), ct);
    public async Task AddFareAsync(Fare fare, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockRouteAsync(fare.RouteId, ct);
        if (await FarePeriodOverlapsAsync(fare.RouteId, fare.CompanyId, fare.Type, fare.ValidFrom, fare.ValidTo, ct: ct))
            throw new AgencyCatalogConflictException("The fare period overlaps another fare of the same type and route.");
        db.Fares.Add(fare);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
    public async Task SaveFareAsync(Fare fare, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await LockRouteAsync(fare.RouteId, ct);
        if (await FarePeriodOverlapsAsync(fare.RouteId, fare.CompanyId, fare.Type, fare.ValidFrom, fare.ValidTo, fare.Id, ct))
            throw new AgencyCatalogConflictException("The fare period overlaps another fare of the same type and route.");
        db.Fares.Update(fare);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<Trip>> GetTripsAsync(Guid companyId, Guid agencyId, CancellationToken ct = default) =>
        await db.Trips.Where(x => x.CompanyId == companyId && x.AgencyId == agencyId).OrderBy(x => x.DepartureAtUtc).ToArrayAsync(ct);
    public Task<Trip?> GetTripAsync(Guid id, Guid companyId, Guid agencyId, CancellationToken ct = default) =>
        db.Trips.SingleOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && x.AgencyId == agencyId, ct);
    public Task<bool> HasOverlappingScheduledTripAsync(Guid vehicleId, DateTimeOffset from, DateTimeOffset to, Guid? excludingTripId = null, CancellationToken ct = default) =>
        db.Trips.AnyAsync(x => x.VehicleId == vehicleId && x.Status == TripStatus.Scheduled && x.Id != excludingTripId &&
            x.DepartureAtUtc < to && from < x.ArrivalAtUtc, ct);

    private async Task LockRouteAsync(Guid routeId, CancellationToken ct) =>
        _ = await db.Routes.FromSqlInterpolated($"SELECT * FROM transport_scheduling.routes WHERE id = {routeId} FOR UPDATE")
            .SingleOrDefaultAsync(ct) ?? throw new AgencyCatalogNotFoundException("Route was not found.");
}
