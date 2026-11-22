using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

/// <summary>EF Core context for the CompaniesAgencies module.</summary>
public sealed class CompaniesAgenciesDbContext : DbContext
{
    public CompaniesAgenciesDbContext(DbContextOptions<CompaniesAgenciesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("companies_agencies");
        base.OnModelCreating(modelBuilder);
    }
}
