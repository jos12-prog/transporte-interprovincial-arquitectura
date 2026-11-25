using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;

public sealed class SaveCompanyUseCase(ICompaniesAgenciesAdministrationRepository repository)
{
    public async Task<Company> ExecuteAsync(Guid? companyId, SaveCompanyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name)) throw new CatalogValidationException("Company name is required.");
        if (request.TaxId is { Length: > 0 } taxId && (taxId.Length != 11 || taxId.Any(character => !char.IsDigit(character))))
            throw new CatalogValidationException("Tax identifier must contain exactly 11 digits.");
        if (request.TaxId is { Length: > 0 } && await repository.TaxIdExistsAsync(request.TaxId, companyId, cancellationToken))
            throw new CatalogConflictException("A company with this tax identifier already exists.");

        if (companyId is null)
        {
            var company = new Company(Guid.NewGuid(), request.Name, request.TaxId, request.ContactName, request.ContactPhone);
            await repository.AddCompanyAsync(company, cancellationToken);
            return company;
        }

        var existing = await repository.GetCompanyAsync(companyId.Value, cancellationToken)
            ?? throw new CatalogNotFoundException("Company was not found.");
        existing.UpdateProfile(request.Name, request.TaxId, request.ContactName, request.ContactPhone);
        await repository.SaveCompanyAsync(existing, cancellationToken);
        return existing;
    }
}
