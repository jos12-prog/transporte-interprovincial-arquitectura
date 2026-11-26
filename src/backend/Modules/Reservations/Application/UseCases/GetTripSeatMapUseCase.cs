using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Domain.Enums;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.Reservations.Application.UseCases;

public sealed class GetTripSeatMapUseCase(
    ITransportSchedulingApi schedulingApi,
    IReservationRepository reservationRepository)
{
    public async Task<TripSeatMap> ExecuteAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        if (tripId == Guid.Empty)
        {
            throw new ReservationValidationException("A trip id is required.");
        }

        var layout = await schedulingApi.GetSeatLayoutAsync(tripId, cancellationToken)
            ?? throw new ReservationNotFoundException("Trip was not found.");
        var tripSeats = await reservationRepository.GetTripSeatsAsync(tripId, cancellationToken)
            ?? throw new ReservationNotFoundException("Trip seats were not found.");
        if (tripSeats.Count == 0)
        {
            throw new ReservationNotFoundException("Trip seats were not found.");
        }
        var byNumber = tripSeats.ToDictionary(seat => seat.SeatNumber);

        return new TripSeatMap(tripId, layout.Seats.Select(position =>
        {
            if (!byNumber.TryGetValue(position.Number, out var tripSeat))
            {
                throw new ReservationNotFoundException("Trip seat map is incomplete.");
            }

            return new SeatMapSeat(tripSeat.Id, position.Number, position.Row, position.Column, position.Floor, tripSeat.Status);
        }).ToArray(), layout.VehicleDisplayName);
    }
}
