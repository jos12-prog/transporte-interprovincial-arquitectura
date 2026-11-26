using TransportPlatform.Modules.Reservations.Domain.Enums;

namespace TransportPlatform.Modules.Reservations.Application.Contracts;

public sealed record SeatMapSeat(Guid TripSeatId, int Number, int? Row, int? Column, int Floor, TripSeatStatus Status);

public sealed record TripSeatMap(Guid TripId, IReadOnlyList<SeatMapSeat> Seats, string VehicleDisplayName = "");

public sealed record CreateReservationHoldRequest(
    Guid TripId,
    IReadOnlyCollection<Guid> TripSeatIds);

public sealed record ReservationHoldResult(
    Guid ReservationId,
    string HoldToken,
    DateTimeOffset ExpiresAtUtc);

public sealed class ReservationValidationException(string message) : Exception(message);
public sealed class ReservationNotFoundException(string message) : Exception(message);
public sealed class ReservationConflictException(string message) : Exception(message);
