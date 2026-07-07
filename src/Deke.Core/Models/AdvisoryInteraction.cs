namespace Deke.Core.Models;

/// <summary>
/// Append-only audit record of a single advisory interaction. Persisted for
/// accountability and future evolution analysis; never modified after write.
/// </summary>
public class AdvisoryInteraction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Domain { get; set; }
    public required string Query { get; set; }
    public Stakes Stakes { get; set; }
    public required string Model { get; set; }
    public List<Guid> CitedFactIds { get; set; } = [];
    public List<float> FactConfidences { get; set; } = [];
    public ConfidenceBand ConfidenceBand { get; set; }
    public List<string> KnowledgeGaps { get; set; } = [];
    public string? RawOutput { get; set; }
    public bool ContainsConflicting { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
