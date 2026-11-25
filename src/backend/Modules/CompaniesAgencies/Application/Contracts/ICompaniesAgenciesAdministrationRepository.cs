using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;

public interface ICompaniesAgenciesAdministrationRepository
{
    Task<IReadOnlyList<Company>> GetCompaniesAsync(CancellationToken cancellationToken = default);
    Task<Company?> GetCompanyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> TaxIdExistsAsync(string taxId, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default);
    Task SaveCompanyAsync(Company company, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agency>> GetAgenciesAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Agency?> GetAgencyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CompanyIsActiveAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgencyCompanyStatus>> GetAgencyCompanyStatusesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default);
    Task AddAgencyAsync(Agency agency, CancellationToken cancellationToken = default);
    Task SaveAgencyAsync(Agency agency, CancellationToken cancellationToken = default);
}
