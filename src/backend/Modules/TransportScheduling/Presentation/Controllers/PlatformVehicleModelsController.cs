using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportPlatform.Modules.TransportScheduling.Application.UseCases;
using TransportPlatform.Shared.Contracts.Administration;
using TransportPlatform.Shared.Contracts.Modules;
using TransportPlatform.Shared.Contracts.Trips;

namespace TransportPlatform.Modules.TransportScheduling.Presentation.Controllers;

[ApiController]
[Route("api/v1/admin/vehicle-models")]
[Authorize(Policy = "SuperAdmin")]
public sealed class PlatformVehicleModelsController(ITransportSchedulingAdminApi scheduling) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleModelAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] bool activeOnly, CancellationToken cancellationToken) =>
        Ok(await scheduling.GetVehicleModelsAsync(activeOnly, cancellationToken));

    [HttpGet("{vehicleModelId:guid}")]
    [ProducesResponseType(typeof(VehicleModelAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid vehicleModelId, CancellationToken cancellationToken)
    {
        var model = await scheduling.GetVehicleModelAsync(vehicleModelId, cancellationToken);
        return model is null ? NotFound(Problem(statusCode: 404, title: "Modelo no encontrado.")) : Ok(model);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleModelAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(SaveVehicleModelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var model = await scheduling.CreateVehicleModelAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { vehicleModelId = model.VehicleModelId }, model);
        }
        catch (VehicleModelValidationException exception) { return Problem(statusCode: 400, title: "Modelo inválido.", detail: exception.Message); }
        catch (VehicleModelConflictException exception) { return Problem(statusCode: 409, title: "Conflicto de modelo.", detail: exception.Message); }
    }

    [HttpPut("{vehicleModelId:guid}")]
    [ProducesResponseType(typeof(VehicleModelAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid vehicleModelId, SaveVehicleModelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var model = await scheduling.UpdateVehicleModelAsync(vehicleModelId, request, cancellationToken);
            return model is null ? NotFound(Problem(statusCode: 404, title: "Modelo no encontrado.")) : Ok(model);
        }
        catch (VehicleModelValidationException exception) { return Problem(statusCode: 400, title: "Modelo inválido.", detail: exception.Message); }
        catch (VehicleModelNotFoundException exception) { return Problem(statusCode: 404, title: "Modelo no encontrado.", detail: exception.Message); }
        catch (VehicleModelConflictException exception) { return Problem(statusCode: 409, title: "Conflicto de modelo.", detail: exception.Message); }
    }

    [HttpPatch("{vehicleModelId:guid}/status")]
    [ProducesResponseType(typeof(VehicleModelAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(Guid vehicleModelId, SetActiveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var model = await scheduling.SetVehicleModelActiveAsync(vehicleModelId, request.IsActive, cancellationToken);
            return model is null ? NotFound(Problem(statusCode: 404, title: "Modelo no encontrado.")) : Ok(model);
        }
        catch (VehicleModelNotFoundException exception) { return Problem(statusCode: 404, title: "Modelo no encontrado.", detail: exception.Message); }
    }
}
