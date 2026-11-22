using System.Security.Cryptography;
using System.Text;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Domain.Entities;

namespace TransportPlatform.Modules.Reservations.Application.UseCases;

public sealed class CreateReservationHoldUseCase(IReservationRepository repository, TimeProvider timeProvider)
{
    public async Task<ReservationHoldResult> ExecuteAsync(
        CreateReservationHoldRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.TripId == Guid.Empty)
        {
            throw new ReservationValidationException("A trip identifier is required.");
        }

        if (request.TripSeatIds is null || request.TripSeatIds.Count is < 1 or > Reservation.MaximumSeatsPerHold)
        {
            throw new ReservationValidationException($"A hold must contain between 1 and {Reservation.MaximumSeatsPerHold} seats.");
        }

        if (request.TripSeatIds.Any(id => id == Guid.Empty) || request.TripSeatIds.Distinct().Count() != request.TripSeatIds.Count)
        {
            throw new ReservationValidationException("Trip seat identifiers must be non-empty and unique.");
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var now = timeProvider.GetUtcNow();
        var tripSeats = await repository.GetTripSeatsAsync(request.TripId, cancellationToken)
            ?? throw new ReservationNotFoundException("Trip was not found.");
        var requestedIds = request.TripSeatIds.ToHashSet();
        var selectedSeats = tripSeats.Where(seat => requestedIds.Contains(seat.Id)).ToArray();
        if (selectedSeats.Length != request.TripSeatIds.Count)
        {
            throw new ReservationNotFoundException("One or more trip seats were not found.");
        }

        var companyId = selectedSeats[0].CompanyId;
        var agencyId = selectedSeats[0].AgencyId;
        if (selectedSeats.Any(seat => seat.CompanyId != companyId || seat.AgencyId != agencyId))
        {
            throw new ReservationNotFoundException("One or more trip seats were not found.");
        }

        var reservation = new Reservation(Guid.NewGuid(), companyId, agencyId,
            request.TripId, tokenHash, request.TripSeatIds, now);

        await repository.CreateHoldAsync(reservation, cancellationToken);
        return new ReservationHoldResult(reservation.Id, token, reservation.ExpiresAtUtc);
    }
}
