namespace Deke.Core.Interfaces;

/// <summary>
/// Computes a 64-bit SimHash fingerprint for near-duplicate detection (level 4).
/// </summary>
public interface ISimHasher
{
    /// <summary>64-bit SimHash over token shingles of the content.</summary>
    long Compute(string content);

    /// <summary>Bit-count difference between two fingerprints (0 = identical).</summary>
    int HammingDistance(long a, long b);
}
