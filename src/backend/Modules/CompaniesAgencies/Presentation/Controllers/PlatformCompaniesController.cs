using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportPlatform.Modules.CompaniesAgencies.Application.UseCases;
using TransportPlatform.Shared.Contracts.Administration;
using TransportPlatform.Shared.Contracts.Companies;
using TransportPlatform.Shared.Contracts.Modules;

namespace TransportPlatform.Modules.CompaniesAgencies.Presentation.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "SuperAdmin")]
public sealed class PlatformCompaniesController(ICompaniesAgenciesAdminApi companies) : ControllerBase
{
    [HttpGet("companies")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCompanies(CancellationToken cancellationToken) =>
        Ok(await companies.GetCompaniesAsync(cancellationToken));

    [HttpGet("companies/{companyId:guid}")]
    [ProducesResponseType(typeof(CompanyAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompany(Guid companyId, CancellationToken cancellationToken)
    {
        var company = await companies.GetCompanyAsync(companyId, cancellationToken);
        return company is null ? NotFound(Problem(statusCode: 404, title: "Empresa no encontrada.")) : Ok(company);
    }

    [HttpPost("companies")]
    [ProducesResponseType(typeof(CompanyAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCompany(SaveCompanyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var company = await companies.CreateCompanyAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCompany), new { companyId = company.CompanyId }, company);
        }
        catch (CatalogValidationException exception) { return Problem(statusCode: 400, title: "Empresa inválida.", detail: exception.Message); }
        catch (CatalogConflictException exception) { return Problem(statusCode: 409, title: "Conflicto de empresa.", detail: exception.Message); }
    }

    [HttpPut("companies/{companyId:guid}")]
    [ProducesResponseType(typeof(CompanyAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCompany(Guid companyId, SaveCompanyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var company = await companies.UpdateCompanyAsync(companyId, request, cancellationToken);
            return company is null ? NotFound(Problem(statusCode: 404, title: "Empresa no encontrada.")) : Ok(company);
        }
        catch (CatalogValidationException exception) { return Problem(statusCode: 400, title: "Empresa inválida.", detail: exception.Message); }
        catch (CatalogNotFoundException exception) { return Problem(statusCode: 404, title: "Empresa no encontrada.", detail: exception.Message); }
        catch (CatalogConflictException exception) { return Problem(statusCode: 409, title: "Conflicto de empresa.", detail: exception.Message); }
    }

    [HttpPatch("companies/{companyId:guid}/status")]
    [ProducesResponseType(typeof(CompanyAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCompanyStatus(Guid companyId, SetActiveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var company = await companies.SetCompanyActiveAsync(companyId, request.IsActive, cancellationToken);
            return company is null ? NotFound(Problem(statusCode: 404, title: "Empresa no encontrada.")) : Ok(company);
        }
        catch (CatalogNotFoundException exception) { return Problem(statusCode: 404, title: "Empresa no encontrada.", detail: exception.Message); }
    }

    [HttpGet("companies/{companyId:guid}/agencies")]
    [ProducesResponseType(typeof(IReadOnlyList<AgencyAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAgencies(Guid companyId, CancellationToken cancellationToken) =>
        Ok(await companies.GetAgenciesAsync(companyId, cancellationToken));

    [HttpPost("companies/{companyId:guid}/agencies")]
    [ProducesResponseType(typeof(AgencyAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAgency(Guid companyId, SaveAgencyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var agency = await companies.CreateAgencyAsync(companyId, request, cancellationToken);
            if (agency is null) return NotFound(Problem(statusCode: 404, title: "Empresa no encontrada."));
            return CreatedAtAction(nameof(GetAgency), new { agencyId = agency.AgencyId }, agency);
        }
        catch (CatalogValidationException exception) { return Problem(statusCode: 400, title: "Agencia inválida.", detail: exception.Message); }
        catch (CatalogNotFoundException exception) { return Problem(statusCode: 404, title: "Empresa no encontrada.", detail: exception.Message); }
    }

    [HttpGet("agencies/{agencyId:guid}")]
    [ProducesResponseType(typeof(AgencyAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgency(Guid agencyId, CancellationToken cancellationToken)
    {
        var agency = await companies.GetAgencyAsync(agencyId, cancellationToken);
        return agency is null ? NotFound(Problem(statusCode: 404, title: "Agencia no encontrada.")) : Ok(agency);
    }

    [HttpPut("agencies/{agencyId:guid}")]
    [ProducesResponseType(typeof(AgencyAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAgency(Guid agencyId, SaveAgencyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var agency = await companies.UpdateAgencyAsync(agencyId, request, cancellationToken);
            return agency is null ? NotFound(Problem(statusCode: 404, title: "Agencia no encontrada.")) : Ok(agency);
        }
        catch (CatalogValidationException exception) { return Problem(statusCode: 400, title: "Agencia inválida.", detail: exception.Message); }
        catch (CatalogNotFoundException exception) { return Problem(statusCode: 404, title: "Agencia no encontrada.", detail: exception.Message); }
        catch (CatalogConflictException exception) { return Problem(statusCode: 409, title: "Conflicto de agencia.", detail: exception.Message); }
    }

    [HttpPatch("agencies/{agencyId:guid}/status")]
    [ProducesResponseType(typeof(AgencyAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAgencyStatus(Guid agencyId, SetActiveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var agency = await companies.SetAgencyActiveAsync(agencyId, request.IsActive, cancellationToken);
            return agency is null ? NotFound(Problem(statusCode: 404, title: "Agencia no encontrada.")) : Ok(agency);
        }
        catch (CatalogNotFoundException exception) { return Problem(statusCode: 404, title: "Agencia no encontrada.", detail: exception.Message); }
    }
}

