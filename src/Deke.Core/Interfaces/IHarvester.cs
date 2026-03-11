using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IHarvester
{
    SourceType SupportedType { get; }
    Task<HarvestResult> HarvestAsync(Source source, CancellationToken ct = default);
}

public class HarvestResult
{
    public bool HasChanges { get; set; }
    public string? NewContentHash { get; set; }
    public List<string> ExtractedTexts { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
