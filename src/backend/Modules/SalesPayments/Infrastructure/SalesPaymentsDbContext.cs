using Microsoft.EntityFrameworkCore;

namespace TransportPlatform.Modules.SalesPayments.Infrastructure.Persistence;

/// <summary>EF Core context for the SalesPayments module.</summary>
public sealed class SalesPaymentsDbContext : DbContext
{
    public SalesPaymentsDbContext(DbContextOptions<SalesPaymentsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales_payments");
        base.OnModelCreating(modelBuilder);
    }
}
