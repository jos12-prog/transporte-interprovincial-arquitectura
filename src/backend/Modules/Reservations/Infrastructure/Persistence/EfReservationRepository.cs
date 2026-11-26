using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

internal sealed class EfReservationRepository(ReservationsDbContext dbContext, ITripReservationGuard? tripGuard = null) : IReservationRepository
{
    public async Task<IReadOnlyList<TripSeat>?> GetTripSeatsAsync(Guid tripId, CancellationToken cancellationToken = default) =>
        await dbContext.TripSeats.AsNoTracking()
            .Where(seat => seat.TripId == tripId)
            .OrderBy(seat => seat.SeatNumber)
            .ToArrayAsync(cancellationToken);

    public async Task CreateHoldAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (tripGuard is not null && !await tripGuard.LockActiveTripAsync(reservation.TripId, transaction.GetDbTransaction(), cancellationToken))
                throw new ReservationConflictException("A cancelled or unavailable trip cannot accept seat holds.");
            var ids = reservation.Seats.Select(seat => seat.TripSeatId).Order().ToArray();
            var lockedSeats = await dbContext.TripSeats
                .FromSqlInterpolated($"SELECT * FROM reservations.trip_seats WHERE id = ANY ({ids}) ORDER BY id FOR UPDATE")
                .ToListAsync(cancellationToken);

            if (lockedSeats.Count != ids.Length || lockedSeats.Any(seat =>
                    seat.TripId != reservation.TripId ||
                    seat.CompanyId != reservation.CompanyId ||
                    seat.AgencyId != reservation.AgencyId))
            {
                throw new ReservationNotFoundException("One or more trip seats were not found.");
            }

            if (lockedSeats.Any(seat => seat.Status != TripSeatStatus.Available))
            {
                throw new ReservationConflictException("One or more seats are no longer available.");
            }

            foreach (var seat in lockedSeats)
            {
                seat.Hold();
            }

            await dbContext.Reservations.AddAsync(reservation, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ReservationConflictException("One or more seats are already held.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> CancelHoldAsync(
        Guid reservationId,
        string holdTokenHash,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var locked = await dbContext.Reservations
                .FromSqlInterpolated($"SELECT * FROM reservations.reservations WHERE id = {reservationId} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (locked is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            var reservation = await dbContext.Reservations
                .Include(item => item.Seats)
                .SingleAsync(item => item.Id == reservationId, cancellationToken);
            if (!FixedTimeHashEquals(reservation.HoldTokenHash, holdTokenHash))
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            if (reservation.Status != ReservationStatus.Held || reservation.ExpiresAtUtc <= nowUtc)
            {
                throw new ReservationConflictException("The hold is no longer active.");
            }

            var seatIds = reservation.Seats.Select(item => item.TripSeatId).Order().ToArray();
            var seats = await dbContext.TripSeats
                .FromSqlInterpolated($"SELECT * FROM reservations.trip_seats WHERE id = ANY ({seatIds}) ORDER BY id FOR UPDATE")
                .ToListAsync(cancellationToken);
            if (seats.Count != seatIds.Length || seats.Any(seat => seat.Status != TripSeatStatus.Held))
            {
                throw new ReservationConflictException("The held seats have inconsistent state.");
            }

            reservation.Cancel();
            foreach (var seat in seats)
            {
                seat.ReleaseHold();
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> ExpireHoldsAsync(DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var expired = await dbContext.Reservations
                .FromSqlInterpolated($"SELECT * FROM reservations.reservations WHERE status = 'Held' AND expires_at_utc <= {nowUtc} ORDER BY expires_at_utc, id FOR UPDATE SKIP LOCKED")
                .ToListAsync(cancellationToken);

            foreach (var reservation in expired)
            {
                await dbContext.Entry(reservation).Collection(item => item.Seats).LoadAsync(cancellationToken);
                var seatIds = reservation.Seats.Select(item => item.TripSeatId).Order().ToArray();
                var seats = await dbContext.TripSeats
                    .FromSqlInterpolated($"SELECT * FROM reservations.trip_seats WHERE id = ANY ({seatIds}) ORDER BY id FOR UPDATE")
                    .ToListAsync(cancellationToken);

                if (seats.Count != seatIds.Length || seats.Any(seat => seat.Status != TripSeatStatus.Held))
                {
                    throw new ReservationConflictException("The held seats have inconsistent state.");
                }

                reservation.Expire(nowUtc);
                foreach (var seat in seats)
                {
                    seat.ReleaseHold();
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return expired.Count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static bool FixedTimeHashEquals(string expected, string supplied) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied));

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
