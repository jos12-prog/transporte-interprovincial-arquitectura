namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid VehicleModelId { get; private set; }
    public string Plate { get; private set; } = string.Empty;

    private Vehicle()
    {
    }

    public Vehicle(Guid id, Guid companyId, Guid agencyId, Guid vehicleModelId, string plate)
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
    }
}
