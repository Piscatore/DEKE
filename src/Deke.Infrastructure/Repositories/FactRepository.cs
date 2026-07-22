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
        entities, metadata, created_at, updated_at, is_outdated, outdated_reason,
        valid_from, valid_until, corroboration_count, last_verified_at,
        contradiction_flag, trust_state, content_hash, normalized_hash,
        similarity_hash, duplicate_of
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
        float? maxSimilarity = null,
        CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var vector = new Vector(embedding);

        var domainClause = domain is not null ? "AND f.domain = @domain" : "";

        var results = await conn.QueryAsync<FactSearchResult>(
            $"""
            SELECT f.id, f.content, f.domain, f.confidence, f.source_id, f.created_at,
                   f.valid_from, f.valid_until,
                   1 - (f.embedding <=> @vector::vector) AS similarity,
                   s.url AS source_url,
                   COALESCE(s.credibility, 0) AS source_credibility
            FROM facts f
            LEFT JOIN sources s ON s.id = f.source_id
            WHERE f.embedding IS NOT NULL
              {domainClause}
              AND 1 - (f.embedding <=> @vector::vector) > @minSimilarity
              AND (@maxSimilarity IS NULL OR 1 - (f.embedding <=> @vector::vector) <= @maxSimilarity)
            ORDER BY f.embedding <=> @vector::vector
            LIMIT @limit
            """,
            new { vector, domain, minSimilarity, maxSimilarity, limit });
        return results.AsList();
    }

    public async Task<Guid> AddAsync(Fact fact, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var embeddingParam = fact.Embedding is not null ? new Vector(fact.Embedding) : null;
        var parameters = new
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
            fact.OutdatedReason,
            fact.ValidFrom,
            fact.ValidUntil,
            fact.CorroborationCount,
            fact.LastVerifiedAt,
            fact.ContradictionFlag,
            fact.TrustState,
            fact.ContentHash,
            fact.NormalizedHash,
            fact.SimilarityHash,
            fact.DuplicateOf
        };

        // ON CONFLICT guards the per-domain normalized-hash uniqueness. NULL
        // normalized_hash rows never conflict (NULLs are distinct), so legacy /
        // hash-less inserts always proceed. On a lost race the insert is
        // suppressed and we return the winning row's id.
        var insertedId = await conn.ExecuteScalarAsync<Guid?>(
            """
            INSERT INTO facts (id, content, domain, embedding, confidence, source_id,
                related_fact_ids, entities, metadata, created_at, updated_at,
                is_outdated, outdated_reason, valid_from, valid_until,
                corroboration_count, last_verified_at, contradiction_flag, trust_state,
                content_hash, normalized_hash, similarity_hash, duplicate_of)
            VALUES (@Id, @Content, @Domain, @Embedding::vector, @Confidence, @SourceId,
                @RelatedFactIds, @Entities, @Metadata, @CreatedAt, @UpdatedAt,
                @IsOutdated, @OutdatedReason, @ValidFrom, @ValidUntil,
                @CorroborationCount, @LastVerifiedAt, @ContradictionFlag, @TrustState,
                @ContentHash, @NormalizedHash, @SimilarityHash, @DuplicateOf)
            ON CONFLICT (domain, normalized_hash) DO NOTHING
            RETURNING id
            """,
            parameters);

        if (insertedId is Guid id)
            return id;

        return await conn.ExecuteScalarAsync<Guid>(
            "SELECT id FROM facts WHERE domain = @Domain AND normalized_hash = @NormalizedHash",
            parameters);
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
                outdated_reason = @OutdatedReason,
                valid_from = @ValidFrom,
                valid_until = @ValidUntil,
                corroboration_count = @CorroborationCount,
                last_verified_at = @LastVerifiedAt,
                contradiction_flag = @ContradictionFlag,
                trust_state = @TrustState,
                content_hash = @ContentHash,
                normalized_hash = @NormalizedHash,
                similarity_hash = @SimilarityHash,
                duplicate_of = @DuplicateOf
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
                fact.OutdatedReason,
                fact.ValidFrom,
                fact.ValidUntil,
                fact.CorroborationCount,
                fact.LastVerifiedAt,
                fact.ContradictionFlag,
                fact.TrustState,
                fact.ContentHash,
                fact.NormalizedHash,
                fact.SimilarityHash,
                fact.DuplicateOf
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

    public async Task<Fact?> GetByContentHashAsync(string contentHash, string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Fact>(
            $"SELECT {SelectColumnsNoEmbedding} FROM facts WHERE content_hash = @contentHash AND domain = @domain LIMIT 1",
            new { contentHash, domain });
    }

    public async Task<Fact?> GetByNormalizedHashAsync(string normalizedHash, string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Fact>(
            $"SELECT {SelectColumnsNoEmbedding} FROM facts WHERE normalized_hash = @normalizedHash AND domain = @domain LIMIT 1",
            new { normalizedHash, domain });
    }

    public async Task IncrementCorroborationAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE facts
            SET corroboration_count = corroboration_count + 1,
                last_verified_at = @now
            WHERE id = @id
            """,
            new { id, now = DateTimeOffset.UtcNow });
    }

    public async Task SetDuplicateOfAsync(Guid id, Guid canonicalId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE facts SET duplicate_of = @canonicalId, updated_at = @now WHERE id = @id",
            new { id, canonicalId, now = DateTimeOffset.UtcNow });
    }

    public async Task SetSimilarityHashAsync(Guid id, long similarityHash, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE facts SET similarity_hash = @similarityHash WHERE id = @id",
            new { id, similarityHash });
    }

    public async Task<List<Fact>> GetPendingSimilarityAsync(int limit, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectColumnsNoEmbedding} FROM facts
            WHERE similarity_hash IS NULL
              AND duplicate_of IS NULL
              AND is_outdated = FALSE
            ORDER BY created_at DESC
            LIMIT @limit
            """,
            new { limit });
        return results.AsList();
    }

    public async Task<List<Fact>> GetPendingSemanticAsync(int limit, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectAllColumns} FROM facts
            WHERE embedding IS NOT NULL
              AND duplicate_of IS NULL
              AND is_outdated = FALSE
            ORDER BY created_at DESC
            LIMIT @limit
            """,
            new { limit });
        return results.AsList();
    }

    // Quality pipeline (P1-2)

    public async Task<List<Fact>> GetPendingTrustEvaluationAsync(int limit, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectColumnsNoEmbedding} FROM facts
            WHERE trust_state = 'Unscored'
              AND duplicate_of IS NULL
              AND is_outdated = FALSE
            ORDER BY created_at
            LIMIT @limit
            """,
            new { limit });
        return results.AsList();
    }

    public async Task SetTrustStateAsync(Guid id, TrustState state, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE facts SET trust_state = @state, updated_at = @now WHERE id = @id",
            new { id, state, now = DateTimeOffset.UtcNow });
    }

    public async Task<List<Fact>> GetContradictionScanCandidatesAsync(int limit, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectAllColumns} FROM facts
            WHERE trust_state IN ('Accepted', 'Flagged')
              AND NOT contradiction_flag
              AND embedding IS NOT NULL
              AND duplicate_of IS NULL
              AND is_outdated = FALSE
            ORDER BY created_at
            LIMIT @limit
            """,
            new { limit });
        return results.AsList();
    }

    public async Task MarkContradictedAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE facts
            SET contradiction_flag = TRUE,
                trust_state = CASE WHEN trust_state <> 'Rejected' THEN 'Contested' ELSE trust_state END,
                updated_at = @now
            WHERE id = @id
            """,
            new { id, now = DateTimeOffset.UtcNow });
    }

    public async Task<List<Fact>> GetPendingReviewAsync(string? domain, int limit, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var domainClause = domain is not null ? "AND domain = @domain" : "";
        var results = await conn.QueryAsync<Fact>(
            $"""
            SELECT {SelectColumnsNoEmbedding} FROM facts
            WHERE trust_state IN ('Flagged', 'Contested')
              {domainClause}
              AND duplicate_of IS NULL
              AND is_outdated = FALSE
            ORDER BY created_at DESC
            LIMIT @limit
            """,
            new { domain, limit });
        return results.AsList();
    }
}
