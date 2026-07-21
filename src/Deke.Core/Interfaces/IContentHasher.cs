namespace Deke.Core.Interfaces;

/// <summary>
/// Computes deterministic exact-match hashes for deduplication levels 2 and 3.
/// </summary>
public interface IContentHasher
{
    /// <summary>SHA-256 hex of the raw content (level 2, exact match).</summary>
    string ContentHash(string content);

    /// <summary>
    /// SHA-256 hex of the normalized content (level 3): whitespace collapsed,
    /// lowercased, punctuation stripped, Unicode NFKC-normalized.
    /// </summary>
    string NormalizedHash(string content);
}
