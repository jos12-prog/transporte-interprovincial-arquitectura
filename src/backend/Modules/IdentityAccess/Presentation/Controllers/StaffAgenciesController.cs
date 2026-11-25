using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.IdentityAccess.Presentation.Controllers;

[ApiController]
[Route("api/v1/staff/agencies")]
[Authorize(Policy = "StaffAgencies")]
public sealed class StaffAgenciesController(ICompaniesAgenciesApi companiesAgencies) : ControllerBase
{
    /// <summary>Lista las agencias autorizadas en el token.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var agencyIds = User.FindAll("agency_ids")
            .Select(claim => Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty).Distinct().ToArray();
        return Ok(await companiesAgencies.GetPublicAgenciesAsync(agencyIds, cancellationToken));
    }

    /// <summary>Devuelve una agencia autorizada para la sesión.</summary>
    [HttpGet("{agencyId:guid}")]
    [Authorize(Policy = "AgencyScope")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid agencyId, CancellationToken cancellationToken)
    {
        var agency = (await companiesAgencies.GetPublicAgenciesAsync([agencyId], cancellationToken)).SingleOrDefault();
        return agency is null ? NotFound() : Ok(agency);
    }
}
