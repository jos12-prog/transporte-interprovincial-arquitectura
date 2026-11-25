using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Application.Contracts;

public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}
