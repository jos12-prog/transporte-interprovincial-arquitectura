using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

internal sealed class EfCompaniesAgenciesAdministrationRepository(CompaniesAgenciesDbContext dbContext)
    : ICompaniesAgenciesAdministrationRepository
{
    public async Task<IReadOnlyList<Company>> GetCompaniesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Companies.AsNoTracking().OrderBy(company => company.Name).ToArrayAsync(cancellationToken);

    public Task<Company?> GetCompanyAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Companies.SingleOrDefaultAsync(company => company.Id == id, cancellationToken);

    public Task<bool> TaxIdExistsAsync(string taxId, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
        dbContext.Companies.AnyAsync(company => company.TaxId == taxId && (!excludingId.HasValue || company.Id != excludingId.Value), cancellationToken);

    public async Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default)
    {
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveCompanyAsync(Company company, CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<Agency>> GetAgenciesAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await dbContext.Agencies.AsNoTracking().Where(agency => agency.CompanyId == companyId)
            .OrderBy(agency => agency.Name).ToArrayAsync(cancellationToken);

    public Task<Agency?> GetAgencyAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Agencies.SingleOrDefaultAsync(agency => agency.Id == id, cancellationToken);

    public Task<bool> CompanyIsActiveAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        dbContext.Companies.AnyAsync(company => company.Id == companyId && company.IsActive, cancellationToken);

    public async Task<IReadOnlyList<AgencyCompanyStatus>> GetAgencyCompanyStatusesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default)
    {
        if (agencyIds.Count == 0) return Array.Empty<AgencyCompanyStatus>();
        return await (from agency in dbContext.Agencies.AsNoTracking()
                      join company in dbContext.Companies.AsNoTracking() on agency.CompanyId equals company.Id
                      where agencyIds.Contains(agency.Id)
                      select new AgencyCompanyStatus(agency.Id, company.Id, agency.IsActive, company.IsActive))
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAgencyAsync(Agency agency, CancellationToken cancellationToken = default)
    {
        dbContext.Agencies.Add(agency);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveAgencyAsync(Agency agency, CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
