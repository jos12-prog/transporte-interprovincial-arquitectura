using TransportPlatform.Modules.TransportScheduling.Application.Contracts;

namespace TransportPlatform.Modules.TransportScheduling.Application.UseCases;

internal static class AgencyCatalogUseCaseSupport
{
    public static void ValidateScope(AgencyOperationScope scope)
    {
        if (scope.CompanyId == Guid.Empty || scope.AgencyId == Guid.Empty)
        {
            throw new AgencyCatalogForbiddenException("A valid active company and agency scope is required.");
        }
    }

    public static async Task ValidateScopeAsync(AgencyOperationScope scope, IAgencyScopeValidator validator, CancellationToken cancellationToken)
    {
        ValidateScope(scope);
        if (!await validator.IsActiveAgencyOfCompanyAsync(scope.CompanyId, scope.AgencyId, cancellationToken))
        {
            throw new AgencyCatalogForbiddenException("The active agency is not assigned to this company.");
        }
    }
}
