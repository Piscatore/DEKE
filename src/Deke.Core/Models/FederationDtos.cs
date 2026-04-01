namespace Deke.Core.Models;

public record FederatedSearchRequest
{
    public required string Query { get; init; }
    public string? Domain { get; init; }
    public int Limit { get; init; } = 10;
    public float MinSimilarity { get; init; } = 0.5f;
}

public record FederatedContextRequest
{
    public required string Topic { get; init; }
    public string? Domain { get; init; }
    public int MaxTokens { get; init; } = 2000;
}

public record FederationContext
{
    public int HopCount { get; init; }
    public required string QueryOrigin { get; init; }
    public List<string> Visited { get; init; } = [];
    public Guid RequestId { get; init; } = Guid.NewGuid();
}

public record ResultProvenance
{
    public required string InstanceId { get; init; }
    public int Hops { get; init; }
    public DateTimeOffset RetrievedAt { get; init; } = DateTimeOffset.UtcNow;
}

public record FederationMetadata
{
    public Guid RequestId { get; init; }
    public List<string> PeersConsulted { get; init; } = [];
    public List<string> PeersSkipped { get; init; } = [];
    public int TotalHops { get; init; }
    public int LocalResultCount { get; init; }
    public int FederatedResultCount { get; init; }
}
