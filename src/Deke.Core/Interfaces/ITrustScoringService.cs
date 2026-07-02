namespace Deke.Core.Interfaces;

public interface ITrustScoringService
{
    double Score(
        float similarity,
        float confidence,
        float sourceCredibility,
        DateTimeOffset createdAt,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil,
        double localityWeight,
        DateTimeOffset now);
}
