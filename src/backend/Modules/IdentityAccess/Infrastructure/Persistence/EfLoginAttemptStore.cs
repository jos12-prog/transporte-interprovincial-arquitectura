using Microsoft.EntityFrameworkCore;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Persistence;

public sealed class EfLoginAttemptStore(IdentityAccessDbContext dbContext, HmacIdentifierHasher hasher) : ILoginAttemptStore
{
    public Task<bool> IsBlockedAsync(string normalizedIdentifier, CancellationToken cancellationToken) =>
        dbContext.LoginAttemptWindows.AnyAsync(window => window.IdentifierHash == hasher.Hash(normalizedIdentifier) &&
            window.BlockedUntilUtc != null && window.BlockedUntilUtc > DateTimeOffset.UtcNow, cancellationToken);

    public async Task RegisterFailureAsync(string normalizedIdentifier, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var hash = hasher.Hash(normalizedIdentifier);
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO identity_access.login_attempt_windows
                (identifier_hash, failure_count, window_started_at_utc, blocked_until_utc)
            VALUES ({hash}, 1, {nowUtc}, NULL)
            ON CONFLICT (identifier_hash) DO UPDATE SET
                failure_count = CASE
                    WHEN identity_access.login_attempt_windows.window_started_at_utc <= {nowUtc} - INTERVAL '5 minutes' THEN 1
                    ELSE identity_access.login_attempt_windows.failure_count + 1
                END,
                window_started_at_utc = CASE
                    WHEN identity_access.login_attempt_windows.window_started_at_utc <= {nowUtc} - INTERVAL '5 minutes' THEN {nowUtc}
                    ELSE identity_access.login_attempt_windows.window_started_at_utc
                END,
                blocked_until_utc = CASE
                    WHEN (CASE
                        WHEN identity_access.login_attempt_windows.window_started_at_utc <= {nowUtc} - INTERVAL '5 minutes' THEN 1
                        ELSE identity_access.login_attempt_windows.failure_count + 1
                    END) >= 5 THEN {nowUtc} + INTERVAL '5 minutes'
                    ELSE NULL
                END
            """, cancellationToken);
    }

    public async Task ResetAsync(string normalizedIdentifier, CancellationToken cancellationToken) =>
        _ = await dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM identity_access.login_attempt_windows WHERE identifier_hash = {hasher.Hash(normalizedIdentifier)}", cancellationToken);
}
