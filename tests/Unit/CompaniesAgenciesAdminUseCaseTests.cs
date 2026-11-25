using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;
using TransportPlatform.Modules.CompaniesAgencies.Domain.Entities;
using TransportPlatform.Shared.Contracts.Companies;
using Microsoft.AspNetCore.Authorization;
using TransportPlatform.Modules.CompaniesAgencies.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TransportPlatform.Host;
using Xunit;

namespace TransportPlatform.Unit.Tests;

public sealed class CompaniesAgenciesAdminUseCaseTests
{
    [Fact]
    public async Task SaveCompanyCreatesNormalizedCompanyAndRejectsDuplicateTaxId()
    {
        var repository = new FakeRepository();
        var useCase = new SaveCompanyUseCase(repository);
        var created = await useCase.ExecuteAsync(null,
            new SaveCompanyRequest(" Andes Express ", "20123456789", " Contacto ", null));

        Assert.Equal("Andes Express", created.Name);
        Assert.Equal("Contacto", created.ContactName);
        Assert.True(created.IsActive);
        await Assert.ThrowsAsync<CatalogConflictException>(() => useCase.ExecuteAsync(null,
            new SaveCompanyRequest("Otra empresa", "20123456789", null, null)));
        await Assert.ThrowsAsync<CatalogValidationException>(() => useCase.ExecuteAsync(null,
            new SaveCompanyRequest("Otra empresa", "123", null, null)));
    }

    [Fact]
    public async Task SaveCompanyAndAgencyUpdatesReturnNotFoundForUnknownIds()
    {
        var repository = new FakeRepository();
        var companyUseCase = new SaveCompanyUseCase(repository);
        var agencyUseCase = new SaveAgencyUseCase(repository);

        await Assert.ThrowsAsync<CatalogNotFoundException>(() => companyUseCase.ExecuteAsync(Guid.NewGuid(),
            new SaveCompanyRequest("Empresa", null, null, null)));
        await Assert.ThrowsAsync<CatalogNotFoundException>(() => agencyUseCase.ExecuteAsync(null, Guid.NewGuid(),
            new SaveAgencyRequest("Agencia", "Ayacucho", null, null)));
    }

    [Fact]
    public async Task SaveCompanyAndAgencySupportSuccessfulUpdatesAndRejectInvalidAgencyScope()
    {
        var repository = new FakeRepository();
        var company = new Company(Guid.NewGuid(), "Andes", "20123456789", "María", "999111222");
        repository.Seed(company);
        var otherCompany = new Company(Guid.NewGuid(), "Wari", null, null, null);
        repository.Seed(otherCompany);
        var companyUseCase = new SaveCompanyUseCase(repository);
        var agencyUseCase = new SaveAgencyUseCase(repository);

        var updatedCompany = await companyUseCase.ExecuteAsync(company.Id,
            new SaveCompanyRequest("Andes Express", "20123456789", "José", "999222333"));
        Assert.Equal("Andes Express", updatedCompany.Name);
        Assert.Equal("José", updatedCompany.ContactName);

        var agency = await agencyUseCase.ExecuteAsync(null, company.Id,
            new SaveAgencyRequest("Terminal", "Ayacucho", "Centro", "999333444"));
        Assert.Equal(company.Id, agency.CompanyId);
        Assert.True(agency.IsActive);
        var updatedAgency = await agencyUseCase.ExecuteAsync(agency.Id, company.Id,
            new SaveAgencyRequest("Terminal renovado", "Huanta", null, null));
        Assert.Equal("Terminal renovado", updatedAgency.Name);
        Assert.Equal("Huanta", updatedAgency.City);

        await Assert.ThrowsAsync<CatalogValidationException>(() => agencyUseCase.ExecuteAsync(null, Guid.Empty,
            new SaveAgencyRequest("Terminal", "Ayacucho", null, null)));
        await Assert.ThrowsAsync<CatalogValidationException>(() => agencyUseCase.ExecuteAsync(null, company.Id,
            new SaveAgencyRequest("", "Ayacucho", null, null)));
        await Assert.ThrowsAsync<CatalogConflictException>(() => agencyUseCase.ExecuteAsync(agency.Id, otherCompany.Id,
            new SaveAgencyRequest("Terminal", "Ayacucho", null, null)));
    }

    [Fact]
    public async Task ActivationUseCasesChangeStateAndReturnNotFoundForUnknownIds()
    {
        var repository = new FakeRepository();
        var company = new Company(Guid.NewGuid(), "Andes", null, null, null);
        repository.Seed(company);
        var agency = await new SaveAgencyUseCase(repository).ExecuteAsync(null, company.Id,
            new SaveAgencyRequest("Terminal", "Ayacucho", null, null));

        var inactiveCompany = await new SetCompanyActiveUseCase(repository).ExecuteAsync(company.Id, false);
        Assert.False(inactiveCompany.IsActive);
        Assert.False((await new SetCompanyActiveUseCase(repository).ExecuteAsync(company.Id, false)).IsActive);
        var activeCompany = await new SetCompanyActiveUseCase(repository).ExecuteAsync(company.Id, true);
        Assert.True(activeCompany.IsActive);

        Assert.False((await new SetAgencyActiveUseCase(repository).ExecuteAsync(agency.Id, false)).IsActive);
        Assert.True((await new SetAgencyActiveUseCase(repository).ExecuteAsync(agency.Id, true)).IsActive);
        await Assert.ThrowsAsync<CatalogNotFoundException>(() => new SetCompanyActiveUseCase(repository).ExecuteAsync(Guid.NewGuid(), false));
        await Assert.ThrowsAsync<CatalogNotFoundException>(() => new SetAgencyActiveUseCase(repository).ExecuteAsync(Guid.NewGuid(), false));
    }

    [Fact]
    public void AdministrativeControllersRequireTheSuperAdminPolicy()
    {
        Assert.Equal("SuperAdmin", typeof(PlatformCompaniesController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Policy);
    }

    [Fact]
    public async Task SuperAdminPolicyRequiresAuthenticationAndTheStableSuperAdminRole()
    {
        var services = new ServiceCollection().AddPlatformAuthorization().BuildServiceProvider();
        var policy = await services.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync("SuperAdmin");

        Assert.NotNull(policy);
        Assert.Contains(policy!.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
        var roles = Assert.Single(policy.Requirements.OfType<RolesAuthorizationRequirement>());
        Assert.Contains("super_admin", roles.AllowedRoles);
    }

    private sealed class FakeRepository : ICompaniesAgenciesAdministrationRepository
    {
        private readonly List<Company> companies = [];
        private readonly List<Agency> agencies = [];

        public void Seed(Company company) => companies.Add(company);

        public Task<IReadOnlyList<Company>> GetCompaniesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Company>>(companies);
        public Task<Company?> GetCompanyAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(companies.SingleOrDefault(company => company.Id == id));
        public Task<bool> TaxIdExistsAsync(string taxId, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(companies.Any(company => company.TaxId == taxId && company.Id != excludingId));
        public Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default)
        {
            companies.Add(company);
            return Task.CompletedTask;
        }
        public Task SaveCompanyAsync(Company company, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Agency>> GetAgenciesAsync(Guid companyId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Agency>>(agencies.Where(agency => agency.CompanyId == companyId).ToArray());
        public Task<Agency?> GetAgencyAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(agencies.SingleOrDefault(agency => agency.Id == id));
        public Task<bool> CompanyIsActiveAsync(Guid companyId, CancellationToken cancellationToken = default) =>
            Task.FromResult(companies.Any(company => company.Id == companyId && company.IsActive));
        public Task<IReadOnlyList<AgencyCompanyStatus>> GetAgencyCompanyStatusesAsync(
            IReadOnlyCollection<Guid> agencyIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AgencyCompanyStatus>>(agencies.Where(agency => agencyIds.Contains(agency.Id))
                .Select(agency => new AgencyCompanyStatus(agency.Id, agency.CompanyId, agency.IsActive,
                    companies.Any(company => company.Id == agency.CompanyId && company.IsActive))).ToArray());
        public Task AddAgencyAsync(Agency agency, CancellationToken cancellationToken = default)
        {
            agencies.Add(agency);
            return Task.CompletedTask;
        }
        public Task SaveAgencyAsync(Agency agency, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
