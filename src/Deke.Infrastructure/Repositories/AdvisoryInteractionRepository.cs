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
            interaction);
        return interaction.Id;
    }
}
