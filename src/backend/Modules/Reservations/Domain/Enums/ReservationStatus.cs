namespace TransportPlatform.Modules.Reservations.Domain.Enums;

public enum ReservationStatus
{
    Held = 0,
    Expired = 1,
    Cancelled = 2,
    Confirmed = 3,
    Rescheduled = 4
}
