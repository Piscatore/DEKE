namespace Deke.Core.Models;

public record ExtractedFact
{
    public required string Content { get; init; }
    public float Confidence { get; init; } = 0.8f;
    public List<ExtractedEntity> Entities { get; init; } = [];
}
