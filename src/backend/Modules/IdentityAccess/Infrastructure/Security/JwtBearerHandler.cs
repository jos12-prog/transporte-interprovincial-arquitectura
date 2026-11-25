using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;

public sealed class JwtBearerHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAccessTokenValidator tokens) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)) return Task.FromResult(AuthenticateResult.NoResult());
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme."));
        var principal = tokens.Validate(authorization[7..].Trim());
        if (principal is null) return Task.FromResult(AuthenticateResult.Fail("Invalid access token."));
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
