using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.Reservations.Domain.Entities;
using TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

namespace TransportPlatform.Modules.Reservations.Infrastructure;

public static class ReservationsDevelopmentSeeder
{
    public static async Task SeedAsync(
        ReservationsDbContext db,
        IReadOnlyCollection<TripSeat> tripSeats,
        CancellationToken cancellationToken = default)
    {
        var ids = (await db.TripSeats.Select(item => item.Id).ToListAsync(cancellationToken)).ToHashSet();
        db.TripSeats.AddRange(tripSeats.Where(item => !ids.Contains(item.Id)));
        await db.SaveChangesAsync(cancellationToken);
    }
}
