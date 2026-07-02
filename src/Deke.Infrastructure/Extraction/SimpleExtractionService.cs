using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Extraction;

public class SimpleExtractionService : IExtractionService
{
    public Task<List<ExtractedFact>> ExtractFactsAsync(
        string content,
        string domain,
        string? sourceContext = null,
        CancellationToken ct = default)
    {
        var trimmed = content.Trim();

        if (trimmed.Length == 0)
            return Task.FromResult(new List<ExtractedFact>());

        return Task.FromResult(new List<ExtractedFact>
        {
            new() { Content = trimmed, Confidence = 0.8f }
        });
    }
}
