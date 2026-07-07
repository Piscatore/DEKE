namespace Deke.Core.Models;

/// <summary>
/// Diagnostic metadata about how an advisory response was produced.
/// </summary>
public record AdvisoryMetadata
{
    public string Model { get; init; } = string.Empty;
    public string ModelKey { get; init; } = string.Empty;
    public double KnowledgeDepth { get; init; }
    public int FactsRetrieved { get; init; }
    public long DurationMs { get; init; }
}

/// <summary>
/// Fixed advisory contract (Layer 1) — response side. Never changes; new fields are additive.
/// </summary>
public record AdvisoryResponse
{
    public required string Content { get; init; }
    public required string InteractionId { get; init; }
    public ConfidenceBand Confidence { get; init; }
    public Guid[] CitedFactIds { get; init; } = [];

    /// <summary>Mandatory disclosure of what the knowledge base did not cover.</summary>
    public string[] KnowledgeGaps { get; init; } = [];

    public bool ContainsConflictingEvidence { get; init; }
    public AdvisoryMetadata Metadata { get; init; } = new();
}
