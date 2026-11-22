namespace TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;

public sealed record SeatPosition
{
    public int Number { get; }
    public int? Row { get; }
    public int? Column { get; }
    public int Floor { get; }

    public SeatPosition(int number, int? row, int? column, int floor = 1)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Seat number must be positive.");
        }

        if (row.HasValue != column.HasValue)
        {
            throw new ArgumentException("Row and column must either both be set or both be empty.");
        }

        if (row is <= 0 || column is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(row), "Row and column must be positive when provided.");
        }

        if (floor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(floor), "Floor must be positive.");
        }

        Number = number;
        Row = row;
        Column = column;
        Floor = floor;
    }
}
