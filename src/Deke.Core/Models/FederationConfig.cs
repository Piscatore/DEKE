namespace Deke.Core.Models;

public class FederationConfig
{
    public bool Enabled { get; set; }
    public string InstanceId { get; set; } = "default";
    public int MaxHops { get; set; } = 3;
    public int TimeoutMs { get; set; } = 5000;
    public float DelegationThreshold { get; set; } = 0.4f;
    public List<PeerConfigEntry> Peers { get; set; } = [];
}

public record PeerConfigEntry
{
    public string InstanceId { get; init; } = "";
    public string BaseUrl { get; init; } = "";
}
