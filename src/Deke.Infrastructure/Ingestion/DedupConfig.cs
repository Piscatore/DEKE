namespace Deke.Infrastructure.Ingestion;

/// <summary>
/// Tunables for the asynchronous deduplication jobs (levels 4-5). Bound from
/// the "Dedup" configuration section; the synchronous gateway (levels 1-3) does
/// not consume these.
/// </summary>
public class DedupConfig
{
    /// <summary>Interval between level-4 (similarity hash) job cycles.</summary>
    public int SimilarityIntervalMinutes { get; set; } = 30;

    /// <summary>Interval between level-5 (semantic) job cycles.</summary>
    public int SemanticIntervalMinutes { get; set; } = 60;

    /// <summary>Facts processed per job cycle.</summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>Max SimHash Hamming distance treated as a near-duplicate (level 4).</summary>
    public int HammingThreshold { get; set; } = 3;

    /// <summary>Min cosine similarity treated as a semantic duplicate (level 5).</summary>
    public float SemanticThreshold { get; set; } = 0.92f;
}
