namespace TransportPlatform.Modules.QueriesReports.Application.UseCases;

public sealed record TripSearchExecutionResult(
    IReadOnlyList<TripSearchResult> Results,
    bool CacheHit);
