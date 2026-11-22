using TransportPlatform.Modules.Reservations.Domain.Enums;

namespace TransportPlatform.Modules.Reservations.Domain.Entities;

public sealed class TripSeat
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid TripId { get; private set; }
    public int SeatNumber { get; private set; }
    public int? Row { get; private set; }
    public int? Column { get; private set; }
    public int Floor { get; private set; }
    public TripSeatStatus Status { get; private set; }

    private TripSeat()
    {
    }

    public TripSeat(
        Guid id,
        Guid companyId,
        Guid agencyId,
        Guid tripId,
        int seatNumber,
        TripSeatStatus status = TripSeatStatus.Available,
        int? row = null,
        int? column = null,
        int floor = 1)
    {
        if (id == Guid.Empty || companyId == Guid.Empty || agencyId == Guid.Empty || tripId == Guid.Empty)
        {
            throw new ArgumentException("Trip seat and tenant identifiers are required.");
        }

        if (seatNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seatNumber), "Seat number must be positive.");
        }

        if (row.HasValue != column.HasValue || row is <= 0 || column is <= 0 || floor <= 0)
        {
            throw new ArgumentException("Seat position must have a valid row, column, and floor.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Trip seat status is not supported.");
        }

        Id = id;
        CompanyId = companyId;
        AgencyId = agencyId;
        TripId = tripId;
        SeatNumber = seatNumber;
        Row = row;
        Column = column;
        Floor = floor;
        Status = status;
    }

    public void Hold()
    {
        if (Status != TripSeatStatus.Available)
        {
            throw new InvalidOperationException($"A trip seat cannot be held from {Status}.");
        }

        Status = TripSeatStatus.Held;
    }

    public void ReleaseHold()
    {
        if (Status != TripSeatStatus.Held)
        {
            throw new InvalidOperationException($"A trip seat cannot be released from {Status}.");
        }

        Status = TripSeatStatus.Available;
    }
}
