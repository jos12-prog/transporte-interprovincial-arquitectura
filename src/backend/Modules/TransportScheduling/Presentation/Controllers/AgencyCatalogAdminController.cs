using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportPlatform.Modules.TransportScheduling.Application.Contracts;
using TransportPlatform.Shared.Contracts.Administration;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Presentation.Controllers;

[ApiController]
[Route("api/v1/admin/agencies/{agencyId:guid}")]
[Authorize(Policy = "AgencyScope")]
[Authorize(Policy = "AgencyCatalogAdmin")]
public sealed class AgencyCatalogAdminController(IAgencyCatalogAdminApi catalog, ITransportSchedulingAdminApi scheduling) : ControllerBase
{
    [HttpGet("vehicle-models")]
    public Task<IReadOnlyList<VehicleModelAdminDto>> VehicleModels(CancellationToken ct) => scheduling.GetVehicleModelsAsync(true, ct);
    [HttpGet("routes")]
    public Task<IReadOnlyList<RouteAdminDto>> Routes(Guid agencyId, CancellationToken ct) => catalog.GetRoutesAsync(Scope(agencyId), ct);
    [HttpPost("routes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoute(Guid agencyId, SaveRouteRequest request, CancellationToken ct) =>
        await Execute(async () => Created("", await catalog.CreateRouteAsync(request, Scope(agencyId), ct)));
    [HttpPut("routes/{id:guid}")]
    public async Task<IActionResult> UpdateRoute(Guid agencyId, Guid id, SaveRouteRequest request, CancellationToken ct) =>
        await Execute(async () => (await catalog.UpdateRouteAsync(id, request, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());
    [HttpPatch("routes/{id:guid}/active")]
    public async Task<IActionResult> SetRouteActive(Guid agencyId, Guid id, SetActiveRequest request, CancellationToken ct) =>
        await Execute(async () => (await catalog.SetRouteActiveAsync(id, request.IsActive, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());

    [HttpGet("vehicles")]
    public Task<IReadOnlyList<VehicleAdminDto>> Vehicles(Guid agencyId, CancellationToken ct) => catalog.GetVehiclesAsync(Scope(agencyId), ct);
    [HttpPost("vehicles")]
    public async Task<IActionResult> CreateVehicle(Guid agencyId, SaveVehicleRequest request, CancellationToken ct) =>
        await Execute(async () => Created("", await catalog.CreateVehicleAsync(request, Scope(agencyId), ct)));
    [HttpPut("vehicles/{id:guid}")]
    public async Task<IActionResult> UpdateVehicle(Guid agencyId, Guid id, SaveVehicleRequest request, CancellationToken ct) =>
        await Execute(async () => (await catalog.UpdateVehicleAsync(id, request, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());
    [HttpPatch("vehicles/{id:guid}/active")]
    public async Task<IActionResult> SetVehicleActive(Guid agencyId, Guid id, SetActiveRequest request, CancellationToken ct) =>
        await Execute(async () => (await catalog.SetVehicleActiveAsync(id, request.IsActive, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());

    [HttpGet("routes/{routeId:guid}/fares")]
    public Task<IReadOnlyList<FareAdminDto>> Fares(Guid agencyId, Guid routeId, CancellationToken ct) => catalog.GetFaresAsync(routeId, Scope(agencyId), ct);
    [HttpPost("routes/{routeId:guid}/fares")]
    public async Task<IActionResult> CreateFare(Guid agencyId, Guid routeId, SaveFareRequest request, CancellationToken ct) =>
        await Execute(async () => Created("", await catalog.CreateFareAsync(routeId, request, Scope(agencyId), ct)));
    [HttpPut("routes/{routeId:guid}/fares/{id:guid}")]
    public async Task<IActionResult> UpdateFare(Guid agencyId, Guid routeId, Guid id, SaveFareRequest request, CancellationToken ct) =>
        await Execute(async () => (await catalog.UpdateFareAsync(routeId, id, request, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());
    [HttpPatch("routes/{routeId:guid}/fares/{id:guid}/active")]
    public async Task<IActionResult> SetFareActive(Guid agencyId, Guid routeId, Guid id, SetActiveRequest request, CancellationToken ct) =>
        await Execute(async () => (await catalog.SetFareActiveAsync(routeId, id, request.IsActive, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());

    [HttpGet("trips")]
    public Task<IReadOnlyList<TripAdminDto>> Trips(Guid agencyId, CancellationToken ct) => catalog.GetTripsAsync(Scope(agencyId), ct);
    [HttpPost("trips")]
    public async Task<IActionResult> ScheduleTrip(Guid agencyId, ScheduleTripRequest request, CancellationToken ct) =>
        await Execute(async () => Created("", await catalog.ScheduleTripAsync(request, Scope(agencyId), ct)));
    [HttpPost("trips/{id:guid}/cancel")]
    public async Task<IActionResult> CancelTrip(Guid agencyId, Guid id, CancellationToken ct) =>
        await Execute(async () => (await catalog.CancelTripAsync(id, Scope(agencyId), ct)) is { } dto ? Ok(dto) : NotFound());

    private AdminAgencyScope Scope(Guid agencyId) => Guid.TryParse(User.FindFirstValue("company_id"), out var companyId)
        ? new AdminAgencyScope(companyId, agencyId)
        : throw new AgencyCatalogForbiddenException("The staff token does not contain a company scope.");

    private async Task<IActionResult> Execute(Func<Task<IActionResult>> operation)
    {
        try { return await operation(); }
        catch (AgencyCatalogValidationException ex) { return Problem(statusCode: 400, title: "Solicitud inválida.", detail: ex.Message); }
        catch (AgencyCatalogForbiddenException ex) { return Problem(statusCode: 403, title: "Acceso denegado.", detail: ex.Message); }
        catch (AgencyCatalogNotFoundException ex) { return Problem(statusCode: 404, title: "Recurso no encontrado.", detail: ex.Message); }
        catch (AgencyCatalogConflictException ex) { return Problem(statusCode: 409, title: "Conflicto de catálogo.", detail: ex.Message); }
        catch (ArgumentException ex) { return Problem(statusCode: 400, title: "Solicitud inválida.", detail: ex.Message); }
        catch (InvalidOperationException ex) { return Problem(statusCode: 409, title: "Conflicto de catálogo.", detail: ex.Message); }
    }
}
