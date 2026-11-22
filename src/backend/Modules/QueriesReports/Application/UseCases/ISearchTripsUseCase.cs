namespace TransportPlatform.Modules.QueriesReports.Application.UseCases;

public interface ISearchTripsUseCase
{
    Task<TripSearchExecutionResult> ExecuteAsync(
        SearchTripsRequest request,
        CancellationToken cancellationToken = default);
}
