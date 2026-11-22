namespace TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

public sealed class Company
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Company()
    {
    }

    public Company(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Company id is required.", nameof(id));
        }

        Id = id;
        Name = RequireName(name, nameof(name));
    }

    internal static string RequireName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty name is required.", parameterName);
        }

        return value.Trim();
    }
}
