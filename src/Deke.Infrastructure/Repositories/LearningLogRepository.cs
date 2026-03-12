using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class LearningLogRepository : ILearningLogRepository
{
    private readonly DbConnectionFactory _db;

    public LearningLogRepository(DbConnectionFactory db) => _db = db;

    public async Task<Guid> AddAsync(LearningLog log, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO learning_logs (id, domain, cycle_type, started_at, completed_at,
                facts_added, facts_updated, facts_outdated, patterns_discovered,
                relations_added, notes, error_message)
            VALUES (@Id, @Domain, @CycleType, @StartedAt, @CompletedAt,
                @FactsAdded, @FactsUpdated, @FactsOutdated, @PatternsDiscovered,
                @RelationsAdded, @Notes, @ErrorMessage)
            """,
            log);
        return log.Id;
    }

    public async Task UpdateAsync(LearningLog log, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE learning_logs
            SET completed_at = @CompletedAt,
                facts_added = @FactsAdded,
                facts_updated = @FactsUpdated,
                facts_outdated = @FactsOutdated,
                patterns_discovered = @PatternsDiscovered,
                relations_added = @RelationsAdded,
                notes = @Notes,
                error_message = @ErrorMessage
            WHERE id = @Id
            """,
            log);
    }

    public async Task<List<LearningLog>> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<LearningLog>(
            "SELECT * FROM learning_logs WHERE domain = @domain ORDER BY started_at DESC",
            new { domain });
        return results.AsList();
    }

    public async Task<LearningLog?> GetLatestAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<LearningLog>(
            "SELECT * FROM learning_logs WHERE domain = @domain ORDER BY started_at DESC LIMIT 1",
            new { domain });
    }
}
