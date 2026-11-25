using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using TransportPlatform.Modules.IdentityAccess.Application.Authorization;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.IdentityAccess.Presentation.Authorization;

public sealed class AgencyScopeAuthorizationHandler(AgencyScope scope, ICompaniesAgenciesApi companiesAgencies)
    : AuthorizationHandler<AgencyScopeRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AgencyScopeRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext ||
            !Guid.TryParse(httpContext.Request.RouteValues["agencyId"]?.ToString(), out var agencyId) ||
            !Guid.TryParse(context.User.FindFirstValue("company_id"), out var companyId)) return;
        var role = context.User.FindFirstValue("role");
        var assignedIds = context.User.FindAll("agency_ids")
            .Select(claim => Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty).ToArray();
        if (!assignedIds.Contains(agencyId)) return;
        var agency = (await companiesAgencies.GetPublicAgenciesAsync([agencyId], httpContext.RequestAborted)).SingleOrDefault();
        if (agency is not null && scope.Allows(role ?? string.Empty, companyId, agency.CompanyId, agencyId, assignedIds))
            context.Succeed(requirement);
    }
}
