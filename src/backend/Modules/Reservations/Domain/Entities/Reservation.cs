using TransportPlatform.Modules.Reservations.Domain.Enums;

namespace TransportPlatform.Modules.Reservations.Domain.Entities;

public sealed class Reservation
{
    public const int MaximumSeatsPerHold = 10;
    public static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

    private readonly List<ReservationSeat> _seats = [];

    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid TripId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string HoldTokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public IReadOnlyCollection<ReservationSeat> Seats => _seats.AsReadOnly();

    private Reservation()
    {
    }

    public Reservation(
        Guid id,
        Guid companyId,
        Guid agencyId,
        Guid tripId,
        string holdTokenHash,
        IEnumerable<Guid> tripSeatIds,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(tripSeatIds);
        if (id == Guid.Empty || companyId == Guid.Empty || agencyId == Guid.Empty || tripId == Guid.Empty)
        {
            throw new ArgumentException("Reservation and tenant identifiers are required.");
        }

        if (string.IsNullOrWhiteSpace(holdTokenHash))
        {
            throw new ArgumentException("A hold token hash is required.", nameof(holdTokenHash));
        }

        var seatIds = tripSeatIds.ToArray();
        if (seatIds.Length is < 1 or > MaximumSeatsPerHold)
        {
            throw new ArgumentOutOfRangeException(nameof(tripSeatIds), $"A hold must contain between 1 and {MaximumSeatsPerHold} seats.");
        }

        if (seatIds.Any(id => id == Guid.Empty) || seatIds.Distinct().Count() != seatIds.Length)
        {
            throw new ArgumentException("Trip seat identifiers must be non-empty and unique.", nameof(tripSeatIds));
        }

        Id = id;
        CompanyId = companyId;
        AgencyId = agencyId;
        TripId = tripId;
        Status = ReservationStatus.Held;
        HoldTokenHash = holdTokenHash;
        CreatedAtUtc = createdAt.ToUniversalTime();
        ExpiresAtUtc = CreatedAtUtc.Add(HoldDuration);
        _seats.AddRange(seatIds.Select(seatId => new ReservationSeat(Id, seatId)));
    }

    public void Cancel()
    {
        TransitionHeldTo(ReservationStatus.Cancelled);
    }

    public void Expire(DateTimeOffset now)
    {
        if (Status != ReservationStatus.Held)
        {
            throw new InvalidOperationException($"Reservation cannot expire from {Status}.");
        }

        if (now.ToUniversalTime() < ExpiresAtUtc)
        {
            throw new InvalidOperationException("Reservation cannot expire before its hold duration has elapsed.");
        }

        TransitionHeldTo(ReservationStatus.Expired);
    }

    private void TransitionHeldTo(ReservationStatus status)
    {
        if (Status != ReservationStatus.Held)
        {
            throw new InvalidOperationException($"Reservation cannot transition from {Status} to {status}.");
        }

        foreach (var seat in _seats)
        {
            seat.TransitionTo(status);
        }

        Status = status;
    }
}
