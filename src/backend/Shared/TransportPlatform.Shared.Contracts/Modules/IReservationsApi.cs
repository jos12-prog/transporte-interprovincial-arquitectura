namespace TransportPlatform.Shared.Contracts.Modules;

public interface IReservationsApi
{
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(
        IReadOnlyCollection<Guid> tripIds,
        CancellationToken cancellationToken = default);
}
