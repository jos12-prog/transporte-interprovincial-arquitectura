namespace TransportPlatform.Modules.IdentityAccess.Application.Contracts;

public interface ILoginAttemptStore
{
    Task<bool> IsBlockedAsync(string normalizedIdentifier, CancellationToken cancellationToken);
    Task RegisterFailureAsync(string normalizedIdentifier, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task ResetAsync(string normalizedIdentifier, CancellationToken cancellationToken);
}
