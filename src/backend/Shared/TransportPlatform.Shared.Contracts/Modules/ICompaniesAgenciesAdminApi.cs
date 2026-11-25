using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Shared.Contracts.Modules;

public interface ICompaniesAgenciesAdminApi
{
    Task<IReadOnlyList<CompanyAdminDto>> GetCompaniesAsync(CancellationToken cancellationToken = default);
    Task<CompanyAdminDto?> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<CompanyAdminDto> CreateCompanyAsync(SaveCompanyRequest request, CancellationToken cancellationToken = default);
    Task<CompanyAdminDto?> UpdateCompanyAsync(Guid companyId, SaveCompanyRequest request, CancellationToken cancellationToken = default);
    Task<CompanyAdminDto?> SetCompanyActiveAsync(Guid companyId, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgencyAdminDto>> GetAgenciesAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<AgencyAdminDto?> GetAgencyAsync(Guid agencyId, CancellationToken cancellationToken = default);
    Task<AgencyAdminDto?> CreateAgencyAsync(Guid companyId, SaveAgencyRequest request, CancellationToken cancellationToken = default);
    Task<AgencyAdminDto?> UpdateAgencyAsync(Guid agencyId, SaveAgencyRequest request, CancellationToken cancellationToken = default);
    Task<AgencyAdminDto?> SetAgencyActiveAsync(Guid agencyId, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgencyCompanyStatus>> GetAgencyCompanyStatusesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default);
}
