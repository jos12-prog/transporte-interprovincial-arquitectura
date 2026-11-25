using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Application.Contracts;

public interface IAccessTokenIssuer
{
    string Issue(User user, DateTimeOffset issuedAtUtc);
}
