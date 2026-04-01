using System.Diagnostics;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deke.Infrastructure.Federation;

public class FederatedSearchService : IFederatedSearchService
{
    private readonly IFactRepository _factRepo;
    private readonly IFederationPeerRepository _peerRepo;
    private readonly IEmbeddingService _embeddings;
    private readonly FederationClient _federationClient;
    private readonly IOptions<FederationConfig> _config;
    private readonly ILogger<FederatedSearchService> _logger;

    public FederatedSearchService(
        IFactRepository factRepo,
        IFederationPeerRepository peerRepo,
        IEmbeddingService embeddings,
        FederationClient federationClient,
        IOptions<FederationConfig> config,
        ILogger<FederatedSearchService> logger)
    {
        _factRepo = factRepo;
        _peerRepo = peerRepo;
        _embeddings = embeddings;
        _federationClient = federationClient;
        _config = config;
        _logger = logger;
    }

    public async Task<SearchResponse> SearchAsync(
        FederatedSearchRequest request,
        FederationContext? federation = null,
        CancellationToken ct = default)
    {
        var config = _config.Value;
        var sw = Stopwatch.StartNew();

        var limit = Math.Clamp(request.Limit, 1, 500);
        var minSimilarity = Math.Clamp(request.MinSimilarity, 0f, 1f);

        // 1. Generate embedding and run local search
        var embedding = _embeddings.GenerateEmbedding(request.Query);
        var localResults = await _factRepo.SearchAsync(embedding, request.Domain, limit, minSimilarity, ct);

        // 2. Determine if federation is needed
        var federatedResults = new List<(FactSearchResult Result, string PeerInstanceId, int Hops)>();
        var peersConsulted = new List<string>();
        var peersSkipped = new List<string>();
        var hopCount = federation?.HopCount ?? 0;

        if (ShouldFederate(config, localResults, request.Domain, hopCount))
        {
            var peers = await GetCandidatePeers(config, request.Domain, federation, ct);

            foreach (var peer in peers.Skipped)
                peersSkipped.Add(peer);

            // 3. Query peers in parallel
            var tasks = peers.Candidates.Select(async peer =>
            {
                var peerContext = BuildPeerContext(config, federation);
                var response = await _federationClient.SearchPeerAsync(
                    peer.BaseUrl, request, peerContext, ct);
                return (peer.InstanceId, Response: response);
            });

            var peerResponses = await Task.WhenAll(tasks);

            foreach (var (instanceId, response) in peerResponses)
            {
                if (response?.Results is { Count: > 0 })
                {
                    peersConsulted.Add(instanceId);
                    foreach (var result in response.Results)
                    {
                        federatedResults.Add((result, instanceId, hopCount + 1));
                    }
                }
                else
                {
                    peersSkipped.Add(instanceId);
                }
            }
        }

        sw.Stop();

        // 4. Merge and score results
        var merged = MergeResults(config, localResults, federatedResults, limit);

        // 5. Build response
        var federationMetadata = peersConsulted.Count > 0 || peersSkipped.Count > 0
            ? new FederationMetadata
            {
                RequestId = federation?.RequestId ?? Guid.NewGuid(),
                PeersConsulted = peersConsulted,
                PeersSkipped = peersSkipped,
                TotalHops = hopCount + (peersConsulted.Count > 0 ? 1 : 0),
                LocalResultCount = localResults.Count,
                FederatedResultCount = federatedResults.Count
            }
            : null;

        return new SearchResponse
        {
            Query = request.Query,
            Domain = request.Domain,
            Results = merged,
            TotalCount = merged.Count,
            SearchDuration = sw.Elapsed,
            Federation = federationMetadata
        };
    }

    public async Task<ContextResponse> GetContextAsync(
        FederatedContextRequest request,
        FederationContext? federation = null,
        CancellationToken ct = default)
    {
        var maxTokens = Math.Clamp(request.MaxTokens, 100, 10000);

        // Delegate to SearchAsync for federation-aware fact retrieval
        var searchRequest = new FederatedSearchRequest
        {
            Query = request.Topic,
            Domain = request.Domain,
            Limit = 30,
            MinSimilarity = 0.4f
        };

        var searchResponse = await SearchAsync(searchRequest, federation, ct);

        // Format results as context
        var sb = new StringBuilder();
        sb.AppendLine($"## Domain Knowledge: {request.Domain ?? "All Domains"}");
        sb.AppendLine($"### Topic: {request.Topic}");
        sb.AppendLine();

        var tokenEstimate = 0;
        var factCount = 0;

        foreach (var fact in searchResponse.Results.OrderByDescending(f => f.Similarity))
        {
            var line = $"- {fact.Content}";
            var lineTokens = line.Length / 4;
            if (tokenEstimate + lineTokens > maxTokens) break;
            sb.AppendLine(line);
            tokenEstimate += lineTokens;
            factCount++;
        }

        return new ContextResponse
        {
            Topic = request.Topic,
            Domain = request.Domain,
            Context = sb.ToString(),
            FactCount = factCount,
            ApproximateTokens = tokenEstimate,
            Federation = searchResponse.Federation
        };
    }

    private bool ShouldFederate(
        FederationConfig config,
        List<FactSearchResult> localResults,
        string? domain,
        int hopCount)
    {
        if (!config.Enabled) return false;
        if (hopCount >= config.MaxHops) return false;

        // Always delegate when no domain specified (cross-domain search)
        if (domain is null) return true;

        // Delegate when no local results
        if (localResults.Count == 0) return true;

        // Delegate when best local result is below threshold
        var bestScore = localResults.Max(r => r.Similarity);
        return bestScore < config.DelegationThreshold;
    }

    private async Task<(List<FederationPeer> Candidates, List<string> Skipped)> GetCandidatePeers(
        FederationConfig config,
        string? domain,
        FederationContext? federation,
        CancellationToken ct)
    {
        var allPeers = await _peerRepo.GetHealthyAsync(ct);
        var candidates = new List<FederationPeer>();
        var skipped = new List<string>();
        var visited = federation?.Visited ?? [];

        foreach (var peer in allPeers)
        {
            // Skip self
            if (string.Equals(peer.InstanceId, config.InstanceId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Skip already visited
            if (visited.Contains(peer.InstanceId, StringComparer.OrdinalIgnoreCase))
            {
                skipped.Add(peer.InstanceId);
                continue;
            }

            // Filter by domain if specified
            if (domain is not null)
            {
                var hasDomain = peer.Domains.Any(d =>
                    string.Equals(d.Name, domain, StringComparison.OrdinalIgnoreCase));
                if (!hasDomain)
                {
                    skipped.Add(peer.InstanceId);
                    continue;
                }
            }

            candidates.Add(peer);
        }

        return (candidates, skipped);
    }

    private FederationContext BuildPeerContext(FederationConfig config, FederationContext? federation)
    {
        var visited = new List<string>(federation?.Visited ?? []) { config.InstanceId };

        return new FederationContext
        {
            HopCount = (federation?.HopCount ?? 0) + 1,
            QueryOrigin = federation?.QueryOrigin ?? config.InstanceId,
            Visited = visited,
            RequestId = federation?.RequestId ?? Guid.NewGuid()
        };
    }

    private List<FactSearchResult> MergeResults(
        FederationConfig config,
        List<FactSearchResult> localResults,
        List<(FactSearchResult Result, string PeerInstanceId, int Hops)> federatedResults,
        int limit)
    {
        var scored = new List<(FactSearchResult Result, float Score)>();

        // Score local results
        var localWeight = config.GetLocalityWeight(0);
        foreach (var result in localResults)
        {
            var score = result.Similarity * result.Confidence * localWeight;
            scored.Add((result, score));
        }

        // Score federated results with provenance
        foreach (var (result, peerInstanceId, hops) in federatedResults)
        {
            var weight = config.GetLocalityWeight(hops);
            var score = result.Similarity * result.Confidence * weight;
            var tagged = result with
            {
                Provenance = new ResultProvenance
                {
                    InstanceId = peerInstanceId,
                    Hops = hops,
                    RetrievedAt = DateTimeOffset.UtcNow
                }
            };
            scored.Add((tagged, score));
        }

        return scored
            .OrderByDescending(s => s.Score)
            .Take(limit)
            .Select(s => s.Result)
            .ToList();
    }
}
