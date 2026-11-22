using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Modules.CompaniesAgencies.Application.Contracts;

public interface IAgencyDirectoryRepository
{
    Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default);
}
