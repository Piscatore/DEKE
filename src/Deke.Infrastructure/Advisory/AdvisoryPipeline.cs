using System.Diagnostics;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deke.Infrastructure.Advisory;

/// <summary>
/// Layer 2 shared core. Runs the 7-stage advisory pipeline: validate, retrieve,
/// assemble context, construct prompt, call model, assemble response, log.
/// The honesty constraint is enforced here — the confidence band is computed from
/// retrieval quality and never raised by an adapter.
/// </summary>
public class AdvisoryPipeline : IAdvisoryPipeline
{
    private readonly IEmbeddingService _embeddings;
    private readonly IFactRepository _facts;
    private readonly ITrustScoringService _trust;
    private readonly IReadOnlyList<IAdvisoryAdapter> _adapters;
    private readonly ILlmSelectionPolicy _policy;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAdvisoryInteractionRepository _interactions;
    private readonly AdvisoryConfig _config;
    private readonly ILogger<AdvisoryPipeline> _logger;

    public AdvisoryPipeline(
        IEmbeddingService embeddings,
        IFactRepository facts,
        ITrustScoringService trust,
        IEnumerable<IAdvisoryAdapter> adapters,
        ILlmSelectionPolicy policy,
        IServiceProvider serviceProvider,
        IAdvisoryInteractionRepository interactions,
        IOptions<AdvisoryConfig> config,
        ILogger<AdvisoryPipeline> logger)
    {
        _embeddings = embeddings;
        _facts = facts;
        _trust = trust;
        _adapters = adapters.ToList();
        _policy = policy;
        _serviceProvider = serviceProvider;
        _interactions = interactions;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<AdvisoryResponse> AdviseAsync(AdvisoryRequest request, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var now = DateTimeOffset.UtcNow;
        var stakes = request.Hints?.Stakes ?? Stakes.Medium;
        var adapter = ResolveAdapter(request.Domain);

        // Stage 1 — request validation.
        if (string.IsNullOrWhiteSpace(request.Domain))
        {
            return await InsufficientAsync(request, stakes, ["No domain was specified."], stopwatch, ct);
        }

        // Stage 2 — fact retrieval (local-only, trust-filtered).
        var embedding = _embeddings.GenerateEmbedding(request.Query);
        var results = await _facts.SearchAsync(embedding, request.Domain, _config.RetrievalLimit, _config.MinSimilarity, ct);

        if (results.Count == 0)
        {
            return await InsufficientAsync(
                request, stakes,
                [$"The '{request.Domain}' knowledge base has no facts relevant to this query."],
                stopwatch, ct);
        }

        // Stage 3 — context assembly.
        var weighted = adapter.WeightFacts(results, request.Query);
        var depth = KnowledgeDepth.Compute(weighted, _trust, _config.RetrievalLimit, now);
        var band = KnowledgeDepth.Band(depth); // honesty: band derives from retrieval, not the adapter
        var trustGuidance = adapter.CalibrateTrust(depth);
        var context = adapter.FormatContext(weighted);

        // Stage 4 — prompt construction. PriorExchanges are accepted but intentionally unused in the MVP.
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, adapter.SystemPrompt()),
            new(ChatRole.User, BuildUserPrompt(trustGuidance, context, request.Query))
        };

        // Stage 5 — model call.
        var selection = _policy.Select(depth, band, stakes, adapter.ActivationCriteria.AllowLocalModel, request.Hints?.ModelOverride);
        var chatClient = _serviceProvider.GetRequiredKeyedService<IChatClient>(selection.ClientKey);
        var chatOptions = new ChatOptions { ModelId = selection.ModelId, MaxOutputTokens = _config.MaxOutputTokens };
        var chatResponse = await chatClient.GetResponseAsync(messages, chatOptions, ct);
        var content = chatResponse.Text ?? string.Empty;
        var modelUsed = string.IsNullOrEmpty(chatResponse.ModelId) ? selection.ModelId : chatResponse.ModelId;

        // Stage 6 — response assembly.
        var citedFactIds = weighted.Select(f => f.Id).ToArray();
        var knowledgeGaps = band <= ConfidenceBand.Low
            ? new[] { "Retrieved facts provide only partial coverage of this query." }
            : [];

        // Stage 7 — audit logging.
        var interaction = new AdvisoryInteraction
        {
            Domain = request.Domain,
            Query = request.Query,
            Stakes = stakes,
            Model = modelUsed,
            CitedFactIds = [.. citedFactIds],
            FactConfidences = weighted.Select(f => f.Confidence).ToList(),
            ConfidenceBand = band,
            KnowledgeGaps = [.. knowledgeGaps],
            RawOutput = content,
            ContainsConflicting = false
        };
        var interactionId = await _interactions.AddAsync(interaction, ct);

        stopwatch.Stop();
        _logger.LogDebug(
            "Advised on '{Query}' in {Domain}: band {Band}, model {Model}, {Count} facts",
            request.Query, request.Domain, band, modelUsed, results.Count);

        return new AdvisoryResponse
        {
            Content = content,
            InteractionId = interactionId.ToString(),
            Confidence = band,
            CitedFactIds = citedFactIds,
            KnowledgeGaps = knowledgeGaps,
            ContainsConflictingEvidence = false,
            Metadata = new AdvisoryMetadata
            {
                Model = modelUsed,
                ModelKey = selection.ClientKey,
                KnowledgeDepth = depth,
                FactsRetrieved = results.Count,
                DurationMs = stopwatch.ElapsedMilliseconds
            }
        };
    }

    private IAdvisoryAdapter ResolveAdapter(string domain)
    {
        var match = _adapters.FirstOrDefault(a =>
            string.Equals(a.ActivationCriteria.Domain, domain, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
            return match;

        var wildcard = _adapters.FirstOrDefault(a => a.ActivationCriteria.Domain == "*");
        return wildcard ?? _adapters[0];
    }

    private static string BuildUserPrompt(string trustGuidance, string context, string query)
    {
        var sb = new StringBuilder();
        sb.AppendLine(trustGuidance);
        sb.AppendLine();
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine($"Question: {query}");
        return sb.ToString().TrimEnd();
    }

    private async Task<AdvisoryResponse> InsufficientAsync(
        AdvisoryRequest request, Stakes stakes, string[] gaps, Stopwatch stopwatch, CancellationToken ct)
    {
        var interaction = new AdvisoryInteraction
        {
            Domain = request.Domain,
            Query = request.Query,
            Stakes = stakes,
            Model = "none",
            ConfidenceBand = ConfidenceBand.Insufficient,
            KnowledgeGaps = [.. gaps],
            RawOutput = null,
            ContainsConflicting = false
        };
        var interactionId = await _interactions.AddAsync(interaction, ct);

        stopwatch.Stop();
        return new AdvisoryResponse
        {
            Content = "The knowledge base does not contain enough information to answer this question.",
            InteractionId = interactionId.ToString(),
            Confidence = ConfidenceBand.Insufficient,
            CitedFactIds = [],
            KnowledgeGaps = gaps,
            ContainsConflictingEvidence = false,
            Metadata = new AdvisoryMetadata
            {
                Model = "none",
                ModelKey = "none",
                KnowledgeDepth = 0.0,
                FactsRetrieved = 0,
                DurationMs = stopwatch.ElapsedMilliseconds
            }
        };
    }
}
