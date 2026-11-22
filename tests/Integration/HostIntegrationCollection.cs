using Xunit;

namespace TransportPlatform.Integration.Tests;

[CollectionDefinition("Host integration", DisableParallelization = true)]
public sealed class HostIntegrationCollection
{
    public const string Name = "Host integration";
}
