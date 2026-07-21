using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Deke.Core.Interfaces;

namespace Deke.Infrastructure.Ingestion;

/// <summary>
/// 64-bit SimHash over token shingles for near-duplicate detection (level 4).
/// Similar texts produce fingerprints with a small Hamming distance.
/// </summary>
public class SimHasher : ISimHasher
{
    private const int ShingleSize = 3;

    public long Compute(string content)
    {
        var tokens = Tokenize(content);
        if (tokens.Count == 0)
            return 0L;

        var weights = new int[64];
        foreach (var shingle in Shingles(tokens))
        {
            var hash = Hash64(shingle);
            for (var bit = 0; bit < 64; bit++)
            {
                if (((hash >> bit) & 1UL) == 1UL)
                    weights[bit]++;
                else
                    weights[bit]--;
            }
        }

        ulong fingerprint = 0;
        for (var bit = 0; bit < 64; bit++)
        {
            if (weights[bit] > 0)
                fingerprint |= 1UL << bit;
        }

        return unchecked((long)fingerprint);
    }

    public int HammingDistance(long a, long b) =>
        BitOperations.PopCount((ulong)(a ^ b));

    private static List<string> Tokenize(string content) =>
        [.. content.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)];

    private static IEnumerable<string> Shingles(List<string> tokens)
    {
        if (tokens.Count < ShingleSize)
        {
            yield return string.Join(' ', tokens);
            yield break;
        }

        for (var i = 0; i <= tokens.Count - ShingleSize; i++)
            yield return string.Join(' ', tokens.GetRange(i, ShingleSize));
    }

    private static ulong Hash64(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return BitConverter.ToUInt64(bytes, 0);
    }
}
