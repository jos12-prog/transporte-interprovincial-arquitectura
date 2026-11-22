namespace TransportPlatform.Modules.QueriesReports.Application.UseCases;

public sealed record SearchTripsRequest(
    string? Origin,
    string? Destination,
    DateOnly? ServiceDate,
    Guid? CompanyId = null,
    Guid? AgencyId = null,
    TimeOnly? DepartureFrom = null,
    TimeOnly? DepartureTo = null,
    decimal? MinimumPrice = null,
    decimal? MaximumPrice = null);
