namespace TransportPlatform.Modules.QueriesReports.Application.UseCases;

public sealed record TripSearchResult(
    Guid TripId,
    Guid CompanyId,
    string CompanyName,
    Guid AgencyId,
    string AgencyName,
    string AgencyCity,
    string Origin,
    string Destination,
    DateTimeOffset DepartureAtUtc,
    DateTimeOffset ArrivalAtUtc,
    string VehicleModelName,
    decimal Price,
    string Currency,
    int AvailableSeats);
