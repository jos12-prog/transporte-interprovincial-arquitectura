namespace TransportPlatform.Modules.IdentityAccess.Application.Contracts;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
