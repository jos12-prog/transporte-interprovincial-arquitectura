using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TransportPlatform.Modules.QueriesReports.Application.Contracts;
using TransportPlatform.Modules.QueriesReports.Application.UseCases;

namespace TransportPlatform.Modules.QueriesReports.Infrastructure.Caching;

public sealed class RedisTripSearchCache(IDistributedCache distributedCache) : ITripSearchCache
{
    private static readonly TimeSpan MaximumTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LocalGenerationTtl = TimeSpan.FromSeconds(2);
    private const string GenerationKey = "trip-search:generation:v1";

    // The generation is kept in memory for a short time so that each search performs a single
    // Redis operation. The same instance sees an invalidation immediately; other instances within 2 s.
    private LocalGeneration? _localGeneration;

    // Circuit breaker: while open, Redis is skipped and searches go straight to PostgreSQL.
    private long _openUntilTicks;

    private bool IsOpen => DateTime.UtcNow.Ticks < Interlocked.Read(ref _openUntilTicks);

    private void Open() =>
        Interlocked.Exchange(ref _openUntilTicks, DateTime.UtcNow.Add(BreakDuration).Ticks);

    private void RememberGeneration(string generation) =>
        Volatile.Write(ref _localGeneration, new LocalGeneration(generation, DateTime.UtcNow.Add(LocalGenerationTtl).Ticks));

    private void ForgetGeneration() => Volatile.Write(ref _localGeneration, null);

    public async Task<string> GetGenerationAsync(CancellationToken cancellationToken = default)
    {
        if (IsOpen) return "cache-bypass";
        var local = Volatile.Read(ref _localGeneration);
        if (local is not null && DateTime.UtcNow.Ticks < local.ExpiresAtTicks) return local.Value;
        try
        {
            var generation = await distributedCache.GetStringAsync(GenerationKey, cancellationToken)
                .WaitAsync(OperationTimeout, cancellationToken);
            if (generation is null)
            {
                generation = Guid.NewGuid().ToString("N");
                await distributedCache.SetStringAsync(GenerationKey, generation, new DistributedCacheEntryOptions(), cancellationToken)
                    .WaitAsync(OperationTimeout, cancellationToken);
            }

            RememberGeneration(generation);
            return generation;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            Open();
            ForgetGeneration();
            return "cache-bypass";
        }
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        if (IsOpen)
        {
            ForgetGeneration();
            return;
        }

        try
        {
            var generation = Guid.NewGuid().ToString("N");
            await distributedCache.SetStringAsync(GenerationKey, generation, new DistributedCacheEntryOptions(), cancellationToken)
                .WaitAsync(OperationTimeout, cancellationToken);
            RememberGeneration(generation);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            Open();
            ForgetGeneration();
        }
    }

    public async Task<IReadOnlyList<TripSearchResult>?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (IsOpen)
        {
            return null;
        }

        try
        {
            var payload = await distributedCache
                .GetStringAsync(key, cancellationToken)
                .WaitAsync(OperationTimeout, cancellationToken);
            return payload is null ? null : JsonSerializer.Deserialize<TripSearchResult[]>(payload);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            Open();
            return null;
        }
    }

    public async Task SetAsync(
        string key,
        IReadOnlyList<TripSearchResult> results,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(results);
        if (timeToLive <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeToLive), "Cache TTL must be positive.");
        }

        if (IsOpen)
        {
            return;
        }

        var effectiveTtl = timeToLive > MaximumTtl ? MaximumTtl : timeToLive;
        var payload = JsonSerializer.Serialize(results);

        try
        {
            await distributedCache
                .SetStringAsync(key, payload, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = effectiveTtl
                }, cancellationToken)
                .WaitAsync(OperationTimeout, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            Open();
        }
    }

    private sealed record LocalGeneration(string Value, long ExpiresAtTicks);
}
