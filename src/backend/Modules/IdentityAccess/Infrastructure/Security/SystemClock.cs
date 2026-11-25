using TransportPlatform.Modules.IdentityAccess.Application.Contracts;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
