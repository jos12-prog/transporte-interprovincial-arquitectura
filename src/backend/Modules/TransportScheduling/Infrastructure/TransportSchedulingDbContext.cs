using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

/// <summary>EF Core context for the TransportScheduling module.</summary>
public sealed class TransportSchedulingDbContext : DbContext
{
    public TransportSchedulingDbContext(DbContextOptions<TransportSchedulingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("transport_scheduling");
        base.OnModelCreating(modelBuilder);
    }
}
