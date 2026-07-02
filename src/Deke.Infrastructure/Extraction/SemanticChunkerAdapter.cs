using Deke.Core.Interfaces;
using Microsoft.Extensions.AI;
using SemanticChunkerNET;

namespace Deke.Infrastructure.Extraction;

public class SemanticChunkerAdapter : IChunker
{
    private const int TokenLimit = 384;
    private readonly SemanticChunker _chunker;

    public SemanticChunkerAdapter(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _chunker = new SemanticChunker(embeddingGenerator, TokenLimit);
    }

    public async Task<IReadOnlyList<string>> ChunkAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var chunks = await _chunker.CreateChunksAsync(text, ct);
        return chunks.Select(c => c.Text).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
    }
}
