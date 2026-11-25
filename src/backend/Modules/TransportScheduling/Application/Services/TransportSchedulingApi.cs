using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.UseCases;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Application.Services;

public sealed class TransportSchedulingApi(
    ITripSearchReadRepository tripSearchRepository,
    IReservationsApi reservationsApi,
    IVehicleModelAdministrationRepository? vehicleModelRepository = null) : ITransportSchedulingApi, ITransportSchedulingAdminApi
{
    private IVehicleModelAdministrationRepository AdminRepository => vehicleModelRepository
        ?? throw new InvalidOperationException("Vehicle model administration is not configured.");

    public async Task<IReadOnlyList<VehicleModelAdminDto>> GetVehicleModelsAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var models = await AdminRepository.GetVehicleModelsAsync(activeOnly, cancellationToken);
        var inUseIds = (await AdminRepository.GetVehicleModelIdsInUseAsync(cancellationToken)).ToHashSet();
        return models.Select(model => ToDto(model, inUseIds.Contains(model.Id))).ToArray();
    }

    public async Task<VehicleModelAdminDto?> GetVehicleModelAsync(Guid vehicleModelId, CancellationToken cancellationToken = default)
    {
        var model = await AdminRepository.GetVehicleModelAsync(vehicleModelId, cancellationToken);
        return model is null ? null : await ToDtoAsync(model, cancellationToken);
    }

    public async Task<VehicleModelAdminDto> CreateVehicleModelAsync(SaveVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        var model = await new SaveVehicleModelUseCase(AdminRepository).ExecuteAsync(null, request, cancellationToken);
        return await ToDtoAsync(model, cancellationToken);
    }

    public async Task<VehicleModelAdminDto?> UpdateVehicleModelAsync(Guid vehicleModelId, SaveVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        var model = await new SaveVehicleModelUseCase(AdminRepository).ExecuteAsync(vehicleModelId, request, cancellationToken);
        return await ToDtoAsync(model, cancellationToken);
    }

    public async Task<VehicleModelAdminDto?> SetVehicleModelActiveAsync(Guid vehicleModelId, bool isActive, CancellationToken cancellationToken = default)
    {
        var model = await new SetVehicleModelActiveUseCase(AdminRepository).ExecuteAsync(vehicleModelId, isActive, cancellationToken);
        return await ToDtoAsync(model, cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduledTripSummary>> SearchTripsAsync(
        TripSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var trips = await tripSearchRepository.SearchTripsAsync(criteria, cancellationToken);
        if (trips.Count == 0)
        {
            return trips;
        }

        var counts = await reservationsApi.GetAvailableSeatCountsAsync(
            trips.Select(trip => trip.TripId).Distinct().ToArray(), cancellationToken);

        return trips.Select(trip => trip with
        {
            AvailableSeats = counts.GetValueOrDefault(trip.TripId)
        }).ToArray();
    }

    public Task<SeatLayoutDto?> GetSeatLayoutAsync(Guid tripId, CancellationToken cancellationToken = default) =>
        tripSearchRepository.GetSeatLayoutAsync(tripId, cancellationToken);

    private async Task<VehicleModelAdminDto> ToDtoAsync(VehicleModel model, CancellationToken cancellationToken) =>
        ToDto(model, await AdminRepository.IsVehicleModelInUseAsync(model.Id, cancellationToken));

    private static VehicleModelAdminDto ToDto(VehicleModel model, bool inUse) => new(
            model.Id,
            model.Name,
            model.Category,
            model.Capacity,
            model.SeatLayout.Seats.Select(seat => new SeatPositionContract(seat.Number, seat.Floor, seat.Row, seat.Column)).ToArray(),
            model.IsActive,
            inUse);
}
