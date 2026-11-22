using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Application.Services;
using TransportPlatform.Modules.QueriesReports.Application.Contracts;
using TransportPlatform.Modules.QueriesReports.Application.UseCases;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Application.Services;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.Services;
using TransportPlatform.Shared.Contracts.Companies;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class TripSearchApplicationTests
{
    [Fact]
    public async Task SearchUseCaseNormalizesCriteriaAndEnrichesPublicAgencyData()
    {
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var trip = new ScheduledTripSummary(Guid.NewGuid(), companyId, agencyId, "Ayacucho", "Lima",
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(8),
            "Toyota Avanza", 60m, "PEN", 5);
        var scheduling = new FakeTransportSchedulingApi([trip]);
        var companies = new FakeCompaniesAgenciesApi(
        [
            new PublicAgencySummary(companyId, "Ruta Sur", agencyId, "Terminal Centro", "Ayacucho")
        ]);
        var cache = new FakeTripSearchCache();
        var useCase = new SearchTripsUseCase(scheduling, companies, cache);

        var result = await useCase.ExecuteAsync(new SearchTripsRequest(
            " Ayacucho ", " Lima ", new DateOnly(2026, 11, 23),
            DepartureFrom: new TimeOnly(18, 0), MinimumPrice: 20m, MaximumPrice: 90m));

        Assert.False(result.CacheHit);
        Assert.Single(result.Results);
        Assert.Equal("Ruta Sur", result.Results[0].CompanyName);
        Assert.Equal("Terminal Centro", result.Results[0].AgencyName);
        Assert.Equal(5, result.Results[0].AvailableSeats);
        Assert.Equal(TimeSpan.FromSeconds(60), cache.LastTtl);
        Assert.Equal("Ayacucho", scheduling.Criteria!.Origin);
        Assert.Equal("Lima", scheduling.Criteria.Destination);
        Assert.Equal(new[] { agencyId }, companies.RequestedAgencyIds);
    }

    [Fact]
    public async Task SearchUseCaseRejectsInvalidCriteriaBeforeCallingModuleApis()
    {
        var scheduling = new FakeTransportSchedulingApi([]);
        var companies = new FakeCompaniesAgenciesApi([]);
        var useCase = new SearchTripsUseCase(scheduling, companies, new FakeTripSearchCache());

        var exception = await Assert.ThrowsAsync<SearchTripsValidationException>(() =>
            useCase.ExecuteAsync(new SearchTripsRequest("Ayacucho", " ayacucho ", null,
                MinimumPrice: 100m, MaximumPrice: 10m)));

        Assert.Contains(nameof(SearchTripsRequest.Destination), exception.Errors.Keys);
        Assert.Contains(nameof(SearchTripsRequest.ServiceDate), exception.Errors.Keys);
        Assert.Contains(nameof(SearchTripsRequest.MaximumPrice), exception.Errors.Keys);
        Assert.Null(scheduling.Criteria);
        Assert.Null(companies.RequestedAgencyIds);
    }

    [Fact]
    public async Task SearchUseCaseReturnsEmptyResultsWithoutAgencyLookup()
    {
        var scheduling = new FakeTransportSchedulingApi([]);
        var companies = new FakeCompaniesAgenciesApi([]);
        var useCase = new SearchTripsUseCase(scheduling, companies, new FakeTripSearchCache());

        var results = await useCase.ExecuteAsync(new SearchTripsRequest(
            "Ayacucho", "Lima", new DateOnly(2026, 11, 23)));

        Assert.Empty(results.Results);
        Assert.False(results.CacheHit);
        Assert.Null(companies.RequestedAgencyIds);
    }

    [Fact]
    public async Task SearchUseCaseReturnsCachedAggregateWithoutCallingModuleApisAgain()
    {
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var trip = new ScheduledTripSummary(Guid.NewGuid(), companyId, agencyId, "Ayacucho", "Lima",
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(8),
            "Toyota Avanza", 60m, "PEN", 5);
        var scheduling = new FakeTransportSchedulingApi([trip]);
        var companies = new FakeCompaniesAgenciesApi(
            [new PublicAgencySummary(companyId, "Ruta Sur", agencyId, "Terminal Centro", "Ayacucho")]);
        var cache = new FakeTripSearchCache();
        var useCase = new SearchTripsUseCase(scheduling, companies, cache);
        var date = new DateOnly(2026, 11, 23);

        var miss = await useCase.ExecuteAsync(new SearchTripsRequest(" Ayacucho ", "Lima", date));
        var hit = await useCase.ExecuteAsync(new SearchTripsRequest("AYACUCHO", "LIMA", date));

        Assert.False(miss.CacheHit);
        Assert.True(hit.CacheHit);
        Assert.Equal(miss.Results, hit.Results);
        Assert.Equal("Ayacucho", scheduling.Criteria!.Origin);
        Assert.Equal(1, scheduling.CallCount);
        Assert.Equal(1, companies.CallCount);
    }

    [Fact]
    public async Task SearchUseCaseFallsBackToModuleApisWhenCacheReadFails()
    {
        var scheduling = new FakeTransportSchedulingApi([]);
        var companies = new FakeCompaniesAgenciesApi([]);
        var useCase = new SearchTripsUseCase(scheduling, companies, new FakeTripSearchCache { ThrowOnGet = true });

        var result = await useCase.ExecuteAsync(new SearchTripsRequest("Ayacucho", "Lima", new DateOnly(2026, 11, 23)));

        Assert.False(result.CacheHit);
        Assert.Empty(result.Results);
        Assert.NotNull(scheduling.Criteria);
    }

    [Fact]
    public async Task TransportSchedulingApiLoadsAvailabilityInOneBatch()
    {
        var tripId = Guid.NewGuid();
        var candidate = new ScheduledTripSummary(tripId, Guid.NewGuid(), Guid.NewGuid(), "Ayacucho", "Lima",
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(1).AddHours(8),
            "Bus 1 piso", 80m, "PEN", 0);
        var repository = new FakeTripSearchReadRepository([candidate]);
        var reservations = new FakeReservationsApi(new Dictionary<Guid, int> { [tripId] = 22 });
        var api = new TransportSchedulingApi(repository, reservations);

        var result = await api.SearchTripsAsync(new TripSearchCriteria("Ayacucho", "Lima", new DateOnly(2026, 11, 23)));

        Assert.Equal(22, Assert.Single(result).AvailableSeats);
        Assert.Equal(new[] { tripId }, reservations.RequestedTripIds);
    }

    [Fact]
    public async Task ModuleApisDeduplicateRequestedIdsAndReturnEmptyWithoutRepositoryCalls()
    {
        var companyId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var agencyRepository = new FakeAgencyDirectoryRepository(
            [new PublicAgencySummary(companyId, "Ruta Sur", agencyId, "Terminal Centro", "Ayacucho")]);
        var companyApi = new CompaniesAgenciesApi(agencyRepository);
        var agencies = await companyApi.GetPublicAgenciesAsync([agencyId, agencyId]);

        Assert.Single(agencies);
        Assert.Equal(new[] { agencyId }, agencyRepository.RequestedAgencyIds);
        Assert.Empty(await companyApi.GetPublicAgenciesAsync([]));

        var tripId = Guid.NewGuid();
        var availability = new FakeSeatAvailabilityService(new Dictionary<Guid, int> { [tripId] = 4 });
        var reservationsApi = new ReservationsApi(availability);
        var counts = await reservationsApi.GetAvailableSeatCountsAsync([tripId, tripId]);

        Assert.Equal(4, counts[tripId]);
        Assert.Equal(new[] { tripId }, availability.RequestedTripIds);
        Assert.Empty(await reservationsApi.GetAvailableSeatCountsAsync([]));
    }

    private sealed class FakeTransportSchedulingApi(IReadOnlyList<ScheduledTripSummary> trips) : ITransportSchedulingApi
    {
        public TripSearchCriteria? Criteria { get; private set; }
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
            TripSearchCriteria criteria,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            Criteria = criteria;
            return Task.FromResult(trips);
        }
    }

    private sealed class FakeCompaniesAgenciesApi(IReadOnlyList<PublicAgencySummary> agencies) : ICompaniesAgenciesApi
    {
        public IReadOnlyCollection<Guid>? RequestedAgencyIds { get; private set; }
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
            IReadOnlyCollection<Guid> agencyIds,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            RequestedAgencyIds = agencyIds;
            return Task.FromResult(agencies);
        }
    }

    private sealed class FakeTripSearchCache : ITripSearchCache
    {
        private readonly Dictionary<string, IReadOnlyList<TripSearchResult>> _values = new(StringComparer.Ordinal);

        public bool ThrowOnGet { get; init; }
        public TimeSpan? LastTtl { get; private set; }

        public Task<IReadOnlyList<TripSearchResult>?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (ThrowOnGet)
            {
                throw new InvalidOperationException("Simulated Redis failure.");
            }

            _values.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task SetAsync(
            string key,
            IReadOnlyList<TripSearchResult> results,
            TimeSpan timeToLive,
            CancellationToken cancellationToken = default)
        {
            LastTtl = timeToLive;
            _values[key] = results;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTripSearchReadRepository(IReadOnlyList<ScheduledTripSummary> trips) : ITripSearchReadRepository
    {
        public Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
            TripSearchCriteria criteria,
            CancellationToken cancellationToken = default) => Task.FromResult(trips);
    }

    private sealed class FakeReservationsApi(IReadOnlyDictionary<Guid, int> counts) : IReservationsApi
    {
        public IReadOnlyCollection<Guid>? RequestedTripIds { get; private set; }

        public Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(
            IReadOnlyCollection<Guid> tripIds,
            CancellationToken cancellationToken = default)
        {
            RequestedTripIds = tripIds;
            return Task.FromResult(counts);
        }
    }

    private sealed class FakeAgencyDirectoryRepository(IReadOnlyList<PublicAgencySummary> agencies)
        : IAgencyDirectoryRepository
    {
        public IReadOnlyCollection<Guid>? RequestedAgencyIds { get; private set; }

        public Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
            IReadOnlyCollection<Guid> agencyIds,
            CancellationToken cancellationToken = default)
        {
            RequestedAgencyIds = agencyIds;
            return Task.FromResult(agencies);
        }
    }

    private sealed class FakeSeatAvailabilityService(IReadOnlyDictionary<Guid, int> counts)
        : ISeatAvailabilityService
    {
        public IReadOnlyCollection<Guid>? RequestedTripIds { get; private set; }

        public Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(
            IReadOnlyCollection<Guid> tripIds,
            CancellationToken cancellationToken = default)
        {
            RequestedTripIds = tripIds;
            return Task.FromResult(counts);
        }
    }
}
