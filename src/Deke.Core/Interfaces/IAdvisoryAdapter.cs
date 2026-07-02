using Deke.Core.Models;

namespace Deke.Core.Interfaces;

/// <summary>
/// Layer 3 domain plugin. Supplies domain-specific system prompt, fact
/// weighting, context formatting, and trust calibration. The shared-core
/// pipeline enforces the honesty constraint; adapters cannot raise confidence.
/// </summary>
public interface IAdvisoryAdapter
{
    string SystemPrompt();

    IReadOnlyList<FactSearchResult> WeightFacts(IReadOnlyList<FactSearchResult> facts, string query);

    string FormatContext(IReadOnlyList<FactSearchResult> facts);

    /// <summary>Translates a composite trust score into natural-language guidance for the model.</summary>
    string CalibrateTrust(double trustScore);

    DomainActivationCriteria ActivationCriteria { get; }
}
