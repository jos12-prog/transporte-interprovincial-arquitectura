using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.Reservations.Application.Services;

public sealed class ReservationsApi(ISeatAvailabilityService seatAvailabilityService) : IReservationsApi
{
    public Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(
        IReadOnlyCollection<Guid> tripIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tripIds);
        if (tripIds.Count == 0)
        {
            return Task.FromResult<IReadOnlyDictionary<Guid, int>>(new Dictionary<Guid, int>());
        }

        return seatAvailabilityService.GetAvailableSeatCountsAsync(tripIds.Distinct().ToArray(), cancellationToken);
    }
}
