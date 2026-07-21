namespace Deke.Core.Models;

public class FactVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid FactId { get; set; }
    public required string ContentSnapshot { get; set; }
    public float[]? EmbeddingSnapshot { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public ChangeReason ChangeReason { get; set; }
}

public enum ChangeReason
{
    SourceUpdate,
    ManualCorrection,
    Merge,
    ContradictionResolution
}
