using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class DomainTrustPolicyRepository : IDomainTrustPolicyRepository
{
    private readonly DbConnectionFactory _db;

    public DomainTrustPolicyRepository(DbConnectionFactory db) => _db = db;

    public async Task<DomainTrustPolicy?> GetByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<DomainTrustPolicy>(
            "SELECT * FROM domain_trust_policy WHERE domain = @domain",
            new { domain });
    }

    public async Task<List<DomainTrustPolicy>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<DomainTrustPolicy>(
            "SELECT * FROM domain_trust_policy ORDER BY domain");
        return results.AsList();
    }

    public async Task UpsertAsync(DomainTrustPolicy policy, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO domain_trust_policy (domain, require_primary_source, min_corroboration,
                auto_accept_tiers, flag_for_review_tiers, temporal_validity_required, min_confidence_score)
            VALUES (@Domain, @RequirePrimarySource, @MinCorroboration,
                @AutoAcceptTiers, @FlagForReviewTiers, @TemporalValidityRequired, @MinConfidenceScore)
            ON CONFLICT (domain) DO UPDATE
            SET require_primary_source = @RequirePrimarySource,
                min_corroboration = @MinCorroboration,
                auto_accept_tiers = @AutoAcceptTiers,
                flag_for_review_tiers = @FlagForReviewTiers,
                temporal_validity_required = @TemporalValidityRequired,
                min_confidence_score = @MinConfidenceScore
            """,
            policy);
    }
}
