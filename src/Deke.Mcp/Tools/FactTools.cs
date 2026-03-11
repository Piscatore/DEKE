using System.ComponentModel;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using ModelContextProtocol.Server;

namespace Deke.Mcp.Tools;

[McpServerToolType]
public class FactTools
{
    [McpServerTool(Name = "add_fact"), Description("Add a new fact to the knowledge base")]
    public static async Task<string> AddFact(
        IFactRepository factRepository,
        IEmbeddingService embeddingService,
        [Description("The fact content text")] string content,
        [Description("The knowledge domain this fact belongs to")] string domain,
        [Description("Confidence level from 0.0 to 1.0")] float confidence = 1.0f,
        CancellationToken ct = default)
    {
        var embedding = embeddingService.GenerateEmbedding(content);

        var fact = new Fact
        {
            Content = content,
            Domain = domain,
            Embedding = embedding,
            Confidence = confidence
        };

        var id = await factRepository.AddAsync(fact, ct);

        return $"Fact added successfully.\nID: {id}\nDomain: {domain}\nConfidence: {confidence:F2}";
    }

    [McpServerTool(Name = "get_fact"), Description("Get a specific fact by its ID")]
    public static async Task<string> GetFact(
        IFactRepository factRepository,
        [Description("The unique identifier of the fact")] string id,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(id, out var factId))
        {
            return $"Invalid fact ID format: '{id}'. Expected a GUID.";
        }

        var fact = await factRepository.GetByIdAsync(factId, ct);

        if (fact is null)
        {
            return $"Fact with ID '{id}' not found.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"**Fact**: {fact.Id}");
        sb.AppendLine($"**Domain**: {fact.Domain}");
        sb.AppendLine($"**Confidence**: {fact.Confidence:F2}");
        sb.AppendLine($"**Created**: {fact.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}");

        if (fact.SourceId.HasValue)
            sb.AppendLine($"**Source ID**: {fact.SourceId}");

        if (fact.IsOutdated)
            sb.AppendLine($"**OUTDATED**: {fact.OutdatedReason}");

        sb.AppendLine();
        sb.AppendLine($"**Content**:");
        sb.AppendLine(fact.Content);

        return sb.ToString().TrimEnd();
    }

    [McpServerTool(Name = "get_domain_stats"), Description("Get statistics for a knowledge domain")]
    public static async Task<string> GetDomainStats(
        IFactRepository factRepository,
        [Description("The knowledge domain to get statistics for")] string domain,
        CancellationToken ct = default)
    {
        var count = await factRepository.GetCountAsync(domain, ct);

        var sb = new StringBuilder();
        sb.AppendLine($"**Domain**: {domain}");
        sb.AppendLine($"**Total facts**: {count}");

        return sb.ToString().TrimEnd();
    }
}
