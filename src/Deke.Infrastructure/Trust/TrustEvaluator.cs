using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Trust;

public class TrustEvaluator : ITrustEvaluator
{
    public TrustState Evaluate(Fact fact, SourceTier? sourceTier, DomainTrustPolicy? policy)
    {
        if (policy is null)
            return TrustState.Accepted;

        if (policy.RequirePrimarySource && sourceTier != SourceTier.Primary)
            return TrustState.Flagged;

        if (policy.TemporalValidityRequired && fact.ValidFrom is null)
            return TrustState.Flagged;

        if (fact.Confidence < policy.MinConfidenceScore)
            return TrustState.Flagged;

        if (policy.MinCorroboration > 0 && fact.CorroborationCount < policy.MinCorroboration)
            return TrustState.Flagged;

        if (sourceTier is not null && policy.FlagForReviewTiers.Contains(sourceTier.Value))
            return TrustState.Flagged;

        return TrustState.Accepted;
    }
}
