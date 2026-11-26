using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Shared.Contracts.Modules;

/// <summary>Public MI04 commands used by MI03 to provision trip inventory and cancel reservations.</summary>
public interface IReservationsAdministrationApi
{
    Task ProvisionTripSeatsAsync(ProvisionTripSeatsRequest request, CancellationToken cancellationToken = default);
    Task<TripCancellationReservationResult> CancelTripAndReleaseHoldsAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task<TripReservationEligibility> GetTripReservationEligibilityAsync(Guid tripId, CancellationToken cancellationToken = default);
}
