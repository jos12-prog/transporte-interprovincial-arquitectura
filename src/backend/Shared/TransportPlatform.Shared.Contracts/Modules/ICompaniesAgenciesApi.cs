using TransportPlatform.Shared.Contracts.Companies;

namespace TransportPlatform.Shared.Contracts.Modules;

public interface ICompaniesAgenciesApi
{
    Task<IReadOnlyList<PublicAgencySummary>> GetPublicAgenciesAsync(
        IReadOnlyCollection<Guid> agencyIds,
        CancellationToken cancellationToken = default);
}
