using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.Contracts;

public interface IVehicleModelAdministrationRepository
{
    Task<IReadOnlyList<VehicleModel>> GetVehicleModelsAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetVehicleModelIdsInUseAsync(CancellationToken cancellationToken = default);
    Task<VehicleModel?> GetVehicleModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> VehicleModelNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task<bool> IsVehicleModelInUseAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddVehicleModelAsync(VehicleModel model, CancellationToken cancellationToken = default);
    Task SaveVehicleModelAsync(VehicleModel model, CancellationToken cancellationToken = default);
}
