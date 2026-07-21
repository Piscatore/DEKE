using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;
using Pgvector;

namespace Deke.Infrastructure.Repositories;

public class FactVersionRepository : IFactVersionRepository
{
    private readonly DbConnectionFactory _db;

    public FactVersionRepository(DbConnectionFactory db) => _db = db;

    private const string SelectColumns =
        "id, fact_id, content_snapshot, changed_at, change_reason";

    public async Task<List<FactVersion>> GetByFactIdAsync(Guid factId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<FactVersion>(
            $"SELECT {SelectColumns} FROM fact_version WHERE fact_id = @factId ORDER BY changed_at DESC",
            new { factId });
        return results.AsList();
    }

    public async Task<FactVersion?> GetAsOfAsync(Guid factId, DateTimeOffset asOf, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<FactVersion>(
            $"""
            SELECT {SelectColumns} FROM fact_version
            WHERE fact_id = @factId AND changed_at <= @asOf
            ORDER BY changed_at DESC
            LIMIT 1
            """,
            new { factId, asOf });
    }

    public async Task<Guid> AddAsync(FactVersion version, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var embeddingParam = version.EmbeddingSnapshot is not null ? new Vector(version.EmbeddingSnapshot) : null;
        await conn.ExecuteAsync(
            """
            INSERT INTO fact_version (id, fact_id, content_snapshot, embedding_snapshot, changed_at, change_reason)
            VALUES (@Id, @FactId, @ContentSnapshot, @Embedding::vector, @ChangedAt, @ChangeReason)
            """,
            new
            {
                version.Id,
                version.FactId,
                version.ContentSnapshot,
                Embedding = embeddingParam,
                version.ChangedAt,
                version.ChangeReason
            });
        return version.Id;
    }
}
