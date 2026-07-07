using Deke.Core.Models;
using Deke.Infrastructure.Advisory;
using Deke.Infrastructure.Trust;

namespace Deke.Tests;

public class KnowledgeDepthTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static FactSearchResult Fact(float similarity, float confidence, float credibility) => new()
    {
        Id = Guid.NewGuid(),
        Content = "fact",
        Domain = "software-product",
        Similarity = similarity,
        Confidence = confidence,
        SourceCredibility = credibility,
        CreatedAt = Now.AddDays(-1)
    };

    [Fact]
    public void Compute_EmptyFacts_ReturnsZero()
    {
        var depth = KnowledgeDepth.Compute([], new TrustScoringService(), limit: 10, Now);

        Assert.Equal(0.0, depth);
    }

    [Fact]
    public void Compute_ReturnsScoreInUnitInterval()
    {
        var facts = new List<FactSearchResult>
        {
            Fact(0.9f, 0.9f, 0.9f),
            Fact(0.8f, 0.8f, 0.8f)
        };

        var depth = KnowledgeDepth.Compute(facts, new TrustScoringService(), limit: 10, Now);

        Assert.InRange(depth, 0.0, 1.0);
        Assert.True(depth > 0.0);
    }

    [Fact]
    public void Compute_MoreCoverage_RaisesScore()
    {
        var trust = new TrustScoringService();
        var few = new List<FactSearchResult> { Fact(0.9f, 0.9f, 0.9f) };
        var many = Enumerable.Range(0, 10).Select(_ => Fact(0.9f, 0.9f, 0.9f)).ToList();

        var depthFew = KnowledgeDepth.Compute(few, trust, limit: 10, Now);
        var depthMany = KnowledgeDepth.Compute(many, trust, limit: 10, Now);

        Assert.True(depthMany > depthFew);
    }

    [Theory]
    [InlineData(0.75, ConfidenceBand.High)]
    [InlineData(0.90, ConfidenceBand.High)]
    [InlineData(0.74, ConfidenceBand.Medium)]
    [InlineData(0.50, ConfidenceBand.Medium)]
    [InlineData(0.49, ConfidenceBand.Low)]
    [InlineData(0.25, ConfidenceBand.Low)]
    [InlineData(0.24, ConfidenceBand.Insufficient)]
    [InlineData(0.0, ConfidenceBand.Insufficient)]
    public void Band_MapsScoreToBand(double score, ConfidenceBand expected)
    {
        Assert.Equal(expected, KnowledgeDepth.Band(score));
    }
}
