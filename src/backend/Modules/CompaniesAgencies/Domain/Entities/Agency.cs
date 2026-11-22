namespace TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

public sealed class Agency
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;

    private Agency()
    {
    }

    public Agency(Guid id, Guid companyId, string name, string city)
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
    }
}
