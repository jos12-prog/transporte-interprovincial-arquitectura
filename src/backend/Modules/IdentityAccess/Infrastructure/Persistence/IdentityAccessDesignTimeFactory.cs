using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

public sealed class IdentityAccessDesignTimeFactory : IDesignTimeDbContextFactory<IdentityAccessDbContext>
{
    public IdentityAccessDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=transport;Username=transport;Password=design-time-only";
        var options = new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(connectionString).Options;
        return new IdentityAccessDbContext(options);
    }
}
