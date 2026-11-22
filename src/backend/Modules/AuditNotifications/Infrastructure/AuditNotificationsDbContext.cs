using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.AuditNotifications.Infrastructure.Persistence;

/// <summary>EF Core context for the AuditNotifications module.</summary>
public sealed class AuditNotificationsDbContext : DbContext
{
    public AuditNotificationsDbContext(DbContextOptions<AuditNotificationsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit_notifications");
        base.OnModelCreating(modelBuilder);
    }
}
