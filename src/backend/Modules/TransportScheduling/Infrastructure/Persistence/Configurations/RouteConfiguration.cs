using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Configurations;

public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("routes");
        builder.HasKey(route => route.Id);
        builder.Property(route => route.Id).HasColumnName("id");
        builder.Property(route => route.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(route => route.OriginCity).HasColumnName("origin_city").HasMaxLength(120).IsRequired();
        builder.Property(route => route.DestinationCity).HasColumnName("destination_city").HasMaxLength(120).IsRequired();
        builder.Property(route => route.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.HasIndex(route => new { route.OriginCity, route.DestinationCity });
        builder.HasIndex(route => new { route.CompanyId, route.OriginCity, route.DestinationCity }).IsUnique();
    }
}
