using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Deke.Core.Interfaces;
using ModelContextProtocol.Server;

namespace Deke.Mcp.Tools;

[McpServerToolType]
public class SearchTools
{
    [McpServerTool(Name = "search_knowledge"), Description("Search for facts using semantic similarity")]
    public static async Task<string> SearchKnowledge(
        IFactRepository factRepository,
        IEmbeddingService embeddingService,
        [Description("The search query text")] string query,
        [Description("The knowledge domain to search in")] string domain,
        [Description("Maximum number of results to return")] int limit = 10,
        [Description("Minimum similarity threshold (0.0 to 1.0)")] float minSimilarity = 0.5f,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var embedding = embeddingService.GenerateEmbedding(query);
        var results = await factRepository.SearchAsync(embedding, domain, limit, minSimilarity, ct);
        sw.Stop();

        if (results.Count == 0)
        {
            return $"No results found for '{query}' in domain '{domain}' with minimum similarity {minSimilarity:F2}.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found {results.Count} result(s) for '{query}' in domain '{domain}' ({sw.ElapsedMilliseconds}ms):");
        sb.AppendLine();

        foreach (var result in results)
        {
            sb.AppendLine($"- [{result.Similarity:F2}] {result.Content}");
            sb.AppendLine($"  ID: {result.Id} | Confidence: {result.Confidence:F2} | Source: {result.SourceUrl ?? "N/A"}");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    [McpServerTool(Name = "get_context"), Description("Get relevant knowledge context formatted for LLM consumption")]
    public static async Task<string> GetContext(
        IFactRepository factRepository,
        IEmbeddingService embeddingService,
        [Description("The topic to get context for")] string topic,
        [Description("The knowledge domain")] string domain,
        [Description("Approximate maximum tokens for the context")] int maxTokens = 2000,
        CancellationToken ct = default)
    {
        var embedding = embeddingService.GenerateEmbedding(topic);

        // Fetch more results than we might need, then trim to token budget
        var results = await factRepository.SearchAsync(embedding, domain, 20, 0.4f, ct);

        if (results.Count == 0)
        {
            return $"No knowledge available for topic '{topic}' in domain '{domain}'.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"# Knowledge Context: {topic}");
        sb.AppendLine($"**Domain**: {domain}");
        sb.AppendLine();

        var factCount = 0;

        foreach (var result in results)
        {
            var entry = $"- **[{result.Confidence:P0} confidence]** {result.Content}\n";

            // Rough token estimate: ~4 chars per token
            if ((sb.Length + entry.Length) / 4 > maxTokens)
                break;

            sb.Append(entry);
            factCount++;
        }

        var approximateTokens = sb.Length / 4;
        sb.AppendLine();
        sb.AppendLine($"---");
        sb.AppendLine($"*{factCount} facts | ~{approximateTokens} tokens*");

        return sb.ToString().TrimEnd();
    }

    [McpServerTool(Name = "list_domains"), Description("List all available knowledge domains")]
    public static async Task<string> ListDomains(
        ISourceRepository sourceRepository,
        CancellationToken ct = default)
    {
        var sources = await sourceRepository.GetAllAsync(ct);
        var domains = sources
            .Select(s => s.Domain)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d)
            .ToList();

        if (domains.Count == 0)
        {
            return "No domains found. Add sources or facts to create domains.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Available domains ({domains.Count}):");
        sb.AppendLine();

        foreach (var domain in domains)
        {
            var sourceCount = sources.Count(s => string.Equals(s.Domain, domain, StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"- **{domain}** ({sourceCount} source(s))");
        }

        return sb.ToString().TrimEnd();
    }
}
