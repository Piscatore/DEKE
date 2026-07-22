using Deke.Core.Models;
using Deke.Infrastructure.Trust;

namespace Deke.Tests;

public class TrustEvaluatorTests
{
    private readonly TrustEvaluator _evaluator = new();

    private static Fact MakeFact(
        float confidence = 1.0f,
        int corroborationCount = 0,
        DateTimeOffset? validFrom = null) => new()
        {
            Content = "test content",
            Domain = "fishing",
            Confidence = confidence,
            CorroborationCount = corroborationCount,
            ValidFrom = validFrom
        };

    private static DomainTrustPolicy MakePolicy(
        bool requirePrimarySource = false,
        int minCorroboration = 0,
        List<SourceTier>? flagForReviewTiers = null,
        bool temporalValidityRequired = false,
        float minConfidenceScore = 0f) => new()
        {
            Domain = "fishing",
            RequirePrimarySource = requirePrimarySource,
            MinCorroboration = minCorroboration,
            FlagForReviewTiers = flagForReviewTiers ?? [],
            TemporalValidityRequired = temporalValidityRequired,
            MinConfidenceScore = minConfidenceScore
        };

    [Fact]
    public void Evaluate_NoPolicy_ReturnsAccepted()
    {
        var result = _evaluator.Evaluate(MakeFact(), sourceTier: null, policy: null);

        Assert.Equal(TrustState.Accepted, result);
    }

    [Fact]
    public void Evaluate_RequiresPrimarySource_NonPrimaryTier_ReturnsFlagged()
    {
        var policy = MakePolicy(requirePrimarySource: true);

        var result = _evaluator.Evaluate(MakeFact(), SourceTier.Secondary, policy);

        Assert.Equal(TrustState.Flagged, result);
    }

    [Fact]
    public void Evaluate_RequiresPrimarySource_PrimaryTier_ReturnsAccepted()
    {
        var policy = MakePolicy(requirePrimarySource: true);

        var result = _evaluator.Evaluate(MakeFact(), SourceTier.Primary, policy);

        Assert.Equal(TrustState.Accepted, result);
    }

    [Fact]
    public void Evaluate_TemporalValidityRequired_MissingValidFrom_ReturnsFlagged()
    {
        var policy = MakePolicy(temporalValidityRequired: true);

        var result = _evaluator.Evaluate(MakeFact(validFrom: null), sourceTier: null, policy);

        Assert.Equal(TrustState.Flagged, result);
    }

    [Fact]
    public void Evaluate_TemporalValidityRequired_HasValidFrom_ReturnsAccepted()
    {
        var policy = MakePolicy(temporalValidityRequired: true);

        var result = _evaluator.Evaluate(MakeFact(validFrom: DateTimeOffset.UtcNow), sourceTier: null, policy);

        Assert.Equal(TrustState.Accepted, result);
    }

    [Fact]
    public void Evaluate_ConfidenceBelowMinimum_ReturnsFlagged()
    {
        var policy = MakePolicy(minConfidenceScore: 0.8f);

        var result = _evaluator.Evaluate(MakeFact(confidence: 0.5f), sourceTier: null, policy);

        Assert.Equal(TrustState.Flagged, result);
    }

    [Fact]
    public void Evaluate_ConfidenceAtOrAboveMinimum_ReturnsAccepted()
    {
        var policy = MakePolicy(minConfidenceScore: 0.8f);

        var result = _evaluator.Evaluate(MakeFact(confidence: 0.8f), sourceTier: null, policy);

        Assert.Equal(TrustState.Accepted, result);
    }

    [Fact]
    public void Evaluate_BelowMinCorroboration_ReturnsFlagged()
    {
        var policy = MakePolicy(minCorroboration: 2);

        var result = _evaluator.Evaluate(MakeFact(corroborationCount: 1), sourceTier: null, policy);

        Assert.Equal(TrustState.Flagged, result);
    }

    [Fact]
    public void Evaluate_MeetsMinCorroboration_ReturnsAccepted()
    {
        var policy = MakePolicy(minCorroboration: 2);

        var result = _evaluator.Evaluate(MakeFact(corroborationCount: 2), sourceTier: null, policy);

        Assert.Equal(TrustState.Accepted, result);
    }

    [Fact]
    public void Evaluate_SourceTierInFlagForReviewList_ReturnsFlagged()
    {
        var policy = MakePolicy(flagForReviewTiers: [SourceTier.Unverified]);

        var result = _evaluator.Evaluate(MakeFact(), SourceTier.Unverified, policy);

        Assert.Equal(TrustState.Flagged, result);
    }

    [Fact]
    public void Evaluate_SourceTierNotListed_AllGatesPassed_ReturnsAccepted()
    {
        var policy = MakePolicy(flagForReviewTiers: [SourceTier.Unverified]);

        var result = _evaluator.Evaluate(MakeFact(), SourceTier.Aggregated, policy);

        Assert.Equal(TrustState.Accepted, result);
    }
}
