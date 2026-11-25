using TransportPlatform.Modules.QueriesReports.Application.Contracts;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.QueriesReports.Application.Services;

public sealed class QueriesReportsCacheControl(ITripSearchCache tripSearchCache) : IQueriesReportsCacheControl
{
    public Task InvalidateTripSearchAsync(CancellationToken cancellationToken = default) =>
        tripSearchCache.InvalidateAsync(cancellationToken);
}
