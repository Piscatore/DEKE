using System.Diagnostics;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/search").WithTags("Search");

        group.MapGet("/", SearchFacts)
            .WithName("SearchFacts")
            .WithDescription("Search for facts using semantic similarity");

        group.MapGet("/context", GetContext)
            .WithName("GetContext")
            .WithDescription("Get relevant context for a topic, formatted for LLM consumption");
    }

    private static async Task<IResult> SearchFacts(
        string query,
        string domain,
        int limit = 10,
        float minSimilarity = 0.5f,
        IFactRepository? factRepo = null,
        IEmbeddingService? embeddings = null,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var embedding = embeddings!.GenerateEmbedding(query);
        var results = await factRepo!.SearchAsync(embedding, domain, limit, minSimilarity, ct);
        sw.Stop();

        return Results.Ok(new SearchResponse
        {
            Query = query,
            Domain = domain,
            Results = results,
            TotalCount = results.Count,
            SearchDuration = sw.Elapsed
        });
    }

    private static async Task<IResult> GetContext(
        string topic,
        string domain,
        int maxTokens = 2000,
        IFactRepository? factRepo = null,
        IEmbeddingService? embeddings = null,
        CancellationToken ct = default)
    {
        var embedding = embeddings!.GenerateEmbedding(topic);
        var facts = await factRepo!.SearchAsync(embedding, domain, limit: 30, minSimilarity: 0.4f, ct);

        var sb = new StringBuilder();
        sb.AppendLine($"## Domain Knowledge: {domain}");
        sb.AppendLine($"### Topic: {topic}");
        sb.AppendLine();

        var tokenEstimate = 0;
        var factCount = 0;

        foreach (var fact in facts.OrderByDescending(f => f.Similarity))
        {
            var line = $"- {fact.Content}";
            var lineTokens = line.Length / 4;
            if (tokenEstimate + lineTokens > maxTokens) break;
            sb.AppendLine(line);
            tokenEstimate += lineTokens;
            factCount++;
        }

        return Results.Ok(new ContextResponse
        {
            Topic = topic,
            Domain = domain,
            Context = sb.ToString(),
            FactCount = factCount,
            ApproximateTokens = tokenEstimate
        });
    }
}
