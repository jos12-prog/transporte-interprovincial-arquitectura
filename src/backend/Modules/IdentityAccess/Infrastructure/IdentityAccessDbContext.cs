using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

/// <summary>EF Core context for the IdentityAccess module.</summary>
public sealed class IdentityAccessDbContext : DbContext
{
    public IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity_access");
        base.OnModelCreating(modelBuilder);
    }
}
