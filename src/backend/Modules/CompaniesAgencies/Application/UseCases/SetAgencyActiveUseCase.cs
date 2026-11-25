using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;

public sealed class SetAgencyActiveUseCase(ICompaniesAgenciesAdministrationRepository repository)
{
    public async Task<Agency> ExecuteAsync(Guid agencyId, bool isActive, CancellationToken cancellationToken = default)
    {
        var agency = await repository.GetAgencyAsync(agencyId, cancellationToken)
            ?? throw new CatalogNotFoundException("Agency was not found.");
        agency.SetActive(isActive);
        await repository.SaveAgencyAsync(agency, cancellationToken);
        return agency;
    }
}
