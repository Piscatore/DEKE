namespace Deke.Core.Models;

/// <summary>
/// Confidence of an advisory response, derived from retrieval quality.
/// Ordered ascending so the honesty constraint can compare/cap bands.
/// </summary>
public enum ConfidenceBand
{
    Insufficient = 0,
    Low = 1,
    Medium = 2,
    High = 3
}
