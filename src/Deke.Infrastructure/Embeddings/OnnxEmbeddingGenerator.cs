using Deke.Core.Interfaces;
using Microsoft.Extensions.AI;

namespace Deke.Infrastructure.Embeddings;

public sealed class OnnxEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IEmbeddingService _embeddings;

    public OnnxEmbeddingGenerator(IEmbeddingService embeddings) => _embeddings = embeddings;

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var results = new GeneratedEmbeddings<Embedding<float>>();

        foreach (var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vector = _embeddings.GenerateEmbedding(value);
            results.Add(new Embedding<float>(vector));
        }

        return Task.FromResult(results);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
