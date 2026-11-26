using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class SaveFareUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<Fare> ExecuteAsync(Guid routeId, Guid? fareId, AgencyOperationScope scope, SaveFareInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        if (!await repository.RouteExistsAsync(routeId, scope.CompanyId, cancellationToken))
            throw new AgencyCatalogNotFoundException("Route was not found.");
        if (input.Price <= 0 || string.IsNullOrWhiteSpace(input.Currency))
            throw new AgencyCatalogValidationException("A positive fare price and currency are required.");
        if (input.ValidTo.HasValue && input.ValidTo.Value < input.ValidFrom)
            throw new AgencyCatalogValidationException("Fare end date cannot precede start date.");
        if (await repository.FarePeriodOverlapsAsync(routeId, scope.CompanyId, input.Type, input.ValidFrom, input.ValidTo, fareId, cancellationToken))
            throw new AgencyCatalogConflictException("The fare period overlaps another fare of the same type and route.");

        if (fareId is null)
        {
            var fare = new Fare(Guid.NewGuid(), scope.CompanyId, routeId, input.Type, input.ValidFrom, input.ValidTo, input.Price, input.Currency);
            await repository.AddFareAsync(fare, cancellationToken);
            return fare;
        }

        var existing = await repository.GetFareAsync(fareId.Value, routeId, scope.CompanyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Fare was not found.");
        existing.Update(input.ValidFrom, input.ValidTo, input.Price, input.Currency);
        await repository.SaveFareAsync(existing, cancellationToken);
        return existing;
    }
}

public sealed class SetFareActiveUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<Fare> ExecuteAsync(Guid routeId, Guid fareId, AgencyOperationScope scope, bool isActive, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        var fare = await repository.GetFareAsync(fareId, routeId, scope.CompanyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Fare was not found.");
        fare.SetActive(isActive);
        await repository.SaveFareAsync(fare, cancellationToken);
        return fare;
    }
}
