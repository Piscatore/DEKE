namespace Deke.Infrastructure.Ingestion;

/// <summary>
/// Tunables for the contradiction-detection job. Bound from the
/// "ContradictionDetection" configuration section.
/// </summary>
public class ContradictionDetectionConfig
{
    /// <summary>Interval between contradiction-detection job cycles.</summary>
    public int IntervalMinutes { get; set; } = 45;

    /// <summary>Facts processed per job cycle.</summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>Lower bound of the candidate similarity band (below L5's dedup threshold).</summary>
    public float MinSimilarity { get; set; } = 0.75f;

    /// <summary>Upper bound of the candidate similarity band.</summary>
    public float MaxSimilarity { get; set; } = 0.90f;
}
