namespace Deke.Core.Models;

public class FederationPeer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string InstanceId { get; set; }
    public required string BaseUrl { get; set; }
    public List<PeerDomainInfo> Domains { get; set; } = [];
    public List<string> Capabilities { get; set; } = [];
    public string ProtocolVersion { get; set; } = "1";
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsHealthy { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public record PeerDomainInfo
{
    public required string Name { get; init; }
    public int FactCount { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
    public float Confidence { get; init; }
}
