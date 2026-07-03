using System.ComponentModel;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using ModelContextProtocol.Server;

namespace Deke.Mcp.Tools;

[McpServerToolType]
public class AdvisoryTools
{
    [McpServerTool(Name = "GetDomainAdvice"), Description("Get a grounded, cited, confidence-scored advisory answer from a DEKE domain expert, backed by the knowledge base")]
    public static async Task<string> GetDomainAdvice(
        IAdvisoryPipeline pipeline,
        [Description("The question to answer")] string query,
        [Description("The knowledge domain to consult, e.g. 'software-product'")] string domain,
        [Description("Stakes hint influencing model escalation: Low, Medium, or High")] string stakes = "Medium",
        [Description("Session id for multi-turn continuity (optional, currently unused)")] string? sessionId = null,
        CancellationToken ct = default)
    {
        var parsedStakes = Enum.TryParse<Stakes>(stakes, ignoreCase: true, out var value) ? value : Stakes.Medium;

        var request = new AdvisoryRequest
        {
            Query = query,
            Domain = domain,
            SessionId = sessionId,
            Hints = new AdvisoryHints { Stakes = parsedStakes }
        };

        var response = await pipeline.AdviseAsync(request, ct);

        var sb = new StringBuilder();
        sb.AppendLine($"# Advice: {query}");
        sb.AppendLine($"**Domain**: {domain} | **Confidence**: {response.Confidence} | **Model**: {response.Metadata.Model}");
        sb.AppendLine();
        sb.AppendLine(response.Content);
        sb.AppendLine();

        if (response.CitedFactIds.Length > 0)
        {
            sb.AppendLine("## Cited Facts");
            foreach (var id in response.CitedFactIds)
            {
                sb.AppendLine($"- {id}");
            }
            sb.AppendLine();
        }

        if (response.KnowledgeGaps.Length > 0)
        {
            sb.AppendLine("## Knowledge Gaps");
            foreach (var gap in response.KnowledgeGaps)
            {
                sb.AppendLine($"- {gap}");
            }
            sb.AppendLine();
        }

        if (response.ContainsConflictingEvidence)
        {
            sb.AppendLine("> Conflicting information exists on this topic.");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine($"*interaction {response.InteractionId} | {response.Metadata.FactsRetrieved} facts | depth {response.Metadata.KnowledgeDepth:F2}*");

        return sb.ToString().TrimEnd();
    }
}
