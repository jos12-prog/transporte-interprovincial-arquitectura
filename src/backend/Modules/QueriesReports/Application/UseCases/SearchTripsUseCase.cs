using System.Security.Cryptography;
using System.Text.Json;
using TransportPlatform.Modules.QueriesReports.Application.Contracts;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.QueriesReports.Application.UseCases;

public sealed class SearchTripsUseCase(
    ITransportSchedulingApi transportSchedulingApi,
    ICompaniesAgenciesApi companiesAgenciesApi,
    ITripSearchCache tripSearchCache) : ISearchTripsUseCase
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public async Task<TripSearchExecutionResult> ExecuteAsync(
        SearchTripsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validate(request);

        var normalizedRequest = request with
        {
            Origin = request.Origin!.Trim(),
            Destination = request.Destination!.Trim()
        };
        var cacheGeneration = await tripSearchCache.GetGenerationAsync(cancellationToken);
        var cacheKey = CreateCacheKey(normalizedRequest, cacheGeneration);
        try
        {
            var cached = await tripSearchCache.GetAsync(cacheKey, cancellationToken);
            if (cached is not null)
            {
                return new TripSearchExecutionResult(cached, CacheHit: true);
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Redis is an optimization; continue from the module-owned sources when it is unavailable.
        }

        var criteria = new TripSearchCriteria(
            normalizedRequest.Origin!,
            normalizedRequest.Destination!,
            normalizedRequest.ServiceDate!.Value,
            normalizedRequest.CompanyId,
            normalizedRequest.AgencyId,
            normalizedRequest.DepartureFrom,
            normalizedRequest.DepartureTo,
            normalizedRequest.MinimumPrice,
            normalizedRequest.MaximumPrice);

        var trips = await transportSchedulingApi.SearchTripsAsync(criteria, cancellationToken);
        if (trips.Count == 0)
        {
            var empty = Array.Empty<TripSearchResult>();
            await TryCacheAsync(cacheKey, empty, cancellationToken);
            return new TripSearchExecutionResult(empty, CacheHit: false);
        }

        var agencies = await companiesAgenciesApi.GetPublicAgenciesAsync(
            trips.Select(trip => trip.AgencyId).Distinct().ToArray(), cancellationToken);
        var agenciesById = agencies.ToDictionary(agency => agency.AgencyId);

        var results = trips.Where(trip =>
        {
            if (!agenciesById.TryGetValue(trip.AgencyId, out var agency) || agency.CompanyId != trip.CompanyId)
            {
                throw new InvalidOperationException("A scheduled trip refers to an unavailable public agency.");
            }

            return agency.CompanyIsActive && agency.AgencyIsActive;
        }).Select(trip =>
        {
            var agency = agenciesById[trip.AgencyId];

            return new TripSearchResult(
                trip.TripId,
                trip.CompanyId,
                agency.CompanyName,
                trip.AgencyId,
                agency.AgencyName,
                agency.City,
                trip.Origin,
                trip.Destination,
                trip.DepartureAtUtc,
                trip.ArrivalAtUtc,
                trip.VehicleModelName,
                trip.Price,
                trip.Currency,
                trip.AvailableSeats,
                trip.VehicleDisplayName);
        }).ToArray();
        await TryCacheAsync(cacheKey, results, cancellationToken);
        return new TripSearchExecutionResult(results, CacheHit: false);
    }

    private async Task TryCacheAsync(
        string key,
        IReadOnlyList<TripSearchResult> results,
        CancellationToken cancellationToken)
    {
        try
        {
            await tripSearchCache.SetAsync(key, results, CacheTtl, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // A cache write failure must not fail an otherwise successful public search.
        }
    }

    private static string CreateCacheKey(SearchTripsRequest request, string generation)
    {
        var canonicalCriteria = new
        {
            Origin = request.Origin!.Trim().ToUpperInvariant(),
            Destination = request.Destination!.Trim().ToUpperInvariant(),
            request.ServiceDate,
            request.CompanyId,
            request.AgencyId,
            DepartureFrom = request.DepartureFrom?.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            DepartureTo = request.DepartureTo?.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            MinimumPrice = request.MinimumPrice?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture),
            MaximumPrice = request.MaximumPrice?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)
        };
        var digest = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(canonicalCriteria));
        return $"trip-search:{generation}:{Convert.ToHexString(digest)}";
    }

    private static void Validate(SearchTripsRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        AddIf(string.IsNullOrWhiteSpace(request.Origin), nameof(request.Origin), "Origin is required.");
        AddIf(string.IsNullOrWhiteSpace(request.Destination), nameof(request.Destination), "Destination is required.");
        AddIf(request.ServiceDate is null, nameof(request.ServiceDate), "Service date is required.");

        var origin = request.Origin?.Trim();
        var destination = request.Destination?.Trim();
        AddIf(!string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(destination)
            && string.Equals(origin, destination, StringComparison.OrdinalIgnoreCase),
            nameof(request.Destination), "Origin and destination must be different.");
        AddIf(request.CompanyId == Guid.Empty, nameof(request.CompanyId), "Company id is invalid.");
        AddIf(request.AgencyId == Guid.Empty, nameof(request.AgencyId), "Agency id is invalid.");
        AddIf(request.DepartureFrom.HasValue && request.DepartureTo.HasValue
            && request.DepartureFrom > request.DepartureTo,
            nameof(request.DepartureTo), "Departure end time must not precede its start time.");
        AddIf(request.MinimumPrice < 0, nameof(request.MinimumPrice), "Minimum price cannot be negative.");
        AddIf(request.MaximumPrice < 0, nameof(request.MaximumPrice), "Maximum price cannot be negative.");
        AddIf(request.MinimumPrice.HasValue && request.MaximumPrice.HasValue
            && request.MinimumPrice > request.MaximumPrice,
            nameof(request.MaximumPrice), "Maximum price must not be below minimum price.");

        if (errors.Count > 0)
        {
            throw new SearchTripsValidationException(errors);
        }

        void AddIf(bool condition, string field, string message)
        {
            if (condition)
            {
                errors[field] = [message];
            }
        }
    }
}
