using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.Reservations.Domain.Entities;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence.Configurations;

public sealed class ReservationSeatConfiguration : IEntityTypeConfiguration<ReservationSeat>
{
    public void Configure(EntityTypeBuilder<ReservationSeat> builder)
    {
        builder.ToTable("reservation_seats");
        builder.HasKey(seat => seat.Id);
        builder.Property(seat => seat.Id).HasColumnName("id");
        builder.Property(seat => seat.ReservationId).HasColumnName("reservation_id").IsRequired();
        builder.Property(seat => seat.TripSeatId).HasColumnName("trip_seat_id").IsRequired();
        builder.Property(seat => seat.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.HasIndex(seat => seat.TripSeatId)
            .IsUnique()
            .HasDatabaseName("UX_reservation_seats_active_trip_seat")
            .HasFilter("status IN ('Held', 'Confirmed')");
        builder.HasIndex(seat => new { seat.ReservationId, seat.Status });
    }
}
