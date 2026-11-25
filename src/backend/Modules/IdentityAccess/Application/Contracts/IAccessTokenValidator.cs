using System.Security.Claims;

namespace TransportPlatform.Modules.IdentityAccess.Application.Contracts;

public interface IAccessTokenValidator
{
    ClaimsPrincipal? Validate(string token);
}
