using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Shared.Contracts.Modules;

public interface ITransportSchedulingAdminApi
{
    Task<IReadOnlyList<VehicleModelAdminDto>> GetVehicleModelsAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<VehicleModelAdminDto?> GetVehicleModelAsync(Guid vehicleModelId, CancellationToken cancellationToken = default);
    Task<VehicleModelAdminDto> CreateVehicleModelAsync(SaveVehicleModelRequest request, CancellationToken cancellationToken = default);
    Task<VehicleModelAdminDto?> UpdateVehicleModelAsync(Guid vehicleModelId, SaveVehicleModelRequest request, CancellationToken cancellationToken = default);
    Task<VehicleModelAdminDto?> SetVehicleModelActiveAsync(Guid vehicleModelId, bool isActive, CancellationToken cancellationToken = default);
}
