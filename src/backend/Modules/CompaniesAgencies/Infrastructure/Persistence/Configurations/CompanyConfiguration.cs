using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

namespace TransportPlatform.Modules.CompaniesAgencies.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(company => company.Id);
        builder.Property(company => company.Id).HasColumnName("id");
        builder.Property(company => company.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        builder.Property(company => company.TaxId).HasColumnName("tax_id").HasMaxLength(11);
        builder.Property(company => company.ContactName).HasColumnName("contact_name").HasMaxLength(160);
        builder.Property(company => company.ContactPhone).HasColumnName("contact_phone").HasMaxLength(40);
        builder.Property(company => company.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.HasIndex(company => company.TaxId).IsUnique().HasFilter("tax_id IS NOT NULL");
    }
}
