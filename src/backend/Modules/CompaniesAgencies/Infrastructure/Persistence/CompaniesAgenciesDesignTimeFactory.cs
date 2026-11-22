using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence;

public sealed class CompaniesAgenciesDesignTimeFactory : IDesignTimeDbContextFactory<CompaniesAgenciesDbContext>
{
    public CompaniesAgenciesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=transport;Username=design_time";
        var options = new DbContextOptionsBuilder<CompaniesAgenciesDbContext>()
            .UseNpgsql(connectionString, postgres =>
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", "companies_agencies"))
            .Options;
        return new CompaniesAgenciesDbContext(options);
    }
}
