using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class FactRelationRepository : IFactRelationRepository
{
    private readonly DbConnectionFactory _db;

    public FactRelationRepository(DbConnectionFactory db) => _db = db;

    public async Task<List<FactRelation>> GetByFactIdAsync(Guid factId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<FactRelation>(
            """
            SELECT * FROM fact_relations
            WHERE from_fact_id = @factId OR to_fact_id = @factId
            ORDER BY created_at DESC
            """,
            new { factId });
        return results.AsList();
    }

    public async Task<Guid> AddAsync(FactRelation relation, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO fact_relations (id, from_fact_id, to_fact_id, relation_type, confidence, created_at)
            VALUES (@Id, @FromFactId, @ToFactId, @RelationType, @Confidence, @CreatedAt)
            """,
            relation);
        return relation.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM fact_relations WHERE id = @id",
            new { id });
    }

    public async Task<bool> ExistsAsync(Guid fromFactId, Guid toFactId, string relationType, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var count = await conn.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(1) FROM fact_relations
            WHERE from_fact_id = @fromFactId
              AND to_fact_id = @toFactId
              AND relation_type = @relationType
            """,
            new { fromFactId, toFactId, relationType });
        return count > 0;
    }

    public async Task<List<FactRelation>> GetByRelationTypeAsync(string relationType, string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<FactRelation>(
            """
            SELECT fr.* FROM fact_relations fr
            INNER JOIN facts f ON f.id = fr.from_fact_id
            WHERE fr.relation_type = @relationType
              AND f.domain = @domain
            ORDER BY fr.confidence DESC
            """,
            new { relationType, domain });
        return results.AsList();
    }
}
