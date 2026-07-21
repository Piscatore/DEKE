using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class FactProvenanceRepository : IFactProvenanceRepository
{
    private readonly DbConnectionFactory _db;

    public FactProvenanceRepository(DbConnectionFactory db) => _db = db;

    public async Task<List<FactProvenance>> GetByFactIdAsync(Guid factId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<FactProvenance>(
            "SELECT * FROM fact_provenance WHERE fact_id = @factId ORDER BY extracted_at DESC",
            new { factId });
        return results.AsList();
    }

    public async Task<Guid> AddAsync(FactProvenance provenance, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO fact_provenance (id, fact_id, source_id, extracted_at, extraction_method, extraction_confidence)
            VALUES (@Id, @FactId, @SourceId, @ExtractedAt, @ExtractionMethod, @ExtractionConfidence)
            ON CONFLICT (fact_id, source_id) DO UPDATE
            SET extracted_at = @ExtractedAt,
                extraction_method = @ExtractionMethod,
                extraction_confidence = @ExtractionConfidence
            """,
            provenance);
        return provenance.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM fact_provenance WHERE id = @id",
            new { id });
    }
}
