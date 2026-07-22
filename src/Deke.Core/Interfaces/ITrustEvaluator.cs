using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface ITrustEvaluator
{
    TrustState Evaluate(Fact fact, SourceTier? sourceTier, DomainTrustPolicy? policy);
}
