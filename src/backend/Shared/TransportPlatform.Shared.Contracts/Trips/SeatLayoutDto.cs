namespace TransportPlatform.Shared.Contracts.Trips;

public sealed record SeatPositionDto(int Number, int? Row, int? Column, int Floor);

public sealed record SeatLayoutDto(IReadOnlyList<SeatPositionDto> Seats, string VehicleDisplayName = "");
