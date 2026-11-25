using System.Security.Cryptography;
using System.Text;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;

public sealed class HmacIdentifierHasher
{
    private readonly byte[] _key;

    public HmacIdentifierHasher(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("LOGIN_ATTEMPT_HMAC_KEY must contain at least 32 bytes.");
        _key = Encoding.UTF8.GetBytes(key);
    }

    public string Hash(string normalizedIdentifier) => Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(normalizedIdentifier)));
}
