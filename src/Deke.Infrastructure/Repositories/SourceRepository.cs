using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class SourceRepository : ISourceRepository
{
    private readonly DbConnectionFactory _db;

    public SourceRepository(DbConnectionFactory db) => _db = db;

    public async Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Source>(
            "SELECT * FROM sources WHERE id = @id",
            new { id });
    }

    public async Task<List<Source>> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Source>(
            "SELECT * FROM sources WHERE domain = @domain ORDER BY created_at DESC",
            new { domain });
        return results.AsList();
    }

    public async Task<List<Source>> GetActiveAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Source>(
            "SELECT * FROM sources WHERE is_active = TRUE ORDER BY created_at DESC");
        return results.AsList();
    }

    public async Task<List<Source>> GetDueForCheckAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Source>(
            """
            SELECT * FROM sources
            WHERE is_active
              AND (last_checked_at IS NULL OR last_checked_at + check_interval <= NOW())
            ORDER BY last_checked_at NULLS FIRST
            """);
        return results.AsList();
    }

    public async Task<Guid> AddAsync(Source source, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO sources (id, url, domain, name, type, check_interval,
                last_checked_at, last_changed_at, content_hash, credibility,
                is_active, created_at, metadata)
            VALUES (@Id, @Url, @Domain, @Name, @Type, @CheckInterval,
                @LastCheckedAt, @LastChangedAt, @ContentHash, @Credibility,
                @IsActive, @CreatedAt, @Metadata)
            """,
            source);
        return source.Id;
    }

    public async Task UpdateAsync(Source source, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE sources
            SET url = @Url,
                domain = @Domain,
                name = @Name,
                type = @Type,
                check_interval = @CheckInterval,
                last_checked_at = @LastCheckedAt,
                last_changed_at = @LastChangedAt,
                content_hash = @ContentHash,
                credibility = @Credibility,
                is_active = @IsActive,
                metadata = @Metadata
            WHERE id = @Id
            """,
            source);
    }

    public async Task<List<Source>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Source>(
            "SELECT * FROM sources ORDER BY created_at DESC");
        return results.AsList();
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE sources SET is_active = FALSE WHERE id = @id",
            new { id });
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM sources WHERE id = @id",
            new { id });
    }
}
