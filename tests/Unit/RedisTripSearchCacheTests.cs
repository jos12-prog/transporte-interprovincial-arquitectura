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

    [Fact]
    public async Task GenerationIsReadFromRedisOnceWithinLocalTtl()
    {
        var distributedCache = new RecordingDistributedCache();
        var cache = new RedisTripSearchCache(distributedCache);

        var first = await cache.GetGenerationAsync();
        var second = await cache.GetGenerationAsync();

        Assert.Equal(first, second);
        Assert.Equal(1, distributedCache.GetCallCount);
    }

    [Fact]
    public async Task InvalidationIsVisibleImmediatelyWithoutReadingRedis()
    {
        var distributedCache = new RecordingDistributedCache();
        var cache = new RedisTripSearchCache(distributedCache);

        var before = await cache.GetGenerationAsync();
        await cache.InvalidateAsync();
        var after = await cache.GetGenerationAsync();

        Assert.NotEqual(before, after);
        Assert.Equal(1, distributedCache.GetCallCount);
    }

    private sealed class RecordingDistributedCache : IDistributedCache
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> _values = new();
        private int _getCallCount;

        public int GetCallCount => Volatile.Read(ref _getCallCount);

        public byte[]? Get(string key)
        {
            Interlocked.Increment(ref _getCallCount);
            return _values.TryGetValue(key, out var value) ? value : null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => _values.TryRemove(key, out _);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _values[key] = value;

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
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
