using Microsoft.Extensions.Configuration;

namespace TransportPlatform.Shared.Infrastructure.Health;

/// <summary>Resolved host and port for a dependency connectivity check.</summary>
public sealed record DependencyEndpoint(string Host, int Port)
{
    public static DependencyEndpoint FromConfiguration(
        IConfiguration configuration,
        string hostKey,
        string portKey,
        string defaultHost,
        int defaultPort)
    {
        var configuredHost = configuration[hostKey];
        var host = string.IsNullOrWhiteSpace(configuredHost) ? defaultHost : configuredHost;
        var hasPort = int.TryParse(configuration[portKey], out var configuredPort);
        var port = hasPort && configuredPort is > 0 and <= 65535 ? configuredPort : defaultPort;
        return new DependencyEndpoint(host, port);
    }
}
