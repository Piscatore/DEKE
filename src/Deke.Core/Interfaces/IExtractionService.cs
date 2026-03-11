using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IExtractionService
{
    Task<List<ExtractedFact>> ExtractFactsAsync(
        string content,
        string domain,
        string? sourceContext = null,
        CancellationToken ct = default);
}

public class ExtractedFact
{
    public required string Content { get; set; }
    public float Confidence { get; set; } = 0.8f;
    public List<ExtractedEntity> Entities { get; set; } = [];
}
