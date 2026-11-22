using TransportPlatform.Modules.Reservations.Domain.Enums;

namespace TransportPlatform.Modules.Reservations.Domain.Entities;

public sealed class ReservationSeat
{
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid TripSeatId { get; private set; }
    public ReservationStatus Status { get; private set; }

    private ReservationSeat()
    {
    }

    internal ReservationSeat(Guid reservationId, Guid tripSeatId)
    {
        if (reservationId == Guid.Empty || tripSeatId == Guid.Empty)
        {
            throw new ArgumentException("Reservation and trip seat identifiers are required.");
        }

        Id = Guid.NewGuid();
        ReservationId = reservationId;
        TripSeatId = tripSeatId;
        Status = ReservationStatus.Held;
    }

    internal void TransitionTo(ReservationStatus status)
    {
        if (Status != ReservationStatus.Held || status is not (ReservationStatus.Expired or ReservationStatus.Cancelled))
        {
            throw new InvalidOperationException($"Reservation seat cannot transition from {Status} to {status}.");
        }

        Status = status;
    }
}
