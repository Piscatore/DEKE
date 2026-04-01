using System.ComponentModel;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using ModelContextProtocol.Server;

namespace Deke.Mcp.Tools;

[McpServerToolType]
public class SearchTools
{
    [McpServerTool(Name = "consult_domain_expert"), Description("Search for facts using semantic similarity, optionally delegating to federated DEKE peers")]
    public static async Task<string> ConsultDomainExpert(
        IFederatedSearchService searchService,
        [Description("The search query text")] string query,
        [Description("The knowledge domain to search in (optional — searches all domains if omitted)")] string? domain = null,
        [Description("Maximum number of results to return")] int limit = 10,
        [Description("Minimum similarity threshold (0.0 to 1.0)")] float minSimilarity = 0.5f,
        CancellationToken ct = default)
    {
        var request = new FederatedSearchRequest
        {
            Query = query,
            Domain = domain,
            Limit = limit,
            MinSimilarity = minSimilarity
        };

        var response = await searchService.SearchAsync(request, federation: null, ct);

        if (response.Results.Count == 0)
        {
            return $"No results found for '{query}'{(domain is not null ? $" in domain '{domain}'" : "")} with minimum similarity {minSimilarity:F2}.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {response.Results.Count} result(s) for '{query}'{(domain is not null ? $" in domain '{domain}'" : "")} ({response.SearchDuration.TotalMilliseconds:F0}ms):");
        sb.AppendLine();

        foreach (var result in response.Results)
        {
            sb.AppendLine($"- [{result.Similarity:F2}] {result.Content}");
            var source = result.SourceUrl ?? "N/A";
            if (result.Provenance is not null)
            {
                sb.AppendLine($"  ID: {result.Id} | Confidence: {result.Confidence:F2} | Source: {source} | From: {result.Provenance.InstanceId} (hop {result.Provenance.Hops})");
            }
            else
            {
                sb.AppendLine($"  ID: {result.Id} | Confidence: {result.Confidence:F2} | Source: {source}");
            }
            sb.AppendLine();
        }

        if (response.Federation is not null)
        {
            sb.AppendLine($"--- Federation: consulted {response.Federation.PeersConsulted.Count} peer(s), {response.Federation.LocalResultCount} local + {response.Federation.FederatedResultCount} federated results ---");
        }

        return sb.ToString().TrimEnd();
    }

    [McpServerTool(Name = "get_context"), Description("Get relevant knowledge context formatted for LLM consumption")]
    public static async Task<string> GetContext(
        IFederatedSearchService searchService,
        [Description("The topic to get context for")] string topic,
        [Description("The knowledge domain (optional — searches all domains if omitted)")] string? domain = null,
        [Description("Approximate maximum tokens for the context")] int maxTokens = 2000,
        CancellationToken ct = default)
    {
        var request = new FederatedContextRequest
        {
            Topic = topic,
            Domain = domain,
            MaxTokens = maxTokens
        };

        var response = await searchService.GetContextAsync(request, federation: null, ct);

        if (response.FactCount == 0)
        {
            return $"No knowledge available for topic '{topic}'{(domain is not null ? $" in domain '{domain}'" : "")}.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"# Knowledge Context: {topic}");
        sb.AppendLine($"**Domain**: {domain ?? "All Domains"}");
        sb.AppendLine();
        sb.Append(response.Context);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine($"*{response.FactCount} facts | ~{response.ApproximateTokens} tokens*");

        if (response.Federation is not null)
        {
            sb.AppendLine($"*Federation: {response.Federation.PeersConsulted.Count} peer(s) consulted*");
        }

        return sb.ToString().TrimEnd();
    }

    [McpServerTool(Name = "list_available_domains"), Description("List all available knowledge domains across this instance and federated peers")]
    public static async Task<string> ListAvailableDomains(
        ISourceRepository sourceRepository,
        IFederationPeerRepository peerRepository,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder();

        // Local domains
        var sources = await sourceRepository.GetAllAsync(ct);
        var localDomains = sources
            .Select(s => s.Domain)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d)
            .ToList();

        sb.AppendLine("## Local Domains");
        if (localDomains.Count == 0)
        {
            sb.AppendLine("No local domains. Add sources or facts to create domains.");
        }
        else
        {
            foreach (var domain in localDomains)
            {
                var sourceCount = sources.Count(s => string.Equals(s.Domain, domain, StringComparison.OrdinalIgnoreCase));
                sb.AppendLine($"- **{domain}** ({sourceCount} source(s))");
            }
        }

        // Peer domains
        var peers = await peerRepository.GetHealthyAsync(ct);
        if (peers.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Federated Peer Domains");
            foreach (var peer in peers)
            {
                if (peer.Domains.Count == 0) continue;
                sb.AppendLine($"- **{peer.InstanceId}** ({peer.BaseUrl}):");
                foreach (var domain in peer.Domains)
                {
                    sb.AppendLine($"  - {domain.Name} ({domain.FactCount} facts, confidence: {domain.Confidence:F2})");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }
}
