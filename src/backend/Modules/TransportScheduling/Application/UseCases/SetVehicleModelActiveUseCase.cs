using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class SetVehicleModelActiveUseCase(IVehicleModelAdministrationRepository repository)
{
    public async Task<VehicleModel> ExecuteAsync(Guid vehicleModelId, bool isActive, CancellationToken cancellationToken = default)
    {
        var model = await repository.GetVehicleModelAsync(vehicleModelId, cancellationToken)
            ?? throw new VehicleModelNotFoundException("Vehicle model was not found.");
        model.SetActive(isActive);
        await repository.SaveVehicleModelAsync(model, cancellationToken);
        return model;
    }
}
