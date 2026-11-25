using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Shared.Contracts.Companies;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.Services;

public sealed class CompaniesAgenciesApi(
    IAgencyDirectoryRepository directoryRepository,
    ICompaniesAgenciesAdministrationRepository? administrationRepository = null,
    IQueriesReportsCacheControl? cacheControl = null) : ICompaniesAgenciesApi, ICompaniesAgenciesAdminApi
{
    private ICompaniesAgenciesAdministrationRepository AdminRepository => administrationRepository
        ?? throw new InvalidOperationException("Companies and agencies administration is not configured.");

    public async Task<IReadOnlyList<CompanyAdminDto>> GetCompaniesAsync(CancellationToken cancellationToken = default) =>
        (await AdminRepository.GetCompaniesAsync(cancellationToken)).Select(ToDto).ToArray();

    public async Task<CompanyAdminDto?> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        ToDtoOrNull(await AdminRepository.GetCompanyAsync(companyId, cancellationToken));

    public async Task<CompanyAdminDto> CreateCompanyAsync(SaveCompanyRequest request, CancellationToken cancellationToken = default) =>
        ToDto(await new SaveCompanyUseCase(AdminRepository).ExecuteAsync(null, request, cancellationToken));

    public async Task<CompanyAdminDto?> UpdateCompanyAsync(Guid companyId, SaveCompanyRequest request, CancellationToken cancellationToken = default) =>
        ToDtoOrNull(await new SaveCompanyUseCase(AdminRepository).ExecuteAsync(companyId, request, cancellationToken));

    public async Task<CompanyAdminDto?> SetCompanyActiveAsync(Guid companyId, bool isActive, CancellationToken cancellationToken = default)
    {
        var company = await new SetCompanyActiveUseCase(AdminRepository).ExecuteAsync(companyId, isActive, cancellationToken);
        if (cacheControl is not null) await cacheControl.InvalidateTripSearchAsync(cancellationToken);
        return ToDto(company);
    }

    public async Task<IReadOnlyList<AgencyAdminDto>> GetAgenciesAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        (await AdminRepository.GetAgenciesAsync(companyId, cancellationToken)).Select(ToDto).ToArray();

    public async Task<AgencyAdminDto?> GetAgencyAsync(Guid agencyId, CancellationToken cancellationToken = default) =>
        ToDtoOrNull(await AdminRepository.GetAgencyAsync(agencyId, cancellationToken));

    public async Task<AgencyAdminDto?> CreateAgencyAsync(Guid companyId, SaveAgencyRequest request, CancellationToken cancellationToken = default) =>
        ToDtoOrNull(await new SaveAgencyUseCase(AdminRepository).ExecuteAsync(null, companyId, request, cancellationToken));

    public async Task<AgencyAdminDto?> UpdateAgencyAsync(Guid agencyId, SaveAgencyRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await AdminRepository.GetAgencyAsync(agencyId, cancellationToken);
        if (existing is null) return null;
        return ToDto(await new SaveAgencyUseCase(AdminRepository)
            .ExecuteAsync(agencyId, existing.CompanyId, request, cancellationToken));
    }

    public async Task<AgencyAdminDto?> SetAgencyActiveAsync(Guid agencyId, bool isActive, CancellationToken cancellationToken = default)
    {
        var agency = await new SetAgencyActiveUseCase(AdminRepository).ExecuteAsync(agencyId, isActive, cancellationToken);
        if (cacheControl is not null) await cacheControl.InvalidateTripSearchAsync(cancellationToken);
        return ToDto(agency);
    }

    public Task<IReadOnlyList<AgencyCompanyStatus>> GetAgencyCompanyStatusesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default) =>
        AdminRepository.GetAgencyCompanyStatusesAsync(agencyIds, cancellationToken);

    public Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agencyIds);
        if (agencyIds.Count == 0) return Task.FromResult<IReadOnlyList<PublicAgencySummary>>(Array.Empty<PublicAgencySummary>());
        return directoryRepository.GetPublicAgenciesAsync(agencyIds.Distinct().ToArray(), cancellationToken);
    }

    private static CompanyAdminDto ToDto(Company company) => new(
        company.Id, company.Name, company.TaxId, company.ContactName, company.ContactPhone, company.IsActive);

    private static CompanyAdminDto? ToDtoOrNull(Company? company) => company is null ? null : ToDto(company);

    private static AgencyAdminDto ToDto(Agency agency) => new(
        agency.Id, agency.CompanyId, agency.Name, agency.City, agency.Address, agency.Phone, agency.IsActive);

    private static AgencyAdminDto? ToDtoOrNull(Agency? agency) => agency is null ? null : ToDto(agency);
}
