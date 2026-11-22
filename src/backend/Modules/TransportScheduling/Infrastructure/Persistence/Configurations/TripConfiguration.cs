using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");
        builder.HasKey(trip => trip.Id);
        builder.Property(trip => trip.Id).HasColumnName("id");
        builder.Property(trip => trip.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(trip => trip.AgencyId).HasColumnName("agency_id").IsRequired();
        builder.Property(trip => trip.RouteId).HasColumnName("route_id").IsRequired();
        builder.Property(trip => trip.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(trip => trip.DepartureAtUtc).HasColumnName("departure_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(trip => trip.ArrivalAtUtc).HasColumnName("arrival_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(trip => trip.Price).HasColumnName("price").HasPrecision(10, 2);
        builder.Property(trip => trip.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.HasOne<Route>().WithMany().HasForeignKey(trip => trip.RouteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Vehicle>().WithMany().HasForeignKey(trip => trip.VehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(trip => new { trip.RouteId, trip.DepartureAtUtc });
        builder.HasIndex(trip => new { trip.CompanyId, trip.AgencyId, trip.DepartureAtUtc });
    }
}
