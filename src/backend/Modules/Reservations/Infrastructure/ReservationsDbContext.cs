using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.Reservations.Domain.Entities;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

/// <summary>EF Core context for the Reservations module.</summary>
public sealed class ReservationsDbContext : DbContext
{
    public DbSet<TripSeat> TripSeats => Set<TripSeat>();

    public ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reservations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
