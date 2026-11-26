using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;
using TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

namespace TransportPlatform.Host;

/// <summary>Coordinates MI03 and MI04 writes on the shared PostgreSQL transaction.</summary>
public sealed class EfTripSchedulingCoordinator(
    TransportSchedulingDbContext scheduling,
    ReservationsDbContext reservations) : ITripSchedulingCoordinator
{
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);

    public async Task CreateTripWithSeatsAsync(Trip trip, SeatLayout layout, CancellationToken cancellationToken = default)
    {
        reservations.Database.SetDbConnection(scheduling.Database.GetDbConnection());
        await using var transaction = await scheduling.Database.BeginTransactionAsync(cancellationToken);
        await reservations.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
        try
        {
            var route = await scheduling.Routes.FromSqlInterpolated($"SELECT * FROM transport_scheduling.routes WHERE id = {trip.RouteId} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (route is null || route.CompanyId != trip.CompanyId || !route.IsActive)
                throw new AgencyCatalogConflictException("The route is no longer available for scheduling.");
            await scheduling.Vehicles.FromSqlInterpolated($"SELECT * FROM transport_scheduling.vehicles WHERE id = {trip.VehicleId} FOR UPDATE")
                .SingleAsync(cancellationToken);
            var vehicle = await scheduling.Vehicles.SingleAsync(x => x.Id == trip.VehicleId, cancellationToken);
            var model = await scheduling.VehicleModels.SingleAsync(x => x.Id == vehicle.VehicleModelId, cancellationToken);
            if (!vehicle.IsActive || !model.IsActive || vehicle.CompanyId != trip.CompanyId || vehicle.AgencyId != trip.AgencyId)
                throw new AgencyCatalogConflictException("The vehicle or its model is no longer available for scheduling.");
            var serviceDate = DateOnly.FromDateTime(trip.DepartureAtUtc.ToOffset(LimaOffset).DateTime);
            var currentFare = Fare.ResolveFor(serviceDate, await scheduling.Fares.AsNoTracking()
                .Where(fare => fare.CompanyId == trip.CompanyId && fare.RouteId == trip.RouteId)
                .ToArrayAsync(cancellationToken));
            if (currentFare is null || currentFare.Price != trip.Price || currentFare.Currency != trip.Currency)
                throw new AgencyCatalogConflictException("The applicable fare changed while scheduling. Review the fare and retry.");
            var conflict = await scheduling.Trips.AnyAsync(x => x.VehicleId == trip.VehicleId && x.Status == TripStatus.Scheduled &&
                x.DepartureAtUtc < trip.ArrivalAtUtc && trip.DepartureAtUtc < x.ArrivalAtUtc, cancellationToken);
            if (conflict) throw new AgencyCatalogConflictException("The vehicle is already assigned to a trip during this time.");
            scheduling.Trips.Add(trip);
            reservations.TripSeats.AddRange(layout.Seats.Select(seat => new TripSeat(Guid.NewGuid(), trip.CompanyId, trip.AgencyId,
                trip.Id, seat.Number, TripSeatStatus.Available, seat.Row, seat.Column, seat.Floor)));
            await scheduling.SaveChangesAsync(cancellationToken);
            await reservations.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally { await reservations.Database.UseTransactionAsync(null, cancellationToken); }
    }

    public async Task<bool> CancelTripAndReleaseHoldsAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        reservations.Database.SetDbConnection(scheduling.Database.GetDbConnection());
        await using var transaction = await scheduling.Database.BeginTransactionAsync(cancellationToken);
        await reservations.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
        try
        {
            await scheduling.Trips.FromSqlInterpolated($"SELECT * FROM transport_scheduling.trips WHERE id = {trip.Id} FOR UPDATE")
                .SingleAsync(cancellationToken);
            await reservations.Reservations.FromSqlInterpolated($"SELECT * FROM reservations.reservations WHERE trip_id = {trip.Id} AND status = 'Held' ORDER BY id FOR UPDATE")
                .ToListAsync(cancellationToken);
            var held = await reservations.Reservations.Where(x => x.TripId == trip.Id && x.Status == ReservationStatus.Held)
                .Include(x => x.Seats).ToListAsync(cancellationToken);
            var seats = await reservations.TripSeats.FromSqlInterpolated($"SELECT * FROM reservations.trip_seats WHERE trip_id = {trip.Id} ORDER BY id FOR UPDATE")
                .ToListAsync(cancellationToken);
            if (seats.Any(seat => seat.Status == TripSeatStatus.Confirmed))
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            foreach (var reservation in held) reservation.Cancel();
            foreach (var seat in seats.Where(x => x.Status == TripSeatStatus.Held)) seat.ReleaseHold();
            await scheduling.SaveChangesAsync(cancellationToken);
            await reservations.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally { await reservations.Database.UseTransactionAsync(null, cancellationToken); }
    }
}
