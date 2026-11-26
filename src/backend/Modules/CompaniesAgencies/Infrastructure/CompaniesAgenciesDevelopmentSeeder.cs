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

        var seededCompanies = companies.ToDictionary(item => item.Id);
        var seededCompanyIds = companies.Select(item => item.Id).ToArray();
        var existingCompanies = await db.Companies.Where(item => seededCompanyIds.Contains(item.Id)).ToListAsync(cancellationToken);
        foreach (var existing in existingCompanies)
        {
            var seed = seededCompanies[existing.Id];
            existing.UpdateProfile(seed.Name, seed.TaxId, seed.ContactName, seed.ContactPhone);
        }
        await db.SaveChangesAsync(cancellationToken);

        var agencyIds = (await db.Agencies.Select(item => item.Id).ToListAsync(cancellationToken)).ToHashSet();
        db.Agencies.AddRange(agencies.Where(item => !agencyIds.Contains(item.Id)));
        await db.SaveChangesAsync(cancellationToken);

        var seededAgencyIds = agencies.Select(item => item.Id).ToArray();
        var existingAgencies = await db.Agencies.Where(item => seededAgencyIds.Contains(item.Id)).ToListAsync(cancellationToken);
        var seededAgencies = agencies.ToDictionary(item => item.Id);
        foreach (var existing in existingAgencies)
        {
            var seed = seededAgencies[existing.Id];
            existing.UpdateProfile(seed.Name, seed.City, seed.Address, seed.Phone);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
