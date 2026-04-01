using Dapper;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Data;

namespace Deke.Infrastructure.Repositories;

public class FederationPeerRepository : IFederationPeerRepository
{
    private readonly DbConnectionFactory _db;

    public FederationPeerRepository(DbConnectionFactory db) => _db = db;

    public async Task<FederationPeer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<FederationPeer>(
            "SELECT * FROM federation_peers WHERE id = @id",
            new { id });
    }

    public async Task<FederationPeer?> GetByInstanceIdAsync(string instanceId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<FederationPeer>(
            "SELECT * FROM federation_peers WHERE instance_id = @instanceId",
            new { instanceId });
    }

    public async Task<List<FederationPeer>> GetHealthyAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<FederationPeer>(
            "SELECT * FROM federation_peers WHERE is_healthy = TRUE ORDER BY last_seen_at DESC");
        return results.AsList();
    }

    public async Task<List<FederationPeer>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        var results = await conn.QueryAsync<FederationPeer>(
            "SELECT * FROM federation_peers ORDER BY created_at DESC");
        return results.AsList();
    }

    public async Task<Guid> AddAsync(FederationPeer peer, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO federation_peers (id, instance_id, base_url, domains, capabilities,
                protocol_version, last_seen_at, is_healthy, created_at)
            VALUES (@Id, @InstanceId, @BaseUrl, @Domains, @Capabilities,
                @ProtocolVersion, @LastSeenAt, @IsHealthy, @CreatedAt)
            """,
            peer);
        return peer.Id;
    }

    public async Task UpdateAsync(FederationPeer peer, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE federation_peers
            SET base_url = @BaseUrl,
                domains = @Domains,
                capabilities = @Capabilities,
                protocol_version = @ProtocolVersion,
                last_seen_at = @LastSeenAt,
                is_healthy = @IsHealthy
            WHERE id = @Id
            """,
            peer);
    }

    public async Task UpsertAsync(FederationPeer peer, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO federation_peers (id, instance_id, base_url, domains, capabilities,
                protocol_version, last_seen_at, is_healthy, created_at)
            VALUES (@Id, @InstanceId, @BaseUrl, @Domains, @Capabilities,
                @ProtocolVersion, @LastSeenAt, @IsHealthy, @CreatedAt)
            ON CONFLICT (instance_id) DO UPDATE SET
                base_url = EXCLUDED.base_url,
                domains = EXCLUDED.domains,
                capabilities = EXCLUDED.capabilities,
                protocol_version = EXCLUDED.protocol_version,
                last_seen_at = EXCLUDED.last_seen_at,
                is_healthy = EXCLUDED.is_healthy
            """,
            peer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateConnectionAsync(ct);
        await conn.ExecuteAsync(
            "DELETE FROM federation_peers WHERE id = @id",
            new { id });
    }
}
