using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TransportPlatform.Modules.IdentityAccess.Application.Contracts;
using TransportPlatform.Modules.IdentityAccess.Domain.Entities;

namespace TransportPlatform.Modules.IdentityAccess.Infrastructure.Security;

public sealed class JwtAccessTokenService : IAccessTokenIssuer, IAccessTokenValidator, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RSA _privateKey;
    private readonly RSA _publicKey;
    private readonly string _issuer;

    public JwtAccessTokenService(string issuer, string privateKeyPem, string publicKeyPem)
    {
        if (string.IsNullOrWhiteSpace(issuer)) throw new InvalidOperationException("JWT issuer is required.");
        if (string.IsNullOrWhiteSpace(privateKeyPem) || string.IsNullOrWhiteSpace(publicKeyPem))
            throw new InvalidOperationException("JWT signing and verification keys are required.");
        _issuer = issuer;
        _privateKey = RSA.Create();
        _publicKey = RSA.Create();
        _privateKey.ImportFromPem(privateKeyPem);
        _publicKey.ImportFromPem(publicKeyPem);
    }

    public string Issue(User user, DateTimeOffset issuedAtUtc)
    {
        var expires = issuedAtUtc.AddMinutes(60);
        var payload = new Dictionary<string, object>
        {
            ["iss"] = _issuer,
            ["sub"] = user.Id.ToString(),
            ["role"] = user.Role,
            ["iat"] = issuedAtUtc.ToUnixTimeSeconds(),
            ["nbf"] = issuedAtUtc.ToUnixTimeSeconds(),
            ["exp"] = expires.ToUnixTimeSeconds()
        };
        var companyIds = user.AgencyAssignments.Select(a => a.CompanyId).Distinct().ToArray();
        if (companyIds.Length == 1) payload["company_id"] = companyIds[0].ToString();
        var agencyIds = user.AgencyAssignments.Select(a => a.AgencyId).Distinct().ToArray();
        if (agencyIds.Length > 0) payload["agency_ids"] = agencyIds.Select(id => id.ToString()).ToArray();

        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }, JsonOptions));
        var body = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions));
        var signingInput = $"{header}.{body}";
        var signature = _privateKey.SignData(Encoding.ASCII.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{signingInput}.{Base64Url(signature)}";
    }

    public ClaimsPrincipal? Validate(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;
            var header = JsonDocument.Parse(FromBase64Url(parts[0])).RootElement;
            if (header.GetProperty("alg").GetString() != "RS256") return null;
            var signed = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
            if (!_publicKey.VerifyData(signed, FromBase64Url(parts[2]), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)) return null;
            using var payload = JsonDocument.Parse(FromBase64Url(parts[1]));
            var root = payload.RootElement;
            if (root.GetProperty("iss").GetString() != _issuer ||
                root.GetProperty("exp").GetInt64() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds() ||
                root.GetProperty("nbf").GetInt64() > DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, root.GetProperty("sub").GetString()!),
                new("role", root.GetProperty("role").GetString()!)
            };
            if (root.TryGetProperty("company_id", out var companyId)) claims.Add(new Claim("company_id", companyId.GetString()!));
            if (root.TryGetProperty("agency_ids", out var agencyIds))
                claims.AddRange(agencyIds.EnumerateArray().Select(id => new Claim("agency_ids", id.GetString()!)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer", ClaimTypes.NameIdentifier, "role"));
        }
        catch (Exception exception) when (exception is FormatException or JsonException or CryptographicException or InvalidOperationException or KeyNotFoundException)
        {
            return null;
        }
    }

    public void Dispose() { _privateKey.Dispose(); _publicKey.Dispose(); }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] FromBase64Url(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        normalized += (normalized.Length % 4) switch { 2 => "==", 3 => "=", _ => string.Empty };
        return Convert.FromBase64String(normalized);
    }
}
