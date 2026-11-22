using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Application.UseCases;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class ReservationUseCaseTests
{
    [Fact]
    public async Task CreateHoldAcceptsOneAndTenSeats()
    {
        var requestOne = Request(1);
        var requestTen = Request(10);
        var repository = new FakeReservationRepository();
        var useCase = new CreateReservationHoldUseCase(repository, new FrozenTimeProvider());
        repository.Seats = SeatsFor(requestOne).Concat(SeatsFor(requestTen)).ToArray();

        var one = await useCase.ExecuteAsync(requestOne);
        var ten = await useCase.ExecuteAsync(requestTen);

        Assert.NotEmpty(one.HoldToken);
        Assert.NotEmpty(ten.HoldToken);
        Assert.Equal(2, repository.CreateCount);
        Assert.Equal(10, repository.LastCreated!.Seats.Count);
    }

    [Fact]
    public async Task CreateHoldRejectsZeroAndElevenSeatsBeforeRepositoryCall()
    {
        var repository = new FakeReservationRepository();
        var useCase = new CreateReservationHoldUseCase(repository, new FrozenTimeProvider());

        await Assert.ThrowsAsync<ReservationValidationException>(() => useCase.ExecuteAsync(Request(0)));
        await Assert.ThrowsAsync<ReservationValidationException>(() => useCase.ExecuteAsync(Request(11)));
        var duplicate = Guid.NewGuid();
        await Assert.ThrowsAsync<ReservationValidationException>(() => useCase.ExecuteAsync(Request([duplicate, duplicate])));

        Assert.Equal(0, repository.CreateCount);
    }

    [Fact]
    public async Task CreateHoldPropagatesRepositoryConflictWithoutPartialResult()
    {
        var repository = new FakeReservationRepository { ThrowConflict = true };
        var useCase = new CreateReservationHoldUseCase(repository, new FrozenTimeProvider());
        var request = Request(2);
        repository.Seats = SeatsFor(request);

        await Assert.ThrowsAsync<ReservationConflictException>(() => useCase.ExecuteAsync(request));
        Assert.Equal(1, repository.CreateCount);
    }

    [Fact]
    public async Task ReleaseHoldHashesTokenAndUsesControlledUtcClock()
    {
        var repository = new FakeReservationRepository { CancelResult = true };
        var clock = new FrozenTimeProvider();
        var useCase = new ReleaseReservationHoldUseCase(repository, clock);
        const string token = "opaque-hold-token";
        var now = clock.GetUtcNow();

        await useCase.ExecuteAsync(Guid.NewGuid(), token);

        Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))), repository.CancelTokenHash);
        Assert.Equal(now, repository.CancelNowUtc);
    }

    [Fact]
    public async Task ReleaseHoldReturnsNotFoundForInvalidTokenOrMissingHold()
    {
        var useCase = new ReleaseReservationHoldUseCase(new FakeReservationRepository(), new FrozenTimeProvider());

        await Assert.ThrowsAsync<ReservationNotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid(), "wrong-token"));
    }

    private static CreateReservationHoldRequest Request(int count) => Request(Guid.NewGuid(), Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray());

    private static CreateReservationHoldRequest Request(IReadOnlyCollection<Guid> ids) =>
        Request(Guid.NewGuid(), ids);

    private static CreateReservationHoldRequest Request(Guid tripId, IReadOnlyCollection<Guid> ids) => new(tripId, ids);

    private static TripSeat[] SeatsFor(CreateReservationHoldRequest request) => request.TripSeatIds
        .Select((id, index) => new TripSeat(id, Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"), request.TripId, index + 1))
        .ToArray();

    private sealed class FrozenTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 11, 23, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public int CreateCount { get; private set; }
        public Reservation? LastCreated { get; private set; }
        public bool ThrowConflict { get; init; }
        public bool CancelResult { get; init; }
        public IReadOnlyList<TripSeat> Seats { get; set; } = [];
        public string? CancelTokenHash { get; private set; }
        public DateTimeOffset? CancelNowUtc { get; private set; }

        public Task<IReadOnlyList<TripSeat>?> GetTripSeatsAsync(Guid tripId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TripSeat>?>(Seats.Where(seat => seat.TripId == tripId).ToArray());

        public Task CreateHoldAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            CreateCount++;
            if (ThrowConflict)
            {
                throw new ReservationConflictException("Seat already held.");
            }

            LastCreated = reservation;
            return Task.CompletedTask;
        }

        public Task<bool> CancelHoldAsync(Guid reservationId, string holdTokenHash, DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
        {
            CancelTokenHash = holdTokenHash;
            CancelNowUtc = nowUtc;
            return Task.FromResult(CancelResult);
        }

        public Task<int> ExpireHoldsAsync(DateTimeOffset nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
