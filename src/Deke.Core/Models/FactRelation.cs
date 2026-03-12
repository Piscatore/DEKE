namespace Deke.Core.Models;

public class FactRelation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid FromFactId { get; set; }
    public required Guid ToFactId { get; set; }
    public required string RelationType { get; set; }
    public float Confidence { get; set; } = 0.5f;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Fact? FromFact { get; set; }
    public Fact? ToFact { get; set; }
}
