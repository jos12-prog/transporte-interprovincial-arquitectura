using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Modules.Reservations.Infrastructure;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class ReservationRepositoryTests
{
    [Fact]
    public async Task HoldRollsBackEntireMultiSeatGroupAndReleaseRestoresAllSeats()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:16").Build();
        await postgres.StartAsync();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgres.GetConnectionString()
        }).Build();
        var services = new ServiceCollection();
        services.AddReservationsInfrastructure(configuration);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        await db.Database.EnsureCreatedAsync();
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var seatIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        db.TripSeats.AddRange(seatIds.Select((id, index) => new TripSeat(id, companyId, agencyId, tripId, index + 1)));
        await db.SaveChangesAsync();
        var repository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
        var now = new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);
        var firstReservation = ReservationFor(companyId, agencyId, tripId,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("first-token"))), [seatIds[0]], now);
        await repository.CreateHoldAsync(firstReservation);

        var conflictingGroup = ReservationFor(companyId, agencyId, tripId, "second-token-hash", [seatIds[0], seatIds[1]], now);
        await Assert.ThrowsAsync<ReservationConflictException>(() => repository.CreateHoldAsync(conflictingGroup));
        var afterConflict = await db.TripSeats.AsNoTracking().SingleAsync(seat => seat.Id == seatIds[1]);
        Assert.Equal(TripSeatStatus.Available, afterConflict.Status);
        Assert.False(await db.Reservations.AsNoTracking().AnyAsync(reservation => reservation.Id == conflictingGroup.Id));

        Assert.True(await repository.CancelHoldAsync(firstReservation.Id,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("first-token"))), now.AddMinutes(1)));
        var releasedSeat = await db.TripSeats.AsNoTracking().SingleAsync(seat => seat.Id == seatIds[0]);
        Assert.Equal(TripSeatStatus.Available, releasedSeat.Status);
        var cancelled = await db.Reservations.AsNoTracking().SingleAsync(reservation => reservation.Id == firstReservation.Id);
        Assert.Equal(ReservationStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task ConcurrentExpirationWorkersExpireAndReleaseEachHoldOnce()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:16").Build();
        await postgres.StartAsync();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgres.GetConnectionString()
        }).Build();
        var services = new ServiceCollection();
        services.AddReservationsInfrastructure(configuration);
        await using var provider = services.BuildServiceProvider();
        await using var seedScope = provider.CreateAsyncScope();
        var db = seedScope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
        await db.Database.EnsureCreatedAsync();
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var seatId = Guid.NewGuid();
        var tripSeat = new TripSeat(seatId, companyId, agencyId, tripId, 1);
        tripSeat.Hold();
        db.TripSeats.Add(tripSeat);
        var now = new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);
        var reservation = ReservationFor(companyId, agencyId, tripId, "expired-token-hash", [seatId], now.AddMinutes(-11));
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        var expirationTime = reservation.ExpiresAtUtc;

        async Task<int> RunExpirationAsync()
        {
            await using var scope = provider.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IReservationRepository>()
                .ExpireHoldsAsync(expirationTime);
        }

        var expiredCounts = await Task.WhenAll(RunExpirationAsync(), RunExpirationAsync());

        Assert.Equal(1, expiredCounts.Sum());
        var finalSeat = await db.TripSeats.AsNoTracking().SingleAsync(seat => seat.Id == seatId);
        var finalReservation = await db.Reservations.AsNoTracking().SingleAsync(item => item.Id == reservation.Id);
        Assert.Equal(TripSeatStatus.Available, finalSeat.Status);
        Assert.Equal(ReservationStatus.Expired, finalReservation.Status);
    }

    private static Reservation ReservationFor(
        Guid companyId,
        Guid agencyId,
        Guid tripId,
        string tokenHash,
        Guid[] tripSeatIds,
        DateTimeOffset now) =>
        new(Guid.NewGuid(), companyId, agencyId, tripId, tokenHash, tripSeatIds, now);
}
