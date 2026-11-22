using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.QueriesReports.Infrastructure.Persistence;

/// <summary>EF Core context for the QueriesReports module.</summary>
public sealed class QueriesReportsDbContext : DbContext
{
    public QueriesReportsDbContext(DbContextOptions<QueriesReportsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("queries_reports");
        base.OnModelCreating(modelBuilder);
    }
}
