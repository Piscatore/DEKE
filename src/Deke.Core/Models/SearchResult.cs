namespace Deke.Core.Models;

public record FactSearchResult
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public required string Domain { get; init; }
    public float Confidence { get; init; }
    public float Similarity { get; init; }
    public Guid? SourceId { get; init; }
    public string? SourceUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record SearchResponse
{
    public required string Query { get; init; }
    public required string Domain { get; init; }
    public List<FactSearchResult> Results { get; init; } = [];
    public int TotalCount { get; init; }
    public TimeSpan SearchDuration { get; init; }
}

public record ContextResponse
{
    public required string Topic { get; init; }
    public required string Domain { get; init; }
    public required string Context { get; init; }
    public int FactCount { get; init; }
    public int ApproximateTokens { get; init; }
}
