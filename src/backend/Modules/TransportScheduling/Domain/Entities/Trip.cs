namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class Trip
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid AgencyId { get; private set; }
    public Guid RouteId { get; private set; }
    public Guid VehicleId { get; private set; }
    public string OriginCitySnapshot { get; private set; } = string.Empty;
    public string DestinationCitySnapshot { get; private set; } = string.Empty;
    public string VehicleDisplayNameSnapshot { get; private set; } = string.Empty;
    public string VehicleModelNameSnapshot { get; private set; } = string.Empty;
    public DateTimeOffset DepartureAtUtc { get; private set; }
    public DateTimeOffset ArrivalAtUtc { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "PEN";
    public TripStatus Status { get; private set; } = TripStatus.Scheduled;

    private Trip()
    {
    }

    public Trip(
        Guid id,
        Guid companyId,
        Guid agencyId,
        Guid routeId,
        Guid vehicleId,
        DateTimeOffset departureAt,
        DateTimeOffset arrivalAt,
        decimal price,
        string currency = "PEN")
        : this(id, companyId, agencyId, routeId, vehicleId, departureAt, arrivalAt, price,
            string.Empty, string.Empty, string.Empty, string.Empty, currency)
    {
    }

    public Trip(
        Guid id,
        Guid companyId,
        Guid agencyId,
        Guid routeId,
        Guid vehicleId,
        DateTimeOffset departureAt,
        DateTimeOffset arrivalAt,
        decimal price,
        string originCity,
        string destinationCity,
        string vehicleDisplayName,
        string vehicleModelName,
        string currency = "PEN")
    {
        if (id == Guid.Empty || companyId == Guid.Empty || agencyId == Guid.Empty || routeId == Guid.Empty || vehicleId == Guid.Empty)
        {
            throw new ArgumentException("Trip and tenant identifiers are required.");
        }

        if (arrivalAt <= departureAt)
        {
            throw new ArgumentException("Arrival must be later than departure.", nameof(arrivalAt));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Trip price cannot be negative.");
        }

        Id = id;
        CompanyId = companyId;
        AgencyId = agencyId;
        RouteId = routeId;
        VehicleId = vehicleId;
        DepartureAtUtc = departureAt.ToUniversalTime();
        ArrivalAtUtc = arrivalAt.ToUniversalTime();
        Price = price;
        Currency = Route.RequiredText(currency, nameof(currency)).ToUpperInvariant();
        OriginCitySnapshot = string.IsNullOrWhiteSpace(originCity) ? string.Empty : originCity.Trim();
        DestinationCitySnapshot = string.IsNullOrWhiteSpace(destinationCity) ? string.Empty : destinationCity.Trim();
        VehicleDisplayNameSnapshot = string.IsNullOrWhiteSpace(vehicleDisplayName) ? string.Empty : vehicleDisplayName.Trim();
        VehicleModelNameSnapshot = string.IsNullOrWhiteSpace(vehicleModelName) ? string.Empty : vehicleModelName.Trim();
    }

    public void Cancel() => Status = TripStatus.Cancelled;
}
