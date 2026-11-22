namespace TransportPlatform.Shared.Contracts.Companies;

public sealed record PublicAgencySummary(
    Guid CompanyId,
    string CompanyName,
    Guid AgencyId,
    string AgencyName,
    string City);
