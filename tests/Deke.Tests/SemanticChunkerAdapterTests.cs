using Deke.Infrastructure.Extraction;
using Microsoft.Extensions.AI;

namespace Deke.Tests;

public class SemanticChunkerAdapterTests
{
    [Fact]
    public async Task ChunkAsync_EmptyInput_ReturnsEmpty()
    {
        var adapter = new SemanticChunkerAdapter(new TopicEmbeddingGenerator());

        var chunks = await adapter.ChunkAsync(string.Empty);

        Assert.Empty(chunks);
    }

    [Fact]
    public async Task ChunkAsync_WhitespaceInput_ReturnsEmpty()
    {
        var adapter = new SemanticChunkerAdapter(new TopicEmbeddingGenerator());

        var chunks = await adapter.ChunkAsync("   \n\t  ");

        Assert.Empty(chunks);
    }

    [Fact]
    public async Task ChunkAsync_SingleSentence_ReturnsOneChunk()
    {
        var adapter = new SemanticChunkerAdapter(new TopicEmbeddingGenerator());

        var chunks = await adapter.ChunkAsync("Alpha topic sentence about alpha subject matter.");

        Assert.Single(chunks);
    }

    [Fact]
    public async Task ChunkAsync_MultiTopicText_YieldsMultipleCoherentChunks()
    {
        var adapter = new SemanticChunkerAdapter(new TopicEmbeddingGenerator());

        var alphaParagraph = string.Join(" ", Enumerable.Repeat(
            "Alpha topic sentence about alpha subject matter alpha.", 5));
        var betaParagraph = string.Join(" ", Enumerable.Repeat(
            "Beta topic sentence about beta subject matter beta.", 5));

        var chunks = await adapter.ChunkAsync($"{alphaParagraph} {betaParagraph}");

        Assert.True(chunks.Count > 1, $"Expected multiple chunks, got {chunks.Count}");
        Assert.All(chunks, c => Assert.False(string.IsNullOrWhiteSpace(c)));
    }

    // Produces embeddings that cluster by which topic keyword dominates the text, so a
    // real semantic breakpoint reliably fires at the topic transition without depending
    // on a live embedding model.
    private sealed class TopicEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var results = new GeneratedEmbeddings<Embedding<float>>();

            foreach (var value in values)
            {
                var alpha = CountOccurrences(value, "alpha");
                var beta = CountOccurrences(value, "beta");
                results.Add(new Embedding<float>(new float[] { alpha, beta }));
            }

            return Task.FromResult(results);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }

        private static int CountOccurrences(string text, string token)
        {
            var lower = text.ToLowerInvariant();
            var count = 0;
            var index = 0;

            while ((index = lower.IndexOf(token, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += token.Length;
            }

            return count;
        }
    }
}
