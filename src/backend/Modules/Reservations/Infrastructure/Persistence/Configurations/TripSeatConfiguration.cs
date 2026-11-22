using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransportPlatform.Modules.Reservations.Domain.Entities;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence.Configurations;

public sealed class TripSeatConfiguration : IEntityTypeConfiguration<TripSeat>
{
    public void Configure(EntityTypeBuilder<TripSeat> builder)
    {
        builder.ToTable("trip_seats");
        builder.HasKey(seat => seat.Id);
        builder.Property(seat => seat.Id).HasColumnName("id");
        builder.Property(seat => seat.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(seat => seat.AgencyId).HasColumnName("agency_id").IsRequired();
        builder.Property(seat => seat.TripId).HasColumnName("trip_id").IsRequired();
        builder.Property(seat => seat.SeatNumber).HasColumnName("seat_number").IsRequired();
        builder.Property(seat => seat.Row).HasColumnName("row_number");
        builder.Property(seat => seat.Column).HasColumnName("column_number");
        builder.Property(seat => seat.Floor).HasColumnName("floor").IsRequired();
        builder.Property(seat => seat.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.HasIndex(seat => new { seat.TripId, seat.SeatNumber }).IsUnique();
        builder.HasIndex(seat => new { seat.TripId, seat.Status });
        builder.HasIndex(seat => new { seat.CompanyId, seat.AgencyId, seat.TripId });
    }
}
