using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasKey(vehicle => vehicle.Id);
        builder.Property(vehicle => vehicle.Id).HasColumnName("id");
        builder.Property(vehicle => vehicle.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(vehicle => vehicle.AgencyId).HasColumnName("agency_id").IsRequired();
        builder.Property(vehicle => vehicle.VehicleModelId).HasColumnName("vehicle_model_id").IsRequired();
        builder.Property(vehicle => vehicle.Plate).HasColumnName("plate").HasMaxLength(16).IsRequired();
        builder.HasOne<VehicleModel>()
            .WithMany()
            .HasForeignKey(vehicle => vehicle.VehicleModelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(vehicle => new { vehicle.CompanyId, vehicle.Plate }).IsUnique();
        builder.HasIndex(vehicle => new { vehicle.CompanyId, vehicle.AgencyId });
    }
}
