using TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;
using TransportPlatform.Shared.Contracts.Companies;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.Services;

public sealed class CompaniesAgenciesApi(IAgencyDirectoryRepository repository) : ICompaniesAgenciesApi
{
    public Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agencyIds);
        if (agencyIds.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<PublicAgencySummary>>(Array.Empty<PublicAgencySummary>());
        }

        return repository.GetPublicAgenciesAsync(agencyIds.Distinct().ToArray(), cancellationToken);
    }
}
