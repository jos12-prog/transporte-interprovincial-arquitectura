using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;
using Xunit;

namespace TransportPlatform.Integration.Tests;

public sealed class ReservationSchemaTests
{
    [Fact]
    public async Task PostgreSqlModelCreatesPartialUniqueIndexForActiveReservations()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:16").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<ReservationsDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using var dbContext = new ReservationsDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var indexDefinition = await dbContext.Database.SqlQueryRaw<string>(
                "SELECT indexdef AS \"Value\" FROM pg_indexes WHERE schemaname = 'reservations' AND indexname = 'UX_reservation_seats_active_trip_seat'")
            .SingleAsync();

        Assert.Contains("UNIQUE", indexDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WHERE", indexDefinition, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Held", indexDefinition, StringComparison.Ordinal);
        Assert.Contains("Confirmed", indexDefinition, StringComparison.Ordinal);
    }
}
