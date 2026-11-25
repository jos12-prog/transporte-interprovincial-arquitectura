using Microsoft.AspNetCore.Identity;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;

public sealed class AspNetPasswordHasher(IPasswordHasher<User> passwordHasher) : IPasswordHasher
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
