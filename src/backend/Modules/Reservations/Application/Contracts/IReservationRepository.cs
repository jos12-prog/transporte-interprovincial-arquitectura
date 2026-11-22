using TransportPlatform.Modules.Reservations.Domain.Entities;

namespace TransportPlatform.Modules.Reservations.Application.Contracts;

public interface IReservationRepository
{
    Task<IReadOnlyList<TripSeat>?> GetTripSeatsAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task CreateHoldAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<bool> CancelHoldAsync(Guid reservationId, string holdTokenHash, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
    Task<int> ExpireHoldsAsync(DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}
