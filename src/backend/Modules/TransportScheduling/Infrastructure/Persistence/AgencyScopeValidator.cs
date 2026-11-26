using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.TransportScheduling.Infrastructure.Persistence;

internal sealed class AgencyScopeValidator(ICompaniesAgenciesApi agencies) : IAgencyScopeValidator
{
    public async Task<bool> IsActiveAgencyOfCompanyAsync(Guid companyId, Guid agencyId, CancellationToken cancellationToken = default)
    {
        var agency = (await agencies.GetPublicAgenciesAsync([agencyId], cancellationToken)).SingleOrDefault();
        return agency is not null && agency.CompanyId == companyId && agency.AgencyIsActive && agency.CompanyIsActive;
    }
}
