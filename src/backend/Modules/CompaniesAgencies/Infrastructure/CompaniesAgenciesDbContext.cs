using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

/// <summary>EF Core context for the CompaniesAgencies module.</summary>
public sealed class CompaniesAgenciesDbContext : DbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Agency> Agencies => Set<Agency>();

    public CompaniesAgenciesDbContext(DbContextOptions<CompaniesAgenciesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("companies_agencies");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompaniesAgenciesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
