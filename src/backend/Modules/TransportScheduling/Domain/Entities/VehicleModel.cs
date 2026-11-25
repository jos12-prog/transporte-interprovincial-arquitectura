using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;

namespace TransportPlatform.Modules.TransportScheduling.Domain.Entities;

public sealed class VehicleModel
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public int Capacity { get; private set; }
    public SeatLayout SeatLayout { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private VehicleModel()
    {
    }

    public VehicleModel(Guid id, string name, int capacity, SeatLayout seatLayout, string? category = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Vehicle model id is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(seatLayout);
        if (capacity <= 0 || seatLayout.Capacity != capacity)
        {
            throw new ArgumentException("Capacity must match the number of seats in the layout.", nameof(capacity));
        }

        Id = id;
        Name = Route.RequiredText(name, nameof(name));
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Capacity = capacity;
        SeatLayout = seatLayout;
    }

    public void UpdateProfile(string name, int capacity, SeatLayout seatLayout, string? category = null)
    {
        ArgumentNullException.ThrowIfNull(seatLayout);
        if (capacity <= 0 || seatLayout.Capacity != capacity)
        {
            throw new ArgumentException("Capacity must match the number of seats in the layout.", nameof(capacity));
        }

        Name = Route.RequiredText(name, nameof(name));
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Capacity = capacity;
        SeatLayout = seatLayout;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void SetCategory(string? category) => Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
}
