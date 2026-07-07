using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Advisory;

/// <summary>
/// Computes the knowledge_depth_score for a retrieval result and maps retrieval
/// quality to a <see cref="ConfidenceBand"/>. Depth drives model routing; the band
/// drives the honesty-constrained response confidence.
/// </summary>
public static class KnowledgeDepth
{
    /// <summary>
    /// knowledge_depth_score = mean(topK trust) x coverage x topSimilarity, in [0, 1].
    /// Empty retrieval yields 0.
    /// </summary>
    public static double Compute(
        IReadOnlyList<FactSearchResult> facts,
        ITrustScoringService trust,
        int limit,
        DateTimeOffset now,
        int topK = 5)
    {
        if (facts.Count == 0)
            return 0.0;

        var topTrust = facts
            .Select(f => trust.Score(
                f.Similarity, f.Confidence, f.SourceCredibility,
                f.CreatedAt, f.ValidFrom, f.ValidUntil, localityWeight: 1.0, now))
            .OrderByDescending(s => s)
            .Take(topK)
            .ToList();

        var meanTrust = topTrust.Average();
        var coverage = Math.Min(1.0, facts.Count / (double)Math.Max(1, limit));
        double topSimilarity = facts.Max(f => f.Similarity);

        return meanTrust * coverage * topSimilarity;
    }

    /// <summary>Maps a retrieval score to a confidence band (thresholds 0.75 / 0.50 / 0.25).</summary>
    public static ConfidenceBand Band(double score) => score switch
    {
        >= 0.75 => ConfidenceBand.High,
        >= 0.50 => ConfidenceBand.Medium,
        >= 0.25 => ConfidenceBand.Low,
        _ => ConfidenceBand.Insufficient
    };
}
