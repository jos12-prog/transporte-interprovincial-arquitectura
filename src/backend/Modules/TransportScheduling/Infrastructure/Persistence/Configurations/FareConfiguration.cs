using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Configurations;

public sealed class FareConfiguration : IEntityTypeConfiguration<Fare>
{
    public void Configure(EntityTypeBuilder<Fare> builder)
    {
        builder.ToTable("fares");
        builder.HasKey(fare => fare.Id);
        builder.Property(fare => fare.Id).HasColumnName("id");
        builder.Property(fare => fare.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(fare => fare.RouteId).HasColumnName("route_id").IsRequired();
        builder.Property(fare => fare.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(fare => fare.ValidFrom).HasColumnName("valid_from").HasColumnType("date").IsRequired();
        builder.Property(fare => fare.ValidTo).HasColumnName("valid_to").HasColumnType("date");
        builder.Property(fare => fare.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
        builder.Property(fare => fare.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(fare => fare.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.HasOne<Route>().WithMany().HasForeignKey(fare => fare.RouteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(fare => new { fare.CompanyId, fare.RouteId, fare.Type, fare.ValidFrom, fare.ValidTo });
    }
}
