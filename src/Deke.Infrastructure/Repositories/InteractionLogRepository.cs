using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class InteractionLogRepository : IInteractionLogRepository
{
    private readonly DbConnectionFactory _db;

    public InteractionLogRepository(DbConnectionFactory db) => _db = db;

    public async Task<Guid> AddAsync(InteractionLog log, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO interaction_logs (id, domain, query, model, returned_fact_ids,
                scores, min_similarity, result_count, duration_ms, federation, created_at)
            VALUES (@Id, @Domain, @Query, @Model, @ReturnedFactIds,
                @Scores, @MinSimilarity, @ResultCount, @DurationMs, @Federation, @CreatedAt)
            """,
            log);
        return log.Id;
    }
}
