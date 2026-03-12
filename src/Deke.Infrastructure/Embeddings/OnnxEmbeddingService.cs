using Deke.Core.Interfaces;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Deke.Infrastructure.Embeddings;

public class OnnxEmbeddingService : IEmbeddingService, IDisposable
{
    private readonly InferenceSession _session;
    private readonly string[] _vocabulary;
    private readonly Dictionary<string, int> _vocabIndex;
    private const int MaxSequenceLength = 256;

    public OnnxEmbeddingService(EmbeddingsConfig config)
    {
        _session = new InferenceSession(config.ModelPath);
        _vocabulary = File.ReadAllLines(config.VocabPath);
        _vocabIndex = _vocabulary
            .Select((word, index) => (word, index))
            .ToDictionary(x => x.word, x => x.index);
    }

    public float[] GenerateEmbedding(string text)
    {
        var encoded = Tokenize(text);

        var inputIds = new DenseTensor<long>(
            encoded.InputIds.Select(x => (long)x).ToArray(),
            [1, encoded.InputIds.Length]);

        var attentionMask = new DenseTensor<long>(
            encoded.AttentionMask.Select(x => (long)x).ToArray(),
            [1, encoded.AttentionMask.Length]);

        var tokenTypeIds = new DenseTensor<long>(
            new long[encoded.InputIds.Length],
            [1, encoded.InputIds.Length]);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
        };

        using var results = _session.Run(inputs);

        var output = results.First(r => r.Name == "last_hidden_state").AsTensor<float>();

        return MeanPool(output, encoded.AttentionMask);
    }

    public float[][] GenerateEmbeddings(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var result = new List<float[]>();
        foreach (var text in texts)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(GenerateEmbedding(text));
        }
        return result.ToArray();
    }

    public float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have same length");

        float dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }

    public void Dispose()
    {
        _session?.Dispose();
    }

    private TokenizedInput Tokenize(string text)
    {
        var tokens = new List<int> { GetTokenId("[CLS]") };

        var words = text.ToLowerInvariant()
            .Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var wordTokens = TokenizeWord(word);

            if (tokens.Count + wordTokens.Count >= MaxSequenceLength - 1)
                break;

            tokens.AddRange(wordTokens);
        }

        tokens.Add(GetTokenId("[SEP]"));

        var attentionMask = Enumerable.Repeat(1, tokens.Count).ToArray();

        while (tokens.Count < MaxSequenceLength)
        {
            tokens.Add(0); // [PAD]
        }

        var mask = new int[MaxSequenceLength];
        Array.Copy(attentionMask, mask, attentionMask.Length);

        return new TokenizedInput
        {
            InputIds = tokens.Take(MaxSequenceLength).ToArray(),
            AttentionMask = mask
        };
    }

    private List<int> TokenizeWord(string word)
    {
        var tokens = new List<int>();

        if (_vocabIndex.TryGetValue(word, out var id))
        {
            tokens.Add(id);
            return tokens;
        }

        var remaining = word;
        var isFirst = true;

        while (remaining.Length > 0)
        {
            var found = false;

            for (int end = remaining.Length; end > 0; end--)
            {
                var subword = remaining[..end];
                var lookup = isFirst ? subword : "##" + subword;

                if (_vocabIndex.TryGetValue(lookup, out var subId))
                {
                    tokens.Add(subId);
                    remaining = remaining[end..];
                    isFirst = false;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                tokens.Add(GetTokenId("[UNK]"));
                break;
            }
        }

        return tokens;
    }

    private int GetTokenId(string token)
    {
        return _vocabIndex.TryGetValue(token, out var id) ? id : _vocabIndex["[UNK]"];
    }

    private static float[] MeanPool(Tensor<float> embeddings, int[] attentionMask)
    {
        var seqLen = embeddings.Dimensions[1];
        var hiddenSize = embeddings.Dimensions[2];
        var result = new float[hiddenSize];

        var validTokens = attentionMask.Sum();

        for (int i = 0; i < seqLen; i++)
        {
            if (attentionMask[i] == 1)
            {
                for (int j = 0; j < hiddenSize; j++)
                {
                    result[j] += embeddings[0, i, j];
                }
            }
        }

        for (int j = 0; j < hiddenSize; j++)
        {
            result[j] /= validTokens;
        }

        // L2 normalize
        var norm = MathF.Sqrt(result.Sum(x => x * x));
        for (int j = 0; j < hiddenSize; j++)
        {
            result[j] /= norm;
        }

        return result;
    }

    private class TokenizedInput
    {
        public required int[] InputIds { get; set; }
        public required int[] AttentionMask { get; set; }
    }
}
