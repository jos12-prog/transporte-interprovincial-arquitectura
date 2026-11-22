using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

public sealed class TransportSchedulingDesignTimeFactory : IDesignTimeDbContextFactory<TransportSchedulingDbContext>
{
    public TransportSchedulingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=transport;Username=design_time";
        var options = new DbContextOptionsBuilder<TransportSchedulingDbContext>()
            .UseNpgsql(connectionString, postgres =>
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", "transport_scheduling"))
            .Options;
        return new TransportSchedulingDbContext(options);
    }
}
