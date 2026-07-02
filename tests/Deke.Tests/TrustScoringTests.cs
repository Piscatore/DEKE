using Deke.Infrastructure.Trust;

namespace Deke.Tests;

public class TrustScoringTests
{
    private readonly TrustScoringService _service = new();
    private static readonly DateTimeOffset Now = new(2026, 7, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Score_HigherSourceCredibility_ProducesHigherScore()
    {
        var low = _service.Score(0.8f, 0.8f, 0.2f, Now, null, null, 1.0, Now);
        var high = _service.Score(0.8f, 0.8f, 0.9f, Now, null, null, 1.0, Now);

        Assert.True(high > low);
    }

    [Fact]
    public void Score_HigherConfidence_ProducesHigherScore()
    {
        var low = _service.Score(0.8f, 0.3f, 0.5f, Now, null, null, 1.0, Now);
        var high = _service.Score(0.8f, 0.9f, 0.5f, Now, null, null, 1.0, Now);

        Assert.True(high > low);
    }

    [Fact]
    public void Score_HigherSimilarity_ProducesHigherScore()
    {
        var low = _service.Score(0.3f, 0.8f, 0.5f, Now, null, null, 1.0, Now);
        var high = _service.Score(0.9f, 0.8f, 0.5f, Now, null, null, 1.0, Now);

        Assert.True(high > low);
    }

    [Fact]
    public void Score_AfterValidUntil_ReturnsZero()
    {
        var validUntil = Now.AddDays(-1);
        var score = _service.Score(0.9f, 0.9f, 0.9f, Now.AddDays(-10), null, validUntil, 1.0, Now);

        Assert.Equal(0.0, score);
    }

    [Fact]
    public void Score_BeforeValidFrom_ReturnsZero()
    {
        var validFrom = Now.AddDays(1);
        var score = _service.Score(0.9f, 0.9f, 0.9f, Now, validFrom, null, 1.0, Now);

        Assert.Equal(0.0, score);
    }

    [Fact]
    public void Score_WithinValidityWindow_IsPositive()
    {
        var score = _service.Score(
            0.9f, 0.9f, 0.9f, Now, Now.AddDays(-1), Now.AddDays(1), 1.0, Now);

        Assert.True(score > 0);
    }

    [Fact]
    public void Score_OlderCreatedAt_ProducesLowerScoreViaRecencyDecay()
    {
        var recent = _service.Score(0.8f, 0.8f, 0.8f, Now.AddDays(-1), null, null, 1.0, Now);
        var old = _service.Score(0.8f, 0.8f, 0.8f, Now.AddDays(-400), null, null, 1.0, Now);

        Assert.True(recent > old);
        Assert.True(old > 0);
    }

    [Fact]
    public void Score_ZeroSourceCredibility_UsesNeutralFallback_NotZero()
    {
        // Federated results arrive without local source credibility (defaults to 0);
        // scoring must not zero them out entirely.
        var federated = _service.Score(0.8f, 0.8f, 0f, Now, null, null, 0.9, Now);

        Assert.True(federated > 0);
    }

    [Fact]
    public void Score_NegativeSourceCredibility_UsesNeutralFallback_NotZero()
    {
        var score = _service.Score(0.8f, 0.8f, -1f, Now, null, null, 0.9, Now);

        Assert.True(score > 0);
    }
}
