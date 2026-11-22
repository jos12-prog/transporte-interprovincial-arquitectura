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
    }
}
