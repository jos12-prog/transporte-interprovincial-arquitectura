using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Application.Models;
using TransportPlatform.Modules.IdentityAccess.Application.UseCases;

namespace TransportPlatform.Modules.IdentityAccess.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    RegisterPassengerUseCase registerPassenger,
    LoginUseCase login,
    GetCurrentUserUseCase getCurrentUser,
    IUserRepository users) : ControllerBase
{
    /// <summary>Registra una cuenta de pasajero.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await registerPassenger.ExecuteAsync(new RegisterPassengerCommand(
            request.FullName, request.Dni, request.Email, request.PhoneNumber, request.Password), cancellationToken);
        if (!result.Succeeded)
            return Problem(statusCode: result.Failure == IdentityFailure.Conflict ? 409 : 400,
                title: result.Failure == IdentityFailure.Conflict ? "La cuenta ya existe." : "Los datos enviados no son válidos.");
        return CreatedAtAction(nameof(Me), new { }, new { userId = result.Value });
    }

    /// <summary>Inicia sesión para pasajeros y personal.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("login-by-ip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await login.ExecuteAsync(new LoginCommand(request.Identifier, request.Password), cancellationToken);
        if (!result.Succeeded)
        {
            if (result.Failure == IdentityFailure.RateLimited)
            {
                Response.Headers["Retry-After"] = "300";
                return Problem(statusCode: 429, title: "Se excedió el límite de intentos.");
            }
            return Problem(statusCode: result.Failure == IdentityFailure.InvalidInput ? 400 : 401,
                title: result.Failure == IdentityFailure.InvalidInput ? "Las credenciales enviadas no son válidas." : "Credenciales incorrectas.");
        }

        var access = result.Value!;
        return Ok(new
        {
            accessToken = access.Token,
            tokenType = "Bearer",
            expiresAtUtc = access.ExpiresAtUtc,
            user = new { id = access.User.Id, role = access.User.Role }
        });
    }

    /// <summary>Devuelve el perfil de la sesión autenticada.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var id)) return Unauthorized();
        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null) return Unauthorized();
        return Ok(getCurrentUser.Execute(user));
    }

    public sealed record RegisterRequest(string FullName, string Dni, string Email, string PhoneNumber, string Password);
    public sealed record LoginRequest(string Identifier, string Password);
}
