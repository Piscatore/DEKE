namespace Deke.Core.Models;

public class FactProvenance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid FactId { get; set; }
    public required Guid SourceId { get; set; }
    public DateTimeOffset ExtractedAt { get; set; } = DateTimeOffset.UtcNow;
    public ExtractionMethod ExtractionMethod { get; set; }
    public float ExtractionConfidence { get; set; } = 1.0f;
}

public enum ExtractionMethod
{
    RssHarvest,
    WebHarvest,
    ManualApi,
    LlmExtract
}
