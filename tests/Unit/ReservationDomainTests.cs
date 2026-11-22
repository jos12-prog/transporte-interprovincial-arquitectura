using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class ReservationDomainTests
{
    [Fact]
    public void ReservationCreatesHeldSeatsAndExpiresTenMinutesAfterCreation()
    {
        var createdAt = new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.FromHours(-5));
        var reservation = CreateReservation(TripSeatIds(2), createdAt);

        Assert.Equal(ReservationStatus.Held, reservation.Status);
        Assert.Equal(createdAt.ToUniversalTime(), reservation.CreatedAtUtc);
        Assert.Equal(createdAt.ToUniversalTime().AddMinutes(10), reservation.ExpiresAtUtc);
        Assert.All(reservation.Seats, seat => Assert.Equal(ReservationStatus.Held, seat.Status));
    }

    [Fact]
    public void CancelTransitionsReservationAndAllSeatsToCancelled()
    {
        var reservation = CreateReservation(TripSeatIds(2), DateTimeOffset.UtcNow);

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.All(reservation.Seats, seat => Assert.Equal(ReservationStatus.Cancelled, seat.Status));
        Assert.Throws<InvalidOperationException>(reservation.Cancel);
    }

    [Fact]
    public void ExpireRequiresElapsedHoldDurationAndTransitionsAllSeats()
    {
        var reservation = CreateReservation(TripSeatIds(2), DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => reservation.Expire(reservation.ExpiresAtUtc.AddTicks(-1)));
        reservation.Expire(reservation.ExpiresAtUtc);

        Assert.Equal(ReservationStatus.Expired, reservation.Status);
        Assert.All(reservation.Seats, seat => Assert.Equal(ReservationStatus.Expired, seat.Status));
        Assert.Throws<InvalidOperationException>(() => reservation.Cancel());
    }

    [Fact]
    public void ReservationRequiresOneToTenUniqueNonEmptySeatIds()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentOutOfRangeException>(() => CreateReservation([], now));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateReservation(TripSeatIds(11), now));
        Assert.Throws<ArgumentException>(() => CreateReservation([Guid.NewGuid(), Guid.Empty], now));

        var repeatedId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => CreateReservation([repeatedId, repeatedId], now));
        Assert.Equal(10, CreateReservation(TripSeatIds(10), now).Seats.Count);
    }

    private static Reservation CreateReservation(IEnumerable<Guid> tripSeatIds, DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "token-hash", tripSeatIds, createdAt);

    private static Guid[] TripSeatIds(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();
}
