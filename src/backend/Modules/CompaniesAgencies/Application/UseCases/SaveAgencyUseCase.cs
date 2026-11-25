using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;

public sealed class SaveAgencyUseCase(ICompaniesAgenciesAdministrationRepository repository)
{
    public async Task<Agency> ExecuteAsync(Guid? agencyId, Guid companyId, SaveAgencyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (companyId == Guid.Empty) throw new CatalogValidationException("Company id is required.");
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.City))
            throw new CatalogValidationException("Agency name and city are required.");
        if (!await repository.CompanyIsActiveAsync(companyId, cancellationToken))
            throw new CatalogNotFoundException("Active company was not found.");

        if (agencyId is null)
        {
            var agency = new Agency(Guid.NewGuid(), companyId, request.Name, request.City, request.Address, request.Phone);
            await repository.AddAgencyAsync(agency, cancellationToken);
            return agency;
        }

        var existing = await repository.GetAgencyAsync(agencyId.Value, cancellationToken)
            ?? throw new CatalogNotFoundException("Agency was not found.");
        if (existing.CompanyId != companyId) throw new CatalogConflictException("Agency company cannot be changed.");
        existing.UpdateProfile(request.Name, request.City, request.Address, request.Phone);
        await repository.SaveAgencyAsync(existing, cancellationToken);
        return existing;
    }
}
