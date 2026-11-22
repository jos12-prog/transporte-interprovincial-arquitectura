using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Shared.Contracts.Modules;

public interface ITransportSchedulingApi
{
    Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
        TripSearchCriteria criteria,
        CancellationToken cancellationToken = default);
}
