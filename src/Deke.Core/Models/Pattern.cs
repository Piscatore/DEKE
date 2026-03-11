namespace Deke.Core.Models;

public class Pattern
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Description { get; set; }
    public required string Domain { get; set; }
    public PatternType PatternType { get; set; } = PatternType.Observation;
    public List<Guid> EvidenceFactIds { get; set; } = [];
    public float Confidence { get; set; }
    public int OccurrenceCount { get; set; } = 1;
    public DateTimeOffset DiscoveredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastValidatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum PatternType
{
    Observation,    // General observation about the domain
    Causal,         // X tends to cause Y
    Temporal,       // X happens before/after Y
    Structural      // Things of type X have property Y
}
