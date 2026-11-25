using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

internal sealed class EfAgencyDirectoryRepository(CompaniesAgenciesDbContext dbContext) : IAgencyDirectoryRepository
{
    public async Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default)
    {
        if (agencyIds.Count == 0)
        {
            return Array.Empty<PublicAgencySummary>();
        }

        return await (from agency in dbContext.Agencies.AsNoTracking()
                      join company in dbContext.Companies.AsNoTracking() on agency.CompanyId equals company.Id
                      where agencyIds.Contains(agency.Id)
                      select new PublicAgencySummary(
                          company.Id, company.Name, agency.Id, agency.Name, agency.City,
                          company.IsActive, agency.IsActive))
            .ToArrayAsync(cancellationToken);
    }
}
