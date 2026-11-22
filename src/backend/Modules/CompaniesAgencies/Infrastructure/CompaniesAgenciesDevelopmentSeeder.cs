using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure;

public static class CompaniesAgenciesDevelopmentSeeder
{
    public static async Task SeedAsync(
        CompaniesAgenciesDbContext db,
        IReadOnlyCollection<Company> companies,
        IReadOnlyCollection<Agency> agencies,
        CancellationToken cancellationToken = default)
    {
        var companyIds = (await db.Companies.Select(item => item.Id).ToListAsync(cancellationToken)).ToHashSet();
        db.Companies.AddRange(companies.Where(item => !companyIds.Contains(item.Id)));
        await db.SaveChangesAsync(cancellationToken);

        var agencyIds = (await db.Agencies.Select(item => item.Id).ToListAsync(cancellationToken)).ToHashSet();
        db.Agencies.AddRange(agencies.Where(item => !agencyIds.Contains(item.Id)));
        await db.SaveChangesAsync(cancellationToken);
    }
}
