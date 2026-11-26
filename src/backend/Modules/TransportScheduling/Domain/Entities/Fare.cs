namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class Fare
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid RouteId { get; private set; }
    public FareType Type { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "PEN";
    public bool IsActive { get; private set; } = true;

    private Fare() { }

    public Fare(Guid id, Guid companyId, Guid routeId, FareType type, DateOnly validFrom,
        DateOnly? validTo, decimal price, string currency = "PEN")
    {
        if (id == Guid.Empty || companyId == Guid.Empty || routeId == Guid.Empty)
        {
            throw new ArgumentException("Fare, company and route ids are required.");
        }

        ValidatePeriod(validFrom, validTo);
        ValidatePrice(price);
        Id = id;
        CompanyId = companyId;
        RouteId = routeId;
        Type = type;
        ValidFrom = validFrom;
        ValidTo = validTo;
        Price = price;
        Currency = Route.RequiredText(currency, nameof(currency)).ToUpperInvariant();
    }

    public bool AppliesTo(DateOnly date) => IsActive && date >= ValidFrom && (!ValidTo.HasValue || date <= ValidTo.Value);

    public static Fare? ResolveFor(DateOnly date, IEnumerable<Fare> fares)
    {
        ArgumentNullException.ThrowIfNull(fares);
        var eligible = fares.Where(fare => fare.AppliesTo(date)).ToArray();
        var special = eligible.Where(fare => fare.Type == FareType.Special).ToArray();
        if (special.Length > 1)
        {
            throw new InvalidOperationException("More than one special fare applies to the service date.");
        }
        if (special.Length == 1)
        {
            return special[0];
        }

        var baseFares = eligible.Where(fare => fare.Type == FareType.Base).ToArray();
        if (baseFares.Length > 1)
        {
            throw new InvalidOperationException("More than one base fare applies to the service date.");
        }
        return baseFares.SingleOrDefault();
    }

    public bool Overlaps(Fare other) =>
        other.RouteId == RouteId && other.Type == Type &&
        (!ValidTo.HasValue || other.ValidFrom <= ValidTo.Value) &&
        (!other.ValidTo.HasValue || ValidFrom <= other.ValidTo.Value);

    public void Update(DateOnly validFrom, DateOnly? validTo, decimal price, string currency)
    {
        ValidatePeriod(validFrom, validTo);
        ValidatePrice(price);
        ValidFrom = validFrom;
        ValidTo = validTo;
        Price = price;
        Currency = Route.RequiredText(currency, nameof(currency)).ToUpperInvariant();
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private static void ValidatePeriod(DateOnly from, DateOnly? to)
    {
        if (to.HasValue && to.Value < from)
        {
            throw new ArgumentException("Fare end date cannot precede start date.", nameof(to));
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Fare price must be greater than zero.");
        }
    }
}
