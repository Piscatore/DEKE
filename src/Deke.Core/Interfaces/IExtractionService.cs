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
