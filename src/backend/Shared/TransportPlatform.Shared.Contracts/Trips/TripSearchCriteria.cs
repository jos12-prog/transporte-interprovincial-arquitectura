namespace TransportPlatform.Shared.Contracts.Trips;

public sealed record TripSearchCriteria(
    string Origin,
    string Destination,
    DateOnly ServiceDate,
    Guid? CompanyId = null,
    Guid? AgencyId = null,
    TimeOnly? DepartureFrom = null,
    TimeOnly? DepartureTo = null,
    decimal? MinimumPrice = null,
    decimal? MaximumPrice = null);
