namespace TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

public sealed class Company
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Company()
    {
    }

    public Company(Guid id, string name, string? taxId = null, string? contactName = null, string? contactPhone = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Company id is required.", nameof(id));
        }

        Id = id;
        Name = RequireName(name, nameof(name));
        TaxId = ValidateTaxId(taxId);
        ContactName = NormalizeOptional(contactName);
        ContactPhone = NormalizeOptional(contactPhone);
    }

    public void UpdateProfile(string name, string? taxId, string? contactName, string? contactPhone)
    {
        Name = RequireName(name, nameof(name));
        TaxId = ValidateTaxId(taxId);
        ContactName = NormalizeOptional(contactName);
        ContactPhone = NormalizeOptional(contactPhone);
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private static string? ValidateTaxId(string? value)
    {
        var normalized = NormalizeOptional(value);
        if (normalized is not null && (normalized.Length != 11 || normalized.Any(character => !char.IsDigit(character))))
        {
            throw new ArgumentException("Tax identifier must contain exactly 11 digits.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    internal static string RequireName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty name is required.", parameterName);
        }

        return value.Trim();
    }
}
