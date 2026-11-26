using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class SaveVehicleUseCase(
    IAgencyCatalogRepository repository,
    IAgencyScopeValidator scopeValidator,
    IVehicleModelAdministrationRepository models)
{
    public async Task<Vehicle> ExecuteAsync(Guid? vehicleId, AgencyOperationScope scope, SaveVehicleInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        if (string.IsNullOrWhiteSpace(input.Plate) || string.IsNullOrWhiteSpace(input.DisplayName) || input.VehicleModelId == Guid.Empty)
            throw new AgencyCatalogValidationException("Plate, display name and vehicle model are required.");

        var plate = input.Plate.Trim().ToUpperInvariant();
        if (await repository.VehiclePlateExistsAsync(scope.CompanyId, plate, vehicleId, cancellationToken))
            throw new AgencyCatalogConflictException("A vehicle with this plate already exists in the company.");
        var model = await models.GetVehicleModelAsync(input.VehicleModelId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Vehicle model was not found.");
        if (!model.IsActive)
            throw new AgencyCatalogConflictException("An inactive vehicle model cannot be assigned to a new vehicle.");

        if (vehicleId is null)
        {
            var vehicle = new Vehicle(Guid.NewGuid(), scope.CompanyId, scope.AgencyId, model.Id, plate, input.DisplayName);
            await repository.AddVehicleAsync(vehicle, cancellationToken);
            return vehicle;
        }

        var existing = await repository.GetVehicleAsync(vehicleId.Value, scope.CompanyId, scope.AgencyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Vehicle was not found.");
        if (existing.VehicleModelId != input.VehicleModelId && await repository.VehicleHasTripsAsync(existing.Id, cancellationToken))
            throw new AgencyCatalogConflictException("A vehicle model cannot be changed after the vehicle has trips.");
        existing.UpdateProfile(plate, input.DisplayName, input.VehicleModelId);
        await repository.SaveVehicleAsync(existing, cancellationToken);
        return existing;
    }
}

public sealed class SetVehicleActiveUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<Vehicle> ExecuteAsync(Guid vehicleId, AgencyOperationScope scope, bool isActive, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        var vehicle = await repository.GetVehicleAsync(vehicleId, scope.CompanyId, scope.AgencyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Vehicle was not found.");
        vehicle.SetActive(isActive);
        await repository.SaveVehicleAsync(vehicle, cancellationToken);
        return vehicle;
    }
}
