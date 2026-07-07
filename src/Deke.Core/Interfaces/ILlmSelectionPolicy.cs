using Deke.Core.Models;

namespace Deke.Core.Interfaces;

/// <summary>
/// Result of model routing: which keyed <c>IChatClient</c> to use and the model id to request.
/// </summary>
public record LlmSelection(string ClientKey, string ModelId);

/// <summary>
/// Chooses the LLM backend for an advisory call. Honors the zero-cost priority:
/// prefer the local backend when knowledge depth permits, escalate only when warranted.
/// </summary>
public interface ILlmSelectionPolicy
{
    LlmSelection Select(
        double knowledgeDepth,
        ConfidenceBand band,
        Stakes stakes,
        bool allowLocalModel,
        string? modelOverride);
}
