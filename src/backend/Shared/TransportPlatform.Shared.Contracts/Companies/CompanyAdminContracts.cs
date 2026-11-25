namespace TransportPlatform.Shared.Contracts.Companies;

public sealed record CompanyAdminDto(
    Guid CompanyId,
    string Name,
    string? TaxId,
    string? ContactName,
    string? ContactPhone,
    bool IsActive);

public sealed record SaveCompanyRequest(
    string Name,
    string? TaxId,
    string? ContactName,
    string? ContactPhone);

public sealed record AgencyAdminDto(
    Guid AgencyId,
    Guid CompanyId,
    string Name,
    string City,
    string? Address,
    string? Phone,
    bool IsActive);

public sealed record SaveAgencyRequest(
    string Name,
    string City,
    string? Address,
    string? Phone);

public sealed record AgencyCompanyStatus(
    Guid AgencyId,
    Guid CompanyId,
    bool AgencyIsActive,
    bool CompanyIsActive);
