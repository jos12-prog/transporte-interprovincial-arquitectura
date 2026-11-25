using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;

namespace TransportPlatform.Modules.TransportScheduling.Domain.Catalog;

public static class StandardVehicleModels
{
    private static readonly Guid YarisId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    private static readonly Guid AvanzaId = Guid.Parse("10000000-0000-0000-0000-000000000006");
    private static readonly Guid MinivanId = Guid.Parse("10000000-0000-0000-0000-000000000007");
    private static readonly Guid VanId = Guid.Parse("10000000-0000-0000-0000-000000000015");
    private static readonly Guid BusSingleFloorId = Guid.Parse("10000000-0000-0000-0000-000000000044");
    private static readonly Guid BusDoubleFloorId = Guid.Parse("10000000-0000-0000-0000-000000000052");

    public static IReadOnlyList<VehicleModel> Create()
    {
        return Array.AsReadOnly(new[]
        {
            Model(YarisId, "Toyota Yaris", "Sedán", new SeatPosition(1, 1, 3), new SeatPosition(2, 3, 1),
                new SeatPosition(3, 3, 2), new SeatPosition(4, 3, 3)),
            Model(AvanzaId, "Toyota Avanza", "Familiar", new SeatPosition(1, 1, 3), new SeatPosition(2, 2, 1),
                new SeatPosition(3, 2, 2), new SeatPosition(4, 2, 3), new SeatPosition(5, 3, 1),
                new SeatPosition(6, 3, 3)),
            Model(MinivanId, "Minivan", "Familiar", new SeatPosition(1, 1, 3), new SeatPosition(2, 2, 1),
                new SeatPosition(3, 2, 2), new SeatPosition(4, 2, 3), new SeatPosition(5, 3, 1),
                new SeatPosition(6, 3, 2), new SeatPosition(7, 3, 3)),
            Model(VanId, "Van", "Transporte", new SeatPosition(1, 1, 3), new SeatPosition(2, 1, 4),
                new SeatPosition(3, 2, 1), new SeatPosition(4, 2, 2), new SeatPosition(5, 2, 3),
                new SeatPosition(6, 3, 1), new SeatPosition(7, 3, 2), new SeatPosition(8, 3, 4),
                new SeatPosition(9, 4, 1), new SeatPosition(10, 4, 2), new SeatPosition(11, 4, 4),
                new SeatPosition(12, 5, 1), new SeatPosition(13, 5, 2), new SeatPosition(14, 5, 3),
                new SeatPosition(15, 5, 4)),
            Model(BusSingleFloorId, "Bus 1 piso", "Bus", BusSingleFloorSeats()),
            Model(BusDoubleFloorId, "Bus 2 pisos", "Bus", BusDoubleFloorSeats())
        });
    }

    private static VehicleModel Model(Guid id, string name, string category, params SeatPosition[] seats)
    {
        return new VehicleModel(id, name, seats.Length, new SeatLayout(seats), category);
    }

    private static SeatPosition[] BusSingleFloorSeats() => Enumerable.Range(0, 11)
        .SelectMany(row => new[]
        {
            new SeatPosition(row * 4 + 1, row + 1, 1),
            new SeatPosition(row * 4 + 2, row + 1, 2),
            new SeatPosition(row * 4 + 3, row + 1, 4),
            new SeatPosition(row * 4 + 4, row + 1, 5)
        }).ToArray();

    private static SeatPosition[] BusDoubleFloorSeats()
    {
        var lowerFloor = Enumerable.Range(0, 4)
            .SelectMany(row => new[]
            {
                new SeatPosition(row * 3 + 1, row + 2, 1, floor: 1),
                new SeatPosition(row * 3 + 2, row + 2, 4, floor: 1),
                new SeatPosition(row * 3 + 3, row + 2, 5, floor: 1)
            });
        var upperFloor = Enumerable.Range(0, 10)
            .SelectMany(row => new[]
            {
                new SeatPosition(13 + row * 4, row + 2, 1, floor: 2),
                new SeatPosition(14 + row * 4, row + 2, 2, floor: 2),
                new SeatPosition(15 + row * 4, row + 2, 4, floor: 2),
                new SeatPosition(16 + row * 4, row + 2, 5, floor: 2)
            });
        return lowerFloor.Concat(upperFloor).ToArray();
    }
}
