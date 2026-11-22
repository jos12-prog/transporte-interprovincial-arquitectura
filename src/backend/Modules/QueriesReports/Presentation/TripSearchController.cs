using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TransportPlatform.Modules.QueriesReports.Application.UseCases;

namespace TransportPlatform.Modules.QueriesReports.Presentation;

/// <summary>Provides anonymous public trip-search results.</summary>
[ApiController]
[Route("api/v1/trips")]
[Produces("application/json")]
public sealed class TripSearchController(
    ISearchTripsUseCase searchTripsUseCase,
    ILogger<TripSearchController> logger) : ControllerBase
{
    /// <summary>Searches scheduled trips by origin, destination, and local service date.</summary>
    /// <param name="origin">Origin city name.</param>
    /// <param name="destination">Destination city name.</param>
    /// <param name="date">Service date in yyyy-MM-dd format, interpreted in America/Lima.</param>
    /// <param name="companyId">Optional company identifier.</param>
    /// <param name="agencyId">Optional agency identifier.</param>
    /// <param name="departureFrom">Optional earliest local departure time, HH:mm or HH:mm:ss.</param>
    /// <param name="departureTo">Optional latest local departure time, HH:mm or HH:mm:ss.</param>
    /// <param name="minPrice">Optional inclusive minimum fare.</param>
    /// <param name="maxPrice">Optional inclusive maximum fare.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Public trip results and an X-Cache header.</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<TripSearchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string? origin,
        [FromQuery] string? destination,
        [FromQuery(Name = "date")] string? date,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? agencyId,
        [FromQuery] string? departureFrom,
        [FromQuery] string? departureTo,
        [FromQuery(Name = "minPrice")] decimal? minPrice,
        [FromQuery(Name = "maxPrice")] decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        DateOnly? serviceDate = null;
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            errors["date"] = ["Date is required and must use yyyy-MM-dd format."];
        }
        else
        {
            serviceDate = parsedDate;
        }

        var parsedDepartureFrom = ParseTime(departureFrom, "departureFrom", errors);
        var parsedDepartureTo = ParseTime(departureTo, "departureTo", errors);
        if (errors.Count > 0)
        {
            return BadRequest(CreateValidationProblem(errors));
        }

        try
        {
            var result = await searchTripsUseCase.ExecuteAsync(new SearchTripsRequest(
                origin,
                destination,
                serviceDate,
                companyId,
                agencyId,
                parsedDepartureFrom,
                parsedDepartureTo,
                minPrice,
                maxPrice), cancellationToken);
            Response.Headers["X-Cache"] = result.CacheHit ? "HIT" : "MISS";
            return Ok(result.Results);
        }
        catch (SearchTripsValidationException exception)
        {
            return BadRequest(CreateValidationProblem(exception.Errors.ToDictionary(item => item.Key, item => item.Value)));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Public trip search failed.");
            return Problem(
                title: "Trip search failed.",
                detail: "The trip search could not be completed.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static TimeOnly? ParseTime(string? value, string parameter, IDictionary<string, string[]> errors)
    {
        if (value is null)
        {
            return null;
        }

        if (TimeOnly.TryParseExact(value, ["HH:mm", "HH:mm:ss"], CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        errors[parameter] = [$"{parameter} must use HH:mm or HH:mm:ss format."];
        return null;
    }

    private static ValidationProblemDetails CreateValidationProblem(IDictionary<string, string[]> errors) => new(errors)
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "One or more validation errors occurred."
    };
}
