namespace TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

public sealed class Agency
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Agency()
    {
    }

    public Agency(Guid id, Guid companyId, string name, string city, string? address = null, string? phone = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Agency id is required.", nameof(id));
        }

        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Agency company id is required.", nameof(companyId));
        }

        Id = id;
        CompanyId = companyId;
        Name = Company.RequireName(name, nameof(name));
        City = Company.RequireName(city, nameof(city));
        Address = NormalizeOptional(address);
        Phone = NormalizeOptional(phone);
    }

    public void UpdateProfile(string name, string city, string? address, string? phone)
    {
        Name = Company.RequireName(name, nameof(name));
        City = Company.RequireName(city, nameof(city));
        Address = NormalizeOptional(address);
        Phone = NormalizeOptional(phone);
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
