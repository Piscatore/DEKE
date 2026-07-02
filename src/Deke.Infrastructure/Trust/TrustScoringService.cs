using Deke.Core.Interfaces;

namespace Deke.Infrastructure.Trust;

public class TrustScoringService : ITrustScoringService
{
    private const float NeutralCredibility = 0.5f;
    private static readonly TimeSpan RecencyHalfLife = TimeSpan.FromDays(180);

    public double Score(
        float similarity,
        float confidence,
        float sourceCredibility,
        DateTimeOffset createdAt,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil,
        double localityWeight,
        DateTimeOffset now)
    {
        if (validFrom is { } from && now < from)
            return 0;

        if (validUntil is { } until && now > until)
            return 0;

        // Federated results arrive without local source credibility; treat as neutral rather than zeroing the score.
        var credibility = sourceCredibility > 0 ? sourceCredibility : NeutralCredibility;

        var age = now - createdAt;
        if (age < TimeSpan.Zero)
            age = TimeSpan.Zero;

        var recencyDecay = Math.Pow(0.5, age.TotalDays / RecencyHalfLife.TotalDays);

        return similarity * confidence * credibility * recencyDecay * localityWeight;
    }
}
