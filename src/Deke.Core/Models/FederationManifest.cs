namespace Deke.Core.Models;

public record FederationManifest
{
    public required string InstanceId { get; init; }
    public required string Version { get; init; }
    public required string ProtocolVersion { get; init; }
    public List<PeerDomainInfo> Domains { get; init; } = [];
    public List<string> Capabilities { get; init; } = [];
    public DateTimeOffset RegisteredAt { get; init; }
}

public record DomainStats
{
    public required string Domain { get; init; }
    public int FactCount { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
