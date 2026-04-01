namespace Deke.Core.Models;

public class FederationConfig
{
    public bool Enabled { get; set; }
    public string InstanceId { get; set; } = "default";
    public int MaxHops { get; set; } = 3;
    public int TimeoutMs { get; set; } = 5000;
    public float DelegationThreshold { get; set; } = 0.4f;
    public Dictionary<string, float> LocalityWeights { get; set; } = new()
    {
        ["Local"] = 1.0f,
        ["Hop1"] = 0.9f,
        ["Hop2"] = 0.75f,
        ["Hop3"] = 0.6f
    };
    public List<PeerConfigEntry> Peers { get; set; } = [];

    public float GetLocalityWeight(int hops) =>
        hops == 0
            ? LocalityWeights.GetValueOrDefault("Local", 1.0f)
            : LocalityWeights.GetValueOrDefault($"Hop{hops}", 0.5f);
}

public record PeerConfigEntry
{
    public string InstanceId { get; init; } = "";
    public string BaseUrl { get; init; } = "";
}
