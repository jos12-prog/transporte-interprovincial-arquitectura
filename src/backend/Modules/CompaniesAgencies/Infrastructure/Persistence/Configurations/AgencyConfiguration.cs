using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence.Configurations;

public sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agencies");
        builder.HasKey(agency => agency.Id);
        builder.Property(agency => agency.Id).HasColumnName("id");
        builder.Property(agency => agency.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(agency => agency.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        builder.Property(agency => agency.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(agency => agency.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(agency => new { agency.CompanyId, agency.City });
    }
}
