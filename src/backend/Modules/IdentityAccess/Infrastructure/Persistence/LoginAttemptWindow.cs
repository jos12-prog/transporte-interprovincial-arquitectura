namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

public sealed class LoginAttemptWindow
{
    public string IdentifierHash { get; set; } = string.Empty;
    public int FailureCount { get; set; }
    public DateTimeOffset WindowStartedAtUtc { get; set; }
    public DateTimeOffset? BlockedUntilUtc { get; set; }
}
