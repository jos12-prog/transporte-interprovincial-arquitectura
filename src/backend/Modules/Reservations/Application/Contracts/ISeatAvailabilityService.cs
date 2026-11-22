namespace TransportPlatform.Modules.Reservations.Application.Contracts;

public interface ISeatAvailabilityService
{
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(
        IReadOnlyCollection<Guid> tripIds,
        CancellationToken cancellationToken = default);
}
