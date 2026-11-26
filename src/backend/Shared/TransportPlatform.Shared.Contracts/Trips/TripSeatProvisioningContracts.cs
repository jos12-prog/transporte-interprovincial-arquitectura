namespace TransportPlatform.Shared.Contracts.Trips;

public sealed record TripSeatDefinition(int Number, int? Row, int? Column, int Floor);
public sealed record ProvisionTripSeatsRequest(Guid TripId, Guid CompanyId, Guid AgencyId, IReadOnlyList<TripSeatDefinition> Seats);
public sealed record TripCancellationReservationResult(bool HasConfirmedSeats);
public sealed record TripReservationEligibility(bool Exists, bool IsScheduled);
