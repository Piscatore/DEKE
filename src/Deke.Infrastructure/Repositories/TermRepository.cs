using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class TermRepository : ITermRepository
{
    private readonly DbConnectionFactory _db;

    public TermRepository(DbConnectionFactory db) => _db = db;

    public async Task<Term?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Term>(
            "SELECT * FROM terms WHERE id = @id",
            new { id });
    }

    public async Task<List<Term>> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<Term>(
            "SELECT * FROM terms WHERE domain = @domain ORDER BY canonical_form",
            new { domain });
        return results.AsList();
    }

    public async Task<Term?> GetByCanonicalFormAsync(string canonicalForm, string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Term>(
            "SELECT * FROM terms WHERE canonical_form = @canonicalForm AND domain = @domain",
            new { canonicalForm, domain });
    }

    public async Task<Guid> AddAsync(Term term, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO terms (id, canonical_form, domain, contexts, translations, created_at, updated_at)
            VALUES (@Id, @CanonicalForm, @Domain, @Contexts, @Translations, @CreatedAt, @UpdatedAt)
            """,
            term);
        return term.Id;
    }

    public async Task UpdateAsync(Term term, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        term.UpdatedAt = DateTimeOffset.UtcNow;
        await conn.ExecuteAsync(
            """
            UPDATE terms
            SET canonical_form = @CanonicalForm,
                domain = @Domain,
                contexts = @Contexts,
                translations = @Translations,
                updated_at = @UpdatedAt
            WHERE id = @Id
            """,
            term);
    }
}
