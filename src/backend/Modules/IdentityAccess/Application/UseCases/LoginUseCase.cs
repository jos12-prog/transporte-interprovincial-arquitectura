using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Application.Models;

namespace TransportPlatform.Modules.IdentityAccess.Application.UseCases;

public sealed class LoginUseCase(IUserRepository users, IPasswordHasher hasher, IAccessTokenIssuer tokens,
    ILoginAttemptStore attempts, IClock clock)
{
    public async Task<IdentityResult<AccessToken>> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Identifier) || string.IsNullOrEmpty(command.Password))
            return IdentityResult<AccessToken>.Failed(IdentityFailure.InvalidInput);

        var identifier = command.Identifier.Trim().ToUpperInvariant();
        if (await attempts.IsBlockedAsync(identifier, cancellationToken))
            return IdentityResult<AccessToken>.Failed(IdentityFailure.RateLimited);

        var user = await users.FindByIdentifierAsync(identifier, cancellationToken);
        if (user is null || !hasher.Verify(user, command.Password))
        {
            await attempts.RegisterFailureAsync(identifier, clock.UtcNow, cancellationToken);
            return IdentityResult<AccessToken>.Failed(IdentityFailure.InvalidCredentials);
        }

        await attempts.ResetAsync(identifier, cancellationToken);
        return IdentityResult<AccessToken>.Success(new AccessToken(tokens.Issue(user, clock.UtcNow), clock.UtcNow.AddMinutes(60), user));
    }
}
