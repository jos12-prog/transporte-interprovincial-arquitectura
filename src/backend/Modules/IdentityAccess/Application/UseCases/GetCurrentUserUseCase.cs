using TransportPlatform.Modules.IdentityAccess.Application.Models;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Application.UseCases;

public sealed class GetCurrentUserUseCase
{
    public CurrentUser Execute(User user) => new(user.Id, user.FullName, user.NormalizedEmail, user.Role,
        user.AgencyAssignments.Select(a => a.CompanyId).Distinct().SingleOrDefault(),
        user.AgencyAssignments.Select(a => a.AgencyId).Distinct().ToArray());
}
