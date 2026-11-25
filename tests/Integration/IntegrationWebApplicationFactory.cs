using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TransportPlatform.Integration.Tests;

/// <summary>Starts the host with isolated test credentials and container-specific endpoints.</summary>
internal sealed class IntegrationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _keyDirectory;
    private readonly Dictionary<string, string?> _settings;
    private readonly Dictionary<string, string?> _previousEnvironment;

    public IntegrationWebApplicationFactory(IReadOnlyDictionary<string, string?> overrides)
    {
        _keyDirectory = Path.Combine(Path.GetTempPath(), $"transport-host-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_keyDirectory);
        using var rsa = RSA.Create(2048);
        var privateKeyPath = Path.Combine(_keyDirectory, "private.pem");
        var publicKeyPath = Path.Combine(_keyDirectory, "public.pem");
        File.WriteAllText(privateKeyPath, rsa.ExportRSAPrivateKeyPem());
        File.WriteAllText(publicKeyPath, rsa.ExportRSAPublicKeyPem());

        _settings = new Dictionary<string, string?>
        {
            ["JWT_ISSUER"] = "transport-integration-tests",
            ["JWT_SIGNING_PRIVATE_KEY_PATH"] = privateKeyPath,
            ["JWT_SIGNING_PUBLIC_KEY_PATH"] = publicKeyPath,
            ["LOGIN_ATTEMPT_HMAC_KEY"] = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            ["IDENTITY_SEED_SUPER_ADMIN_PASSWORD"] = "Integration-superadmin-password",
            ["IDENTITY_SEED_AGENCY_ADMIN_PASSWORD"] = "Integration-agencyadmin-password",
            ["IDENTITY_SEED_AGENT_PASSWORD"] = "Integration-agent-password"
        };
        foreach (var (key, value) in overrides) _settings[key] = value;

        _previousEnvironment = _settings.Keys.ToDictionary(
            key => key,
            key => Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process));
        foreach (var (key, value) in _settings)
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        foreach (var (key, value) in _settings) builder.UseSetting(key, value);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(_settings));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        foreach (var (key, value) in _previousEnvironment)
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
        if (Directory.Exists(_keyDirectory)) Directory.Delete(_keyDirectory, recursive: true);
    }
}
