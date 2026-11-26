using System.Data.Common;

namespace TransportPlatform.Shared.Contracts.Modules;

/// <summary>MI03 owns trip lifecycle; MI04 requests an active-trip row lock before changing seats.</summary>
public interface ITripReservationGuard
{
    Task<bool> LockActiveTripAsync(Guid tripId, DbTransaction transaction, CancellationToken cancellationToken = default);
}
