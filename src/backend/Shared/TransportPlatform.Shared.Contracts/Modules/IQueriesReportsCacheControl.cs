namespace TransportPlatform.Shared.Contracts.Modules;

/// <summary>Public cross-module contract for invalidating cached public trip-search results.</summary>
public interface IQueriesReportsCacheControl
{
    Task InvalidateTripSearchAsync(CancellationToken cancellationToken = default);
}
