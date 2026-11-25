namespace TransportPlatform.Shared.Contracts.Trips;

public sealed record SeatPositionContract(int Number, int Floor, int? Row, int? Column);

public sealed record VehicleModelAdminDto(
    Guid VehicleModelId,
    string Name,
    string? Category,
    int Capacity,
    IReadOnlyList<SeatPositionContract> Seats,
    bool IsActive,
    bool IsInUse);

public sealed record SaveVehicleModelRequest(
    string Name,
    string? Category,
    IReadOnlyList<SeatPositionContract> Seats);
