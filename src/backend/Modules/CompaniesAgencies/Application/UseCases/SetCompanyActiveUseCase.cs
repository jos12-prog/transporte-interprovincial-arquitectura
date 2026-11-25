using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;

public sealed class SetCompanyActiveUseCase(ICompaniesAgenciesAdministrationRepository repository)
{
    public async Task<Company> ExecuteAsync(Guid companyId, bool isActive, CancellationToken cancellationToken = default)
    {
        var company = await repository.GetCompanyAsync(companyId, cancellationToken)
            ?? throw new CatalogNotFoundException("Company was not found.");
        company.SetActive(isActive);
        await repository.SaveCompanyAsync(company, cancellationToken);
        return company;
    }
}
