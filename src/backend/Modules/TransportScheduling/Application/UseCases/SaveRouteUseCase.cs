using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Modules.TransportScheduling.Domain.Entities;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

public sealed class SaveRouteUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<Route> ExecuteAsync(Guid? routeId, AgencyOperationScope scope, SaveRouteInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        if (string.IsNullOrWhiteSpace(input.OriginCity) || string.IsNullOrWhiteSpace(input.DestinationCity))
            throw new AgencyCatalogValidationException("Origin and destination are required.");

        if (routeId is null)
        {
            var route = new Route(Guid.NewGuid(), scope.CompanyId, input.OriginCity, input.DestinationCity);
            await repository.AddRouteAsync(route, cancellationToken);
            return route;
        }

        var existing = await repository.GetRouteAsync(routeId.Value, scope.CompanyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Route was not found.");
        try { existing.Update(input.OriginCity, input.DestinationCity); }
        catch (ArgumentException exception) { throw new AgencyCatalogValidationException(exception.Message); }
        await repository.SaveRouteAsync(existing, cancellationToken);
        return existing;
    }
}

public sealed class SetRouteActiveUseCase(IAgencyCatalogRepository repository, IAgencyScopeValidator scopeValidator)
{
    public async Task<Route> ExecuteAsync(Guid routeId, AgencyOperationScope scope, bool isActive, CancellationToken cancellationToken = default)
    {
        await AgencyCatalogUseCaseSupport.ValidateScopeAsync(scope, scopeValidator, cancellationToken);
        var route = await repository.GetRouteAsync(routeId, scope.CompanyId, cancellationToken)
            ?? throw new AgencyCatalogNotFoundException("Route was not found.");
        route.SetActive(isActive);
        await repository.SaveRouteAsync(route, cancellationToken);
        return route;
    }
}
