using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class AdvisoryInteractionRepository : IAdvisoryInteractionRepository
{
    private readonly DbConnectionFactory _db;

    public AdvisoryInteractionRepository(DbConnectionFactory db) => _db = db;

    public async Task<Guid> AddAsync(AdvisoryInteraction interaction, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO advisory_interactions (id, domain, query, stakes, model,
                cited_fact_ids, fact_confidences, confidence_band, knowledge_gaps,
                raw_output, contains_conflicting, created_at)
            VALUES (@Id, @Domain, @Query, @Stakes, @Model,
                @CitedFactIds, @FactConfidences, @ConfidenceBand, @KnowledgeGaps,
                @RawOutput, @ContainsConflicting, @CreatedAt)
            """,
            // Enum names are passed explicitly: Dapper converts enum parameters to their
            // underlying int on write (custom type handlers only apply on read), which would
            // store "1"/"0" instead of readable names in the text columns.
            new
            {
                interaction.Id,
                interaction.Domain,
                interaction.Query,
                Stakes = interaction.Stakes.ToString(),
                interaction.Model,
                interaction.CitedFactIds,
                interaction.FactConfidences,
                ConfidenceBand = interaction.ConfidenceBand.ToString(),
                interaction.KnowledgeGaps,
                interaction.RawOutput,
                interaction.ContainsConflicting,
                interaction.CreatedAt
            });
        return interaction.Id;
    }
}
