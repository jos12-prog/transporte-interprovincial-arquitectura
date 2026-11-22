using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Application.Contracts;

public interface ITripSearchReadRepository
{
    Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
        TripSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<SeatLayoutDto?> GetSeatLayoutAsync(Guid tripId, CancellationToken cancellationToken = default);
}
