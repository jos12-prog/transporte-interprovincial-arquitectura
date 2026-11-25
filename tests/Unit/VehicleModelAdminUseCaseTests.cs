using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Application.UseCases;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;
using TransportPlatform.Modules.TransportScheduling.Domain.ValueObjects;
using TransportPlatform.Shared.Contracts.Trips;
using Xunit;
using Microsoft.AspNetCore.Authorization;
using TransportPlatform.Modules.TransportScheduling.Presentation.Controllers;
using TransportPlatform.Modules.TransportScheduling.Application.Services;

namespace TransportPlatform.Unit.Tests;

public sealed class VehicleModelAdminUseCaseTests
{
    [Fact]
    public async Task SaveModelCreatesValidLayoutAndRejectsDuplicateName()
    {
        var repository = new FakeRepository();
        var useCase = new SaveVehicleModelUseCase(repository);
        var request = new SaveVehicleModelRequest("Yaris", "Sedan", new[]
        {
            new SeatPositionContract(1, 1, 1, 1),
            new SeatPositionContract(2, 1, 1, 2)
        });
        var model = await useCase.ExecuteAsync(null, request);

        Assert.Equal(2, model.Capacity);
        Assert.Equal("Sedan", model.Category);
        Assert.True(model.IsActive);
        await Assert.ThrowsAsync<VehicleModelConflictException>(() => useCase.ExecuteAsync(null, request));
        await Assert.ThrowsAsync<VehicleModelValidationException>(() => useCase.ExecuteAsync(null,
            request with { Seats = Array.Empty<SeatPositionContract>() }));
    }

    [Fact]
    public async Task SaveModelInUseAllowsRenameButRejectsSeatLayoutChange()
    {
        var repository = new FakeRepository { IsInUse = true };
        var oldLayout = new SeatLayout(new[] { new SeatPosition(1, 1, 1), new SeatPosition(2, 1, 2) });
        var model = new VehicleModel(Guid.NewGuid(), "Yaris", 2, oldLayout);
        repository.Seed(model);
        var useCase = new SaveVehicleModelUseCase(repository);

        var renamed = await useCase.ExecuteAsync(model.Id,
            new SaveVehicleModelRequest("Yaris 2027", null, new[]
            {
                new SeatPositionContract(1, 1, 1, 1),
                new SeatPositionContract(2, 1, 1, 2)
            }));
        Assert.Equal("Yaris 2027", renamed.Name);
        Assert.Equal(oldLayout.Seats, renamed.SeatLayout.Seats);
        var renamedLayout = renamed.SeatLayout;

        await Assert.ThrowsAsync<VehicleModelConflictException>(() => useCase.ExecuteAsync(model.Id,
            new SaveVehicleModelRequest("Yaris 2027", null, new[]
            {
                new SeatPositionContract(1, 1, 1, 1),
                new SeatPositionContract(2, 1, 2, 1)
            })));
        Assert.Same(renamedLayout, model.SeatLayout);
    }

    [Fact]
    public async Task SetActiveUseCaseChangesStateAndFiltersInactiveModelsFromCreationCatalog()
    {
        var repository = new FakeRepository();
        var model = new VehicleModel(Guid.NewGuid(), "Avanza", 2,
            new SeatLayout(new[] { new SeatPosition(1, 1, 1), new SeatPosition(2, 1, 2) }));
        repository.Seed(model);
        var useCase = new SetVehicleModelActiveUseCase(repository);

        Assert.False((await useCase.ExecuteAsync(model.Id, false)).IsActive);
        Assert.Empty(await repository.GetVehicleModelsAsync(activeOnly: true));
        Assert.Single(await repository.GetVehicleModelsAsync(activeOnly: false));
        Assert.True((await useCase.ExecuteAsync(model.Id, true)).IsActive);
        await Assert.ThrowsAsync<VehicleModelNotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid(), false));
    }

    [Fact]
    public async Task GetVehicleModelsUsesOneBatchedInUseQueryToBuildDtos()
    {
        var repository = new FakeRepository();
        var used = Model("Avanza", 6);
        var unused = Model("Yaris", 4);
        var inactive = Model("Minivan", 7);
        inactive.SetActive(false);
        repository.Seed(used, unused, inactive);
        repository.UsedModelIds.Add(used.Id);
        var api = new TransportSchedulingApi(null!, null!, repository);

        var result = await api.GetVehicleModelsAsync(activeOnly: false);

        Assert.Equal(1, repository.BatchedInUseQueryCount);
        Assert.Equal(0, repository.PerModelInUseQueryCount);
        Assert.Collection(result,
            dto => Assert.True(dto.IsInUse),
            dto => Assert.False(dto.IsInUse),
            dto => Assert.False(dto.IsInUse));
        Assert.False(result[2].IsActive);
    }

    [Fact]
    public void VehicleModelControllerRequiresTheSuperAdminPolicy()
    {
        Assert.Equal("SuperAdmin", typeof(PlatformVehicleModelsController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Policy);
    }

    private static VehicleModel Model(string name, int capacity)
    {
        var seats = Enumerable.Range(1, capacity).Select(number => new SeatPosition(number, 1, number));
        return new VehicleModel(Guid.NewGuid(), name, capacity, new SeatLayout(seats));
    }

    private sealed class FakeRepository : IVehicleModelAdministrationRepository
    {
        private readonly List<VehicleModel> models = [];
        public bool IsInUse { get; init; }
        public HashSet<Guid> UsedModelIds { get; } = [];
        public int BatchedInUseQueryCount { get; private set; }
        public int PerModelInUseQueryCount { get; private set; }
        public void Seed(params VehicleModel[] seededModels) => models.AddRange(seededModels);
        public Task<IReadOnlyList<VehicleModel>> GetVehicleModelsAsync(bool activeOnly, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<VehicleModel>>(activeOnly ? models.Where(model => model.IsActive).ToArray() : models);
        public Task<VehicleModel?> GetVehicleModelAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(models.SingleOrDefault(model => model.Id == id));
        public Task<IReadOnlyCollection<Guid>> GetVehicleModelIdsInUseAsync(CancellationToken cancellationToken = default)
        {
            BatchedInUseQueryCount++;
            return Task.FromResult<IReadOnlyCollection<Guid>>(UsedModelIds.ToArray());
        }
        public Task<bool> VehicleModelNameExistsAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(models.Any(model => model.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && model.Id != excludingId));
        public Task<bool> IsVehicleModelInUseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            PerModelInUseQueryCount++;
            return Task.FromResult(IsInUse || UsedModelIds.Contains(id));
        }
        public Task AddVehicleModelAsync(VehicleModel model, CancellationToken cancellationToken = default)
        {
            models.Add(model);
            return Task.CompletedTask;
        }
        public Task SaveVehicleModelAsync(VehicleModel model, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
