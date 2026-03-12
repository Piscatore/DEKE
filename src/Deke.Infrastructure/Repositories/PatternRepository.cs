using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class PatternRepository : IPatternRepository
{
    private readonly DbConnectionFactory _db;

    public PatternRepository(DbConnectionFactory db) => _db = db;

    public async Task<Pattern?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Pattern>(
            "SELECT * FROM patterns WHERE id = @id",
            new { id });
    }

    public async Task<List<Pattern>> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Pattern>(
            "SELECT * FROM patterns WHERE domain = @domain ORDER BY confidence DESC",
            new { domain });
        return results.AsList();
    }

    public async Task<List<Pattern>> GetActiveByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Pattern>(
            "SELECT * FROM patterns WHERE domain = @domain AND is_active = TRUE ORDER BY confidence DESC",
            new { domain });
        return results.AsList();
    }

    public async Task<Guid> AddAsync(Pattern pattern, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO patterns (id, description, domain, pattern_type, evidence_fact_ids,
                confidence, occurrence_count, discovered_at, last_validated_at, is_active)
            VALUES (@Id, @Description, @Domain, @PatternType, @EvidenceFactIds,
                @Confidence, @OccurrenceCount, @DiscoveredAt, @LastValidatedAt, @IsActive)
            """,
            pattern);
        return pattern.Id;
    }

    public async Task UpdateAsync(Pattern pattern, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE patterns
            SET description = @Description,
                domain = @Domain,
                pattern_type = @PatternType,
                evidence_fact_ids = @EvidenceFactIds,
                confidence = @Confidence,
                occurrence_count = @OccurrenceCount,
                last_validated_at = @LastValidatedAt,
                is_active = @IsActive
            WHERE id = @Id
            """,
            pattern);
    }
}
