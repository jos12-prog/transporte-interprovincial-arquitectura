using System.Security.Cryptography;
using System.Text;
using TransportPlatform.Modules.Reservations.Application.Contracts;

namespace TransportPlatform.Modules.Reservations.Application.UseCases;

public sealed class ReleaseReservationHoldUseCase(IReservationRepository repository, TimeProvider timeProvider)
{
    public async Task ExecuteAsync(Guid reservationId, string? holdToken, CancellationToken cancellationToken = default)
    {
        if (reservationId == Guid.Empty || string.IsNullOrWhiteSpace(holdToken))
        {
            throw new ReservationNotFoundException("Hold was not found.");
        }

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(holdToken)));
        var released = await repository.CancelHoldAsync(reservationId, tokenHash, timeProvider.GetUtcNow(), cancellationToken);
        if (!released)
        {
            throw new ReservationNotFoundException("Hold was not found or token is invalid.");
        }
    }
}
