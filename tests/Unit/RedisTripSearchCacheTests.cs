using Microsoft.Extensions.Caching.Distributed;
using TransportPlatform.Modules.QueriesReports.Infrastructure.Caching;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class RedisTripSearchCacheTests
{
    [Fact]
    public async Task FailedGetOpensBreakerAndImmediateGetAndSetSkipDistributedCache()
    {
        var distributedCache = new ThrowingDistributedCache();
        var cache = new RedisTripSearchCache(distributedCache);

        Assert.Null(await cache.GetAsync("trip-search:test"));
        Assert.Equal(1, distributedCache.CallCount);

        Assert.Null(await cache.GetAsync("trip-search:test"));
        await cache.SetAsync(
            "trip-search:test",
            Array.Empty<TransportPlatform.Modules.QueriesReports.Application.UseCases.TripSearchResult>(),
            TimeSpan.FromSeconds(60));

        Assert.Equal(1, distributedCache.CallCount);
    }

    private sealed class ThrowingDistributedCache : IDistributedCache
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public byte[]? Get(string key) => CountAndThrow<byte[]?>();

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            CountAndThrow<Task<byte[]?>>();

        public void Refresh(string key) => CountAndThrow<object?>();

        public Task RefreshAsync(string key, CancellationToken token = default) =>
            CountAndThrow<Task>();

        public void Remove(string key) => CountAndThrow<object?>();

        public Task RemoveAsync(string key, CancellationToken token = default) =>
            CountAndThrow<Task>();

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            CountAndThrow<object?>();

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default) => CountAndThrow<Task>();

        private T CountAndThrow<T>()
        {
            Interlocked.Increment(ref _callCount);
            throw new InvalidOperationException("Simulated Redis failure.");
        }
    }
}
