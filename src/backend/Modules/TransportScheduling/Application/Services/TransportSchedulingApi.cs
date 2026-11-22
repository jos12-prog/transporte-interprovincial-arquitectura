using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Application.Services;

public sealed class TransportSchedulingApi(
    ITripSearchReadRepository tripSearchRepository,
    IReservationsApi reservationsApi) : ITransportSchedulingApi
{
    public async Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
        TripSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var trips = await tripSearchRepository.SearchTripsAsync(criteria, cancellationToken);
        if (trips.Count == 0)
        {
            return trips;
        }

        var counts = await reservationsApi.GetAvailableSeatCountsAsync(
            trips.Select(trip => trip.TripId).Distinct().ToArray(), cancellationToken);

        return trips.Select(trip => trip with
        {
            AvailableSeats = counts.GetValueOrDefault(trip.TripId)
        }).ToArray();
    }
}
