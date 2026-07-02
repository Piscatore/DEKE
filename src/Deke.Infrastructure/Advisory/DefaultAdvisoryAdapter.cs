using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Advisory;

/// <summary>
/// Fallback adapter for domains without a custom implementation. Provides a generic
/// grounded-advisor persona, identity fact weighting, markdown context, and
/// threshold-based trust guidance. Activates for any domain ("*").
/// </summary>
public class DefaultAdvisoryAdapter : IAdvisoryAdapter
{
    public DomainActivationCriteria ActivationCriteria { get; } = new() { Domain = "*", MinFacts = 1 };

    public string SystemPrompt() =>
        "You are a domain expert advisor. Answer the user's question using only the facts provided as context. " +
        "Ground every claim in those facts and cite them where relevant. If the context does not cover the question, " +
        "say so plainly rather than guessing. Do not overstate certainty beyond what the facts support.";

    public IReadOnlyList<FactSearchResult> WeightFacts(IReadOnlyList<FactSearchResult> facts, string query) => facts;

    public string FormatContext(IReadOnlyList<FactSearchResult> facts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Knowledge Context");
        foreach (var fact in facts)
        {
            sb.AppendLine($"- [{fact.Id}] (confidence {fact.Confidence:F2}, similarity {fact.Similarity:F2}) {fact.Content}");
        }
        return sb.ToString().TrimEnd();
    }

    public string CalibrateTrust(double trustScore) => trustScore switch
    {
        >= 0.75 => "The retrieved knowledge is strong and well-corroborated; you may answer with confidence.",
        >= 0.50 => "The retrieved knowledge is adequate but has gaps; answer carefully and note uncertainty where it exists.",
        >= 0.25 => "The retrieved knowledge is sparse or aged; be explicit about low confidence and missing coverage.",
        _ => "The knowledge base does not adequately cover this question; state that a grounded answer is not possible."
    };
}
