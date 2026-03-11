namespace Deke.Core.Interfaces;

public interface IEmbeddingService
{
    float[] GenerateEmbedding(string text);
    float[][] GenerateEmbeddings(IEnumerable<string> texts);
    float CosineSimilarity(float[] a, float[] b);
}
