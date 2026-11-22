using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.Reservations.Application.Contracts;
using TransportPlatform.Modules.Reservations.Domain.Enums;

namespace TransportPlatform.Modules.Reservations.Infrastructure.Persistence;

internal sealed class EfSeatAvailabilityService(ReservationsDbContext dbContext) : ISeatAvailabilityService
{
    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableSeatCountsAsync(
        IReadOnlyCollection<Guid> tripIds,
        CancellationToken cancellationToken = default)
    {
        var ids = tripIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var counts = await dbContext.TripSeats.AsNoTracking()
            .Where(seat => ids.Contains(seat.TripId) && seat.Status == TripSeatStatus.Available)
            .GroupBy(seat => seat.TripId)
            .Select(group => new { TripId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.TripId, item => item.Count, cancellationToken);

        return ids.ToDictionary(id => id, id => counts.GetValueOrDefault(id));
    }
}
