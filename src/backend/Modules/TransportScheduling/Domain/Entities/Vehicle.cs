namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid VehicleModelId { get; private set; }
    public string Plate { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Vehicle()
    {
    }

    public Vehicle(Guid id, Guid companyId, Guid agencyId, Guid vehicleModelId, string plate)
        : this(id, companyId, agencyId, vehicleModelId, plate, plate)
    {
    }

    public Vehicle(Guid id, Guid companyId, Guid agencyId, Guid vehicleModelId, string plate, string displayName)
    {
        if (id == Guid.Empty || companyId == Guid.Empty || agencyId == Guid.Empty || vehicleModelId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle and tenant identifiers are required.");
        }

        Id = id;
        CompanyId = companyId;
        AgencyId = agencyId;
        VehicleModelId = vehicleModelId;
        Plate = Route.RequiredText(plate, nameof(plate)).ToUpperInvariant();
        DisplayName = Route.RequiredText(displayName, nameof(displayName));
    }

    public void UpdateProfile(string plate, string displayName, Guid vehicleModelId)
    {
        if (vehicleModelId == Guid.Empty)
        {
            throw new ArgumentException("Vehicle model id is required.", nameof(vehicleModelId));
        }

        Plate = Route.RequiredText(plate, nameof(plate)).ToUpperInvariant();
        DisplayName = Route.RequiredText(displayName, nameof(displayName));
        VehicleModelId = vehicleModelId;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
