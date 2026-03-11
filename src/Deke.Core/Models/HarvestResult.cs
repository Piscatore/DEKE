namespace Deke.Core.Models;

public record HarvestResult
{
    public bool HasChanges { get; init; }
    public string? NewContentHash { get; init; }
    public List<string> ExtractedTexts { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
