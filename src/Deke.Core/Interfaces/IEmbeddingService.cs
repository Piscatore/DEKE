namespace Deke.Core.Interfaces;

public interface IEmbeddingService
{
    float[] GenerateEmbedding(string text);
    float[][] GenerateEmbeddings(IEnumerable<string> texts, CancellationToken ct = default);
    float CosineSimilarity(float[] a, float[] b);
}
