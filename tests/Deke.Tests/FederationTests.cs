using Deke.Core.Models;

namespace Deke.Tests;

public class FederationPeerTests
{
    [Fact]
    public void FederationPeer_HasDefaults()
    {
        var peer = new FederationPeer
        {
            InstanceId = "test-instance",
            BaseUrl = "https://test.example.com"
        };

        Assert.NotEqual(Guid.Empty, peer.Id);
        Assert.Equal("test-instance", peer.InstanceId);
        Assert.Equal("https://test.example.com", peer.BaseUrl);
        Assert.Empty(peer.Domains);
        Assert.Empty(peer.Capabilities);
        Assert.Equal("1", peer.ProtocolVersion);
        Assert.True(peer.IsHealthy);
    }

    [Fact]
    public void FederationPeer_AcceptsDomainInfo()
    {
        var peer = new FederationPeer
        {
            InstanceId = "wildlife-expert",
            BaseUrl = "https://wildlife.internal:5000",
            Domains =
            [
                new PeerDomainInfo
                {
                    Name = "wildlife",
                    FactCount = 500,
                    LastUpdatedAt = DateTimeOffset.UtcNow,
                    Confidence = 0.92f
                }
            ],
            Capabilities = ["search", "replicate"]
        };

        Assert.Single(peer.Domains);
        Assert.Equal("wildlife", peer.Domains[0].Name);
        Assert.Equal(2, peer.Capabilities.Count);
    }
}

public class FederationConfigTests
{
    [Fact]
    public void FederationConfig_HasCorrectDefaults()
    {
        var config = new FederationConfig();

        Assert.False(config.Enabled);
        Assert.Equal("default", config.InstanceId);
        Assert.Equal(3, config.MaxHops);
        Assert.Equal(5000, config.TimeoutMs);
        Assert.Equal(0.4f, config.DelegationThreshold);
        Assert.Empty(config.Peers);
    }

    [Fact]
    public void FederationConfig_AcceptsPeerEntries()
    {
        var config = new FederationConfig
        {
            Enabled = true,
            InstanceId = "fishing-expert",
            Peers =
            [
                new PeerConfigEntry
                {
                    InstanceId = "wildlife-expert",
                    BaseUrl = "https://wildlife.internal:5000"
                }
            ]
        };

        Assert.True(config.Enabled);
        Assert.Single(config.Peers);
        Assert.Equal("wildlife-expert", config.Peers[0].InstanceId);
    }
}

public class FederationManifestTests
{
    [Fact]
    public void FederationManifest_ConstructsCorrectly()
    {
        var manifest = new FederationManifest
        {
            InstanceId = "fishing-expert",
            Version = "1.0.0",
            ProtocolVersion = "1",
            Domains =
            [
                new PeerDomainInfo
                {
                    Name = "fishing",
                    FactCount = 1247,
                    LastUpdatedAt = new DateTimeOffset(2026, 3, 28, 14, 0, 0, TimeSpan.Zero),
                    Confidence = 0.92f
                }
            ],
            Capabilities = ["search"],
            RegisteredAt = DateTimeOffset.UtcNow
        };

        Assert.Equal("fishing-expert", manifest.InstanceId);
        Assert.Equal("1", manifest.ProtocolVersion);
        Assert.Single(manifest.Domains);
        Assert.Equal(1247, manifest.Domains[0].FactCount);
        Assert.Single(manifest.Capabilities);
    }

    [Fact]
    public void DomainStats_ConstructsCorrectly()
    {
        var stats = new DomainStats
        {
            Domain = "fishing",
            FactCount = 500,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal("fishing", stats.Domain);
        Assert.Equal(500, stats.FactCount);
    }
}
