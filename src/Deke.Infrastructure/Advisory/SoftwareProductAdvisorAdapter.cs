using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Advisory;

/// <summary>
/// First domain adapter: DEKE self-advisory over its own bootstrapped design and
/// architecture knowledge (the "software-product" domain). Weights primary-source,
/// recent, high-credibility facts highest and keeps the advice version-aware.
/// </summary>
public class SoftwareProductAdvisorAdapter : IAdvisoryAdapter
{
    public const string DomainName = "software-product";

    public DomainActivationCriteria ActivationCriteria { get; } = new()
    {
        Domain = DomainName,
        MinFacts = 1,
        AllowLocalModel = true
    };

    public string SystemPrompt() =>
        "You are the Software Product Advisor for DEKE, a domain-expert knowledge engine. " +
        "Advise on DEKE's own product direction and software architecture decisions, grounded strictly in the " +
        "design and architecture facts provided as context. Prefer primary-source, recent facts; when facts describe " +
        "an evolving design, be version-aware and prefer the most recent decision while noting what it superseded. " +
        "Ground every recommendation in the cited facts and be explicit about gaps or open questions. " +
        "Never present a recommendation as more certain than the underlying facts justify.";

    public IReadOnlyList<FactSearchResult> WeightFacts(IReadOnlyList<FactSearchResult> facts, string query) =>
        facts
            .OrderByDescending(f => f.SourceCredibility)
            .ThenByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Similarity)
            .ToList();

    public string FormatContext(IReadOnlyList<FactSearchResult> facts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## DEKE Design & Architecture Context");
        foreach (var fact in facts)
        {
            sb.AppendLine(
                $"- [{fact.Id}] (credibility {fact.SourceCredibility:F2}, confidence {fact.Confidence:F2}, " +
                $"recorded {fact.CreatedAt:yyyy-MM-dd}) {fact.Content}");
        }
        return sb.ToString().TrimEnd();
    }

    public string CalibrateTrust(double trustScore) => trustScore switch
    {
        >= 0.75 => "This guidance rests on strong, recent, primary-source design knowledge; recommend with confidence.",
        >= 0.50 => "The design knowledge is adequate but partial; recommend carefully and surface the open questions.",
        >= 0.25 => "The design knowledge is thin or aged; flag low confidence and recommend confirming before acting.",
        _ => "DEKE's knowledge base does not adequately cover this decision; state that a grounded recommendation is not possible."
    };
}
