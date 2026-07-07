using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.Extensions.Options;

namespace Deke.Infrastructure.Advisory;

/// <summary>
/// Routes advisory calls between backends per the zero-cost priority: prefer the
/// local model when knowledge is deep enough, use cheap Anthropic (haiku) by default,
/// and escalate to the stronger model only when warranted.
/// </summary>
public class LlmSelectionPolicy : ILlmSelectionPolicy
{
    private readonly AdvisoryConfig _config;

    public LlmSelectionPolicy(IOptions<AdvisoryConfig> config) => _config = config.Value;

    public LlmSelection Select(
        double knowledgeDepth,
        ConfidenceBand band,
        Stakes stakes,
        bool allowLocalModel,
        string? modelOverride)
    {
        // Explicit caller override always wins (routed to Anthropic).
        if (!string.IsNullOrWhiteSpace(modelOverride))
            return new LlmSelection(AdvisoryClientKeys.Anthropic, modelOverride);

        // Zero-cost priority: local backend when the domain permits it and knowledge is deep.
        if (allowLocalModel && knowledgeDepth >= _config.OllamaDepthThreshold)
            return new LlmSelection(AdvisoryClientKeys.Ollama, _config.OllamaModel);

        // Escalate to the stronger model on low confidence + high stakes.
        if (band == ConfidenceBand.Low && stakes == Stakes.High)
            return new LlmSelection(AdvisoryClientKeys.Anthropic, _config.SonnetModel);

        // Knowledge compensation: cheap default when depth is adequate, stronger model when thin.
        return knowledgeDepth >= _config.HaikuDepthThreshold
            ? new LlmSelection(AdvisoryClientKeys.Anthropic, _config.HaikuModel)
            : new LlmSelection(AdvisoryClientKeys.Anthropic, _config.SonnetModel);
    }
}
