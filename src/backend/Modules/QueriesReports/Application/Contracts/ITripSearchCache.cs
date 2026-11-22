using TransportPlatform.Modules.QueriesReports.Application.UseCases;

namespace TransportPlatform.Modules.QueriesReports.Application.Contracts;

/// <summary>Cache port for the public, aggregated trip-search response only.</summary>
public interface ITripSearchCache
{
    Task<IReadOnlyList<TripSearchResult>?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(
        string key,
        IReadOnlyList<TripSearchResult> results,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default);
}
