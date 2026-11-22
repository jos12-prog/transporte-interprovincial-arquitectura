namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class Route
{
    public Guid Id { get; private set; }
    public string OriginCity { get; private set; } = string.Empty;
    public string DestinationCity { get; private set; } = string.Empty;

    private Route()
    {
    }

    public Route(Guid id, string originCity, string destinationCity)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Route id is required.", nameof(id));
        }

        Id = id;
        OriginCity = RequiredText(originCity, nameof(originCity));
        DestinationCity = RequiredText(destinationCity, nameof(destinationCity));

        if (string.Equals(OriginCity, DestinationCity, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Origin and destination must be different.", nameof(destinationCity));
        }
    }

    internal static string RequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value.Trim();
    }
}
