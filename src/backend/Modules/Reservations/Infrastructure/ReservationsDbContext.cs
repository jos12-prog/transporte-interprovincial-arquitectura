using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

/// <summary>EF Core context for the Reservations module.</summary>
public sealed class ReservationsDbContext : DbContext
{
    public ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reservations");
        base.OnModelCreating(modelBuilder);
    }
}
