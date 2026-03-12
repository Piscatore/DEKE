using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Extraction;

public class SimpleExtractionService : IExtractionService
{
    private static readonly string[] SentenceSeparators = [". ", "! ", "? ", "\n"];

    public Task<List<ExtractedFact>> ExtractFactsAsync(
        string content,
        string domain,
        string? sourceContext = null,
        CancellationToken ct = default)
    {
        var sentences = content.Split(SentenceSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var facts = sentences
            .Where(s => s.Length >= 20 && s.Length <= 500)
            .Select(s => new ExtractedFact
            {
                Content = s,
                Confidence = 0.7f
            })
            .ToList();

        return Task.FromResult(facts);
    }
}
