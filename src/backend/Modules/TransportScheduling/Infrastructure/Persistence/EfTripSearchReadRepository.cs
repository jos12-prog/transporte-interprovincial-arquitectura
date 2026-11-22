using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

internal sealed class EfTripSearchReadRepository(TransportSchedulingDbContext dbContext) : ITripSearchReadRepository
{
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);

    public async Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
        TripSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        var localStart = new DateTimeOffset(criteria.ServiceDate.ToDateTime(TimeOnly.MinValue), LimaOffset);
        var fromUtc = localStart.ToUniversalTime();
        var untilUtc = localStart.AddDays(1).ToUniversalTime();

        var query = from trip in dbContext.Trips.AsNoTracking()
                    join route in dbContext.Routes.AsNoTracking() on trip.RouteId equals route.Id
                    join vehicle in dbContext.Vehicles.AsNoTracking() on trip.VehicleId equals vehicle.Id
                    join model in dbContext.VehicleModels.AsNoTracking() on vehicle.VehicleModelId equals model.Id
                    where EF.Functions.ILike(route.OriginCity, EscapeLikeLiteral(criteria.Origin), "\\")
                       && EF.Functions.ILike(route.DestinationCity, EscapeLikeLiteral(criteria.Destination), "\\")
                       && trip.DepartureAtUtc >= fromUtc
                       && trip.DepartureAtUtc < untilUtc
                    select new { trip, route, model };

        if (criteria.CompanyId.HasValue)
        {
            query = query.Where(item => item.trip.CompanyId == criteria.CompanyId.Value);
        }

        if (criteria.AgencyId.HasValue)
        {
            query = query.Where(item => item.trip.AgencyId == criteria.AgencyId.Value);
        }

        if (criteria.DepartureFrom.HasValue)
        {
            var departureFromUtc = new DateTimeOffset(
                criteria.ServiceDate.ToDateTime(criteria.DepartureFrom.Value), LimaOffset).ToUniversalTime();
            query = query.Where(item => item.trip.DepartureAtUtc >= departureFromUtc);
        }

        if (criteria.DepartureTo.HasValue)
        {
            var departureToUtc = new DateTimeOffset(
                criteria.ServiceDate.ToDateTime(criteria.DepartureTo.Value), LimaOffset).ToUniversalTime();
            query = query.Where(item => item.trip.DepartureAtUtc <= departureToUtc);
        }

        if (criteria.MinimumPrice.HasValue)
        {
            query = query.Where(item => item.trip.Price >= criteria.MinimumPrice.Value);
        }

        if (criteria.MaximumPrice.HasValue)
        {
            query = query.Where(item => item.trip.Price <= criteria.MaximumPrice.Value);
        }

        return await query.OrderBy(item => item.trip.DepartureAtUtc)
            .Select(item => new ScheduledTripSummary(
                item.trip.Id,
                item.trip.CompanyId,
                item.trip.AgencyId,
                item.route.OriginCity,
                item.route.DestinationCity,
                item.trip.DepartureAtUtc,
                item.trip.ArrivalAtUtc,
                item.model.Name,
                item.trip.Price,
                item.trip.Currency,
                0))
            .ToArrayAsync(cancellationToken);
    }

    private static string EscapeLikeLiteral(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal);
}
