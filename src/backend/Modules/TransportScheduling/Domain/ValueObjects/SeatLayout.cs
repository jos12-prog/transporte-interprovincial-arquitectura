namespace TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;

public sealed class SeatLayout
{
    private readonly IReadOnlyList<SeatPosition> _seats;

    public IReadOnlyList<SeatPosition> Seats => _seats;
    public int Capacity => _seats.Count;

    public SeatLayout(IEnumerable<SeatPosition> seats)
    {
        ArgumentNullException.ThrowIfNull(seats);

        var positions = seats.OrderBy(seat => seat.Number).ToArray();
        if (positions.Length == 0)
        {
            throw new ArgumentException("A seat layout must contain at least one seat.", nameof(seats));
        }

        if (positions.Select(seat => seat.Number).Distinct().Count() != positions.Length)
        {
            throw new ArgumentException("Seat numbers must be unique within a layout.", nameof(seats));
        }

        var locatedSeats = positions.Where(seat => seat.Row.HasValue).ToArray();
        if (locatedSeats
            .GroupBy(seat => (seat.Floor, seat.Row, seat.Column))
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Two seats cannot occupy the same position.", nameof(seats));
        }

        _seats = Array.AsReadOnly(positions);
    }
}
