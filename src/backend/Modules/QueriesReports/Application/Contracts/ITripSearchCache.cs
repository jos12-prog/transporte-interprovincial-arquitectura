using TransportPlatform.Modules.QueriesReports.Application.UseCases;

namespace TransportPlatform.Modules.QueriesReports.Application.Contracts;

/// <summary>Cache port for the public, aggregated trip-search response only.</summary>
public interface ITripSearchCache
{
    Task<string> GetGenerationAsync(CancellationToken cancellationToken = default) => Task.FromResult("default");

    Task InvalidateAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    Task<IReadOnlyList<TripSearchResult>?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(
        string key,
        IReadOnlyList<TripSearchResult> results,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default);
}
