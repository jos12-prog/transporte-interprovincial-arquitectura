using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

public sealed class ReservationsDesignTimeFactory : IDesignTimeDbContextFactory<ReservationsDbContext>
{
    public ReservationsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=transport;Username=design_time";
        var options = new DbContextOptionsBuilder<ReservationsDbContext>()
            .UseNpgsql(connectionString, postgres =>
                postgres.MigrationsHistoryTable("__EFMigrationsHistory", "reservations"))
            .Options;
        return new ReservationsDbContext(options);
    }
}
