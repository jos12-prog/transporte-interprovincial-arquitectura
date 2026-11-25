using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class SaveVehicleModelUseCase(IVehicleModelAdministrationRepository repository)
{
    public async Task<VehicleModel> ExecuteAsync(Guid? vehicleModelId, SaveVehicleModelRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name)) throw new VehicleModelValidationException("Vehicle model name is required.");
        if (request.Seats is null || request.Seats.Count == 0) throw new VehicleModelValidationException("At least one seat is required.");
        if (await repository.VehicleModelNameExistsAsync(request.Name.Trim(), vehicleModelId, cancellationToken))
            throw new VehicleModelConflictException("A vehicle model with this name already exists.");

        SeatLayout layout;
        try
        {
            layout = new SeatLayout(request.Seats.Select(seat => new SeatPosition(seat.Number, seat.Row, seat.Column, seat.Floor)));
        }
        catch (ArgumentException exception)
        {
            throw new VehicleModelValidationException(exception.Message);
        }

        if (vehicleModelId is null)
        {
            var model = new VehicleModel(Guid.NewGuid(), request.Name, layout.Capacity, layout, request.Category);
            await repository.AddVehicleModelAsync(model, cancellationToken);
            return model;
        }

        var existing = await repository.GetVehicleModelAsync(vehicleModelId.Value, cancellationToken)
            ?? throw new VehicleModelNotFoundException("Vehicle model was not found.");
        if (await repository.IsVehicleModelInUseAsync(existing.Id, cancellationToken)
            && (existing.Capacity != layout.Capacity || !existing.SeatLayout.Seats.SequenceEqual(layout.Seats)))
        {
            throw new VehicleModelConflictException("The seat layout of a vehicle model in use cannot be changed.");
        }

        existing.UpdateProfile(request.Name, layout.Capacity, layout, request.Category);
        await repository.SaveVehicleModelAsync(existing, cancellationToken);
        return existing;
    }
}
