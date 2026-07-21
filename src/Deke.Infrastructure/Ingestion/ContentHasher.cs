using System.Security.Cryptography;
using System.Text;
using Deke.Core.Interfaces;

namespace Deke.Infrastructure.Ingestion;

/// <summary>
/// SHA-256 exact-match hashing for dedup levels 2 (raw) and 3 (normalized).
/// </summary>
public class ContentHasher : IContentHasher
{
    public string ContentHash(string content) => Sha256Hex(content);

    public string NormalizedHash(string content) => Sha256Hex(Normalize(content));

    /// <summary>
    /// NFKC-canonicalize, lowercase, drop punctuation/symbols, and collapse
    /// whitespace to a single space so trivially-different renderings of the
    /// same text hash identically.
    /// </summary>
    private static string Normalize(string content)
    {
        var canonical = content.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        var sb = new StringBuilder(canonical.Length);
        var lastWasSpace = false;

        foreach (var ch in canonical)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (sb.Length > 0 && !lastWasSpace)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
            }
            else if (!char.IsPunctuation(ch) && !char.IsSymbol(ch))
            {
                sb.Append(ch);
                lastWasSpace = false;
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string Sha256Hex(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
