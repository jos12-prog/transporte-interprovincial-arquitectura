using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class GetRoutesUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<IReadOnlyList<Route>> ExecuteAsync(AgencyOperationScope scope, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        return await repository.GetRoutesAsync(scope.CompanyId, cancellationToken);
    }
}

public sealed class GetVehiclesUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<IReadOnlyList<Vehicle>> ExecuteAsync(AgencyOperationScope scope, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        return await repository.GetVehiclesAsync(scope.CompanyId, scope.AgencyId, cancellationToken);
    }
}

public sealed class GetFaresUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<IReadOnlyList<Fare>> ExecuteAsync(Guid routeId, AgencyOperationScope scope, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        if (!await repository.RouteExistsAsync(routeId, scope.CompanyId, cancellationToken))
            throw new AgencyCatalogNotFoundException("Route was not found.");
        return await repository.GetFaresAsync(routeId, scope.CompanyId, cancellationToken);
    }
}

public sealed class GetTripsUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<IReadOnlyList<Trip>> ExecuteAsync(AgencyOperationScope scope, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        return await repository.GetTripsAsync(scope.CompanyId, scope.AgencyId, cancellationToken);
    }
}
