using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

/// <summary>EF Core context for the TransportScheduling module.</summary>
public sealed class TransportSchedulingDbContext : DbContext
{
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Trip> Trips => Set<Trip>();

    public TransportSchedulingDbContext(DbContextOptions<TransportSchedulingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("transport_scheduling");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransportSchedulingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
