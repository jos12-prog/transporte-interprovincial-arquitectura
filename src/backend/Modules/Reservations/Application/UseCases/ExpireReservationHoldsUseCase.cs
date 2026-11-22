using TransportPlatform.Modules.Reservations.Application.Contracts;

namespace TransportPlatform.Modules.Reservations.Application.UseCases;

public sealed class ExpireReservationHoldsUseCase(IReservationRepository repository, TimeProvider timeProvider)
{
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default) =>
        repository.ExpireHoldsAsync(timeProvider.GetUtcNow(), cancellationToken);
}
