using TransportPlatform.Modules.IdentityAccess.Domain.Entities;
using TransportPlatform.Modules.IdentityAccess.Domain.Roles;

namespace TransportPlatform.Modules.IdentityAccess.Application.Authorization;

public sealed class AgencyScope
{
    public bool Allows(User user, Guid requestedCompanyId, Guid requestedAgencyId) =>
        (user.Role is IdentityRoles.Agent or IdentityRoles.AgencyAdmin) &&
        requestedCompanyId != Guid.Empty && requestedAgencyId != Guid.Empty &&
        user.AgencyAssignments.Any(a => a.CompanyId == requestedCompanyId && a.AgencyId == requestedAgencyId);

    public bool Allows(string role, Guid tokenCompanyId, Guid requestedCompanyId, Guid requestedAgencyId,
        IReadOnlyCollection<Guid> tokenAgencyIds) =>
        (role is IdentityRoles.Agent or IdentityRoles.AgencyAdmin) &&
        tokenCompanyId != Guid.Empty && tokenCompanyId == requestedCompanyId &&
        requestedAgencyId != Guid.Empty && tokenAgencyIds.Contains(requestedAgencyId);
}
