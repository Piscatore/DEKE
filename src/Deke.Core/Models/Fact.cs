using System.Text.Json;

namespace Deke.Core.Models;

public class Fact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Content { get; set; }
    public required string Domain { get; set; }
    public float[]? Embedding { get; set; }
    public float Confidence { get; set; } = 1.0f;
    public Guid? SourceId { get; set; }
    public List<Guid> RelatedFactIds { get; set; } = [];
    public List<ExtractedEntity> Entities { get; set; } = [];
    public Dictionary<string, JsonElement> Metadata { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsOutdated { get; set; }
    public string? OutdatedReason { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public int CorroborationCount { get; set; }
    public DateTimeOffset? LastVerifiedAt { get; set; }
    public bool ContradictionFlag { get; set; }
    public TrustState TrustState { get; set; } = TrustState.Unscored;

    // Deduplication (R2): levels 2-3 exact hashes, level 4 similarity fingerprint,
    // and a pointer to the canonical fact when this one is found to be a duplicate.
    public string? ContentHash { get; set; }
    public string? NormalizedHash { get; set; }
    public long? SimilarityHash { get; set; }
    public Guid? DuplicateOf { get; set; }

    // Navigation
    public Source? Source { get; set; }
}

public record ExtractedEntity
{
    public required string Type { get; init; }
    public required string Value { get; init; }
}

public enum TrustState
{
    Unscored,
    Accepted,
    Flagged,
    Contested,
    Rejected
}
