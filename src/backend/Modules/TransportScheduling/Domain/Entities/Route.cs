namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class Route
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string OriginCity { get; private set; } = string.Empty;
    public string DestinationCity { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Route()
    {
    }

    public Route(Guid id, Guid companyId, string originCity, string destinationCity)
    {
        if (id == Guid.Empty || companyId == Guid.Empty)
        {
            throw new ArgumentException("Route and company ids are required.", nameof(id));
        }

        Id = id;
        CompanyId = companyId;
        OriginCity = RequiredText(originCity, nameof(originCity));
        DestinationCity = RequiredText(destinationCity, nameof(destinationCity));

        if (string.Equals(OriginCity, DestinationCity, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Origin and destination must be different.", nameof(destinationCity));
        }
    }

    public void Update(string originCity, string destinationCity)
    {
        var origin = RequiredText(originCity, nameof(originCity));
        var destination = RequiredText(destinationCity, nameof(destinationCity));
        if (string.Equals(origin, destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Origin and destination must be different.", nameof(destinationCity));
        }

        OriginCity = origin;
        DestinationCity = destination;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    internal static string RequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value.Trim();
    }
}
