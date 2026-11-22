using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Redis;
using TransportPlatform.Modules.QueriesReports.Application.Contracts;
using TransportPlatform.Modules.QueriesReports.Application.UseCases;
using TransportPlatform.Modules.QueriesReports.Infrastructure.Caching;
using TransportPlatform.Shared.Contracts.Companies;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class TripSearchRedisCacheTests
{
    [Fact]
    public async Task SearchCacheReturnsMissThenHitAndExpiresEntries()
    {
        await using var redis = new RedisBuilder().WithImage("redis:7").Build();
        await redis.StartAsync();
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = $"{redis.Hostname}:{redis.GetMappedPublicPort(6379)},abortConnect=false");
        await using var provider = services.BuildServiceProvider();
        var cache = new RedisTripSearchCache(provider.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>());
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var trip = new ScheduledTripSummary(Guid.NewGuid(), companyId, agencyId, "Ayacucho", "Lima",
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(8),
            "Toyota Avanza", 60m, "PEN", 5);
        var scheduling = new FakeTransportSchedulingApi([trip]);
        var companies = new FakeCompaniesAgenciesApi(
            [new PublicAgencySummary(companyId, "Andes Express", agencyId, "Agencia Ayacucho", "Ayacucho")]);
        var useCase = new SearchTripsUseCase(scheduling, companies, cache);
        var request = new SearchTripsRequest("Ayacucho", "Lima", new DateOnly(2026, 11, 23));

        var miss = await useCase.ExecuteAsync(request);
        var hit = await useCase.ExecuteAsync(request);

        Assert.False(miss.CacheHit);
        Assert.True(hit.CacheHit);
        Assert.Equal(miss.Results, hit.Results);
        Assert.Equal(1, scheduling.CallCount);
        Assert.Equal(1, companies.CallCount);

        var expirationKey = $"trip-search-expiration:{Guid.NewGuid():N}";
        await cache.SetAsync(expirationKey, Array.Empty<TripSearchResult>(), TimeSpan.FromMilliseconds(150));
        await Task.Delay(TimeSpan.FromMilliseconds(400));
        Assert.Null(await cache.GetAsync(expirationKey));
    }

    [Fact]
    public async Task SearchFallsBackToSourceWhenRedisIsUnavailable()
    {
        var services = new ServiceCollection();
        services.AddStackExchangeRedisCache(options => options.Configuration =
            "127.0.0.1:1,abortConnect=false,connectRetry=0,connectTimeout=250,syncTimeout=500,asyncTimeout=500");
        await using var provider = services.BuildServiceProvider();
        ITripSearchCache cache = new RedisTripSearchCache(
            provider.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>());
        var scheduling = new FakeTransportSchedulingApi([]);
        var companies = new FakeCompaniesAgenciesApi([]);
        var useCase = new SearchTripsUseCase(scheduling, companies, cache);

        var result = await useCase.ExecuteAsync(new SearchTripsRequest("Ayacucho", "Lima", new DateOnly(2026, 11, 23)));

        Assert.False(result.CacheHit);
        Assert.Empty(result.Results);
        Assert.Equal(1, scheduling.CallCount);
    }

    private sealed class FakeTransportSchedulingApi(IReadOnlyList<ScheduledTripSummary> trips) : ITransportSchedulingApi
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
            TripSearchCriteria criteria,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(trips);
        }
    }

    private sealed class FakeCompaniesAgenciesApi(IReadOnlyList<PublicAgencySummary> agencies) : ICompaniesAgenciesApi
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
            IReadOnlyCollection<Guid> agencyIds,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(agencies);
        }
    }
}
