using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Application.UseCases;
using TransportPlatform.Modules.Reservations.Domain.Enums;

namespace TransportPlatform.Modules.Reservations.Presentation;

public static class ReservationsEndpoints
{
    public static IEndpointRouteBuilder MapReservationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1").WithTags("Reservations");

        group.MapGet("/trips/{tripId:guid}/seats", async (
                Guid tripId,
                GetTripSeatMapUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var map = await useCase.ExecuteAsync(tripId, cancellationToken);
                    var response = new SeatMapResponse(map.TripId, map.Seats.Select(seat => new SeatMapSeatResponse(
                        seat.TripSeatId,
                        seat.Number,
                        seat.Row,
                        seat.Column,
                        seat.Floor,
                        seat.Status switch
                        {
                            TripSeatStatus.Available => "Available",
                            TripSeatStatus.Held => "Held",
                            TripSeatStatus.Confirmed => "Occupied",
                            _ => "Occupied"
                        })).ToArray());
                    return (IResult)Results.Ok(response);
                }
                catch (ReservationValidationException exception)
                {
                    return Problem(StatusCodes.Status400BadRequest, "Invalid request", exception.Message);
                }
                catch (ReservationNotFoundException exception)
                {
                    return Problem(StatusCodes.Status404NotFound, "Trip not found", exception.Message);
                }
            })
            .WithName("GetTripSeatMap")
            .WithSummary("Obtiene el mapa público de asientos de un viaje.")
            .Produces<SeatMapResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/reservations/holds", async (
                CreateReservationHoldRequest request,
                CreateReservationHoldUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    return (IResult)Results.Ok(await useCase.ExecuteAsync(request, cancellationToken));
                }
                catch (ReservationValidationException exception)
                {
                    return Problem(StatusCodes.Status400BadRequest, "Invalid request", exception.Message);
                }
                catch (ReservationNotFoundException exception)
                {
                    return Problem(StatusCodes.Status404NotFound, "Seat not found", exception.Message);
                }
                catch (ReservationConflictException exception)
                {
                    return Problem(StatusCodes.Status409Conflict, "Seat conflict", exception.Message);
                }
            })
            .WithName("CreateReservationHold")
            .WithSummary("Retiene entre uno y diez asientos durante diez minutos.")
            .Produces<ReservationHoldResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/reservations/holds/{id:guid}", async (
                Guid id,
                [FromHeader(Name = "X-Hold-Token")] string? holdToken,
                ReleaseReservationHoldUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await useCase.ExecuteAsync(id, holdToken, cancellationToken);
                    return (IResult)Results.NoContent();
                }
                catch (ReservationNotFoundException exception)
                {
                    return Problem(StatusCodes.Status404NotFound, "Hold not found", exception.Message);
                }
                catch (ReservationConflictException exception)
                {
                    return Problem(StatusCodes.Status409Conflict, "Hold conflict", exception.Message);
                }
            })
            .WithName("ReleaseReservationHold")
            .WithSummary("Libera los asientos de un hold usando el token del titular.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static IResult Problem(int statusCode, string title, string detail) =>
        Results.Problem(statusCode: statusCode, title: title, detail: detail);
}

public sealed record SeatMapResponse(Guid TripId, IReadOnlyList<SeatMapSeatResponse> Seats);

public sealed record SeatMapSeatResponse(Guid TripSeatId, int Number, int? Row, int? Column, int Floor, string Status);
