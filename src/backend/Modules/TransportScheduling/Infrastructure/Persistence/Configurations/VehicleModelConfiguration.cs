using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence.Configurations;

public sealed class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        var converter = new ValueConverter<SeatLayout, string>(
            layout => JsonSerializer.Serialize(layout.Seats, (JsonSerializerOptions?)null),
            json => new SeatLayout(JsonSerializer.Deserialize<SeatPosition[]>(json, (JsonSerializerOptions?)null)
                ?? Array.Empty<SeatPosition>()));
        var comparer = new ValueComparer<SeatLayout>(
            (left, right) => left!.Seats.SequenceEqual(right!.Seats),
            layout => layout!.Seats.Aggregate(0, (hash, seat) => HashCode.Combine(hash, seat)),
            layout => new SeatLayout(layout!.Seats));

        builder.ToTable("vehicle_models");
        builder.HasKey(model => model.Id);
        builder.Property(model => model.Id).HasColumnName("id");
        builder.Property(model => model.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(model => model.Capacity).HasColumnName("capacity").IsRequired();
        var seatLayout = builder.Property(model => model.SeatLayout)
            .HasColumnName("seat_layout")
            .HasColumnType("jsonb")
            .HasConversion(converter)
            .IsRequired();
        seatLayout.Metadata.SetValueComparer(comparer);
        builder.HasIndex(model => model.Name).IsUnique();
    }
}
