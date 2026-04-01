using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;
using Pgvector;

namespace Deke.Infrastructure.Repositories;

public class FactRepository : IFactRepository
{
    private readonly DbConnectionFactory _db;

    public FactRepository(DbConnectionFactory db) => _db = db;

    private const string SelectColumnsNoEmbedding =
        """
        id, content, domain, confidence, source_id, related_fact_ids,
        entities, metadata, created_at, updated_at, is_outdated, outdated_reason
        """;

    private const string SelectAllColumns = SelectColumnsNoEmbedding + ", embedding";

    public async Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Fact>(
            $"SELECT {SelectColumnsNoEmbedding} FROM facts WHERE id = @id",
            new { id });
    }

    public async Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"SELECT {SelectColumnsNoEmbedding} FROM facts WHERE domain = @domain ORDER BY created_at DESC LIMIT @limit",
            new { domain, limit });
        return results.AsList();
    }

    public async Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"SELECT {SelectColumnsNoEmbedding} FROM facts WHERE source_id = @sourceId ORDER BY created_at DESC",
            new { sourceId });
        return results.AsList();
    }

    public async Task<List<FactSearchResult>> SearchAsync(
        float[] embedding,
        string? domain,
        int limit = 10,
        float minSimilarity = 0.5f,
        CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var vector = new Vector(embedding);

        var domainClause = domain is not null ? "AND f.domain = @domain" : "";

        var results = await conn.QueryAsync<FactSearchResult>(
            $"""
            SELECT f.id, f.content, f.domain, f.confidence, f.source_id, f.created_at,
                   1 - (f.embedding <=> @vector::vector) AS similarity,
                   s.url AS source_url
            FROM facts f
            LEFT JOIN sources s ON s.id = f.source_id
            WHERE f.embedding IS NOT NULL
              {domainClause}
              AND 1 - (f.embedding <=> @vector::vector) > @minSimilarity
            ORDER BY f.embedding <=> @vector::vector
            LIMIT @limit
            """,
            new { vector, domain, minSimilarity, limit });
        return results.AsList();
    }

    public async Task<Guid> AddAsync(Fact fact, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var embeddingParam = fact.Embedding is not null ? new Vector(fact.Embedding) : null;
        await conn.ExecuteAsync(
            """
            INSERT INTO facts (id, content, domain, embedding, confidence, source_id,
                related_fact_ids, entities, metadata, created_at, updated_at,
                is_outdated, outdated_reason)
            VALUES (@Id, @Content, @Domain, @Embedding::vector, @Confidence, @SourceId,
                @RelatedFactIds, @Entities, @Metadata, @CreatedAt, @UpdatedAt,
                @IsOutdated, @OutdatedReason)
            """,
            new
            {
                fact.Id,
                fact.Content,
                fact.Domain,
                Embedding = embeddingParam,
                fact.Confidence,
                fact.SourceId,
                fact.RelatedFactIds,
                fact.Entities,
                fact.Metadata,
                fact.CreatedAt,
                fact.UpdatedAt,
                fact.IsOutdated,
                fact.OutdatedReason
            });
        return fact.Id;
    }

    public async Task UpdateAsync(Fact fact, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        fact.UpdatedAt = DateTimeOffset.UtcNow;
        var embeddingParam = fact.Embedding is not null ? new Vector(fact.Embedding) : null;
        await conn.ExecuteAsync(
            """
            UPDATE facts
            SET content = @Content,
                domain = @Domain,
                embedding = @Embedding::vector,
                confidence = @Confidence,
                source_id = @SourceId,
                related_fact_ids = @RelatedFactIds,
                entities = @Entities,
                metadata = @Metadata,
                updated_at = @UpdatedAt,
                is_outdated = @IsOutdated,
                outdated_reason = @OutdatedReason
            WHERE id = @Id
            """,
            new
            {
                fact.Id,
                fact.Content,
                fact.Domain,
                Embedding = embeddingParam,
                fact.Confidence,
                fact.SourceId,
                fact.RelatedFactIds,
                fact.Entities,
                fact.Metadata,
                fact.UpdatedAt,
                fact.IsOutdated,
                fact.OutdatedReason
            });
    }

    public async Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE facts
            SET is_outdated = TRUE,
                outdated_reason = @reason,
                updated_at = @now
            WHERE id = @id
            """,
            new { id, reason, now = DateTimeOffset.UtcNow });
    }

    public async Task<int> GetCountAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM facts WHERE domain = @domain",
            new { domain });
    }

    public async Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectAllColumns} FROM facts
            WHERE domain = @domain
              AND created_at >= NOW() - @interval::interval
            ORDER BY created_at DESC
            LIMIT @limit
            """,
            new { domain, interval = $"{days} days", limit });
        return results.AsList();
    }

    public async Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectAllColumns} FROM facts f
            WHERE f.domain = @domain
              AND NOT EXISTS (
                  SELECT 1 FROM fact_relations fr
                  WHERE fr.from_fact_id = f.id OR fr.to_fact_id = f.id
              )
            ORDER BY f.created_at DESC
            LIMIT @limit
            """,
            new { domain, limit });
        return results.AsList();
    }

    public async Task<List<DomainStats>> GetDomainStatsAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<DomainStats>(
            """
            SELECT domain, COUNT(*) as fact_count, MAX(created_at) as last_updated_at
            FROM facts
            WHERE NOT is_outdated
            GROUP BY domain
            """);
        return results.AsList();
    }
}
