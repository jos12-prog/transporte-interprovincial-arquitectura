namespace TransportPlatform.Modules.QueriesReports.Application.UseCases;

public sealed class SearchTripsValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("The trip search request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
