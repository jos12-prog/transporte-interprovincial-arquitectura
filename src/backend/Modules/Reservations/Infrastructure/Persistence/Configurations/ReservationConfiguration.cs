using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.Reservations.Domain.Entities;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(reservation => reservation.Id);
        builder.Property(reservation => reservation.Id).HasColumnName("id");
        builder.Property(reservation => reservation.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(reservation => reservation.AgencyId).HasColumnName("agency_id").IsRequired();
        builder.Property(reservation => reservation.TripId).HasColumnName("trip_id").IsRequired();
        builder.Property(reservation => reservation.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(reservation => reservation.HoldTokenHash).HasColumnName("hold_token_hash").HasMaxLength(128).IsRequired();
        builder.Property(reservation => reservation.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(reservation => reservation.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.HasMany(reservation => reservation.Seats).WithOne().HasForeignKey(seat => seat.ReservationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(reservation => reservation.Seats).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(reservation => new { reservation.CompanyId, reservation.AgencyId, reservation.TripId });
        builder.HasIndex(reservation => new { reservation.Status, reservation.ExpiresAtUtc });
    }
}
