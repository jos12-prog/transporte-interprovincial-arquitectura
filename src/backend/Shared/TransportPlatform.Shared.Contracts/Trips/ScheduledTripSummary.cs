namespace TransportPlatform.Shared.Contracts.Trips;

public sealed record ScheduledTripSummary(
    Guid TripId,
    Guid CompanyId,
    Guid AgencyId,
    string Origin,
    string Destination,
    DateTimeOffset DepartureAtUtc,
    DateTimeOffset ArrivalAtUtc,
    string VehicleModelName,
    decimal Price,
    string Currency,
    int AvailableSeats);
