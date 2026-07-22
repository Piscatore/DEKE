namespace Deke.Infrastructure.Ingestion;

/// <summary>
/// Tunables for the trust-state evaluation job. Bound from the
/// "TrustEvaluation" configuration section.
/// </summary>
public class TrustEvaluationConfig
{
    /// <summary>Interval between evaluation job cycles.</summary>
    public int IntervalMinutes { get; set; } = 20;

    /// <summary>Facts processed per job cycle.</summary>
    public int BatchSize { get; set; } = 200;
}
