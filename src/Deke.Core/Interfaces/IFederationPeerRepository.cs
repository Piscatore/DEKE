using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IFederationPeerRepository
{
    Task<FederationPeer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FederationPeer?> GetByInstanceIdAsync(string instanceId, CancellationToken ct = default);
    Task<List<FederationPeer>> GetHealthyAsync(CancellationToken ct = default);
    Task<List<FederationPeer>> GetAllAsync(CancellationToken ct = default);
    Task<Guid> AddAsync(FederationPeer peer, CancellationToken ct = default);
    Task UpdateAsync(FederationPeer peer, CancellationToken ct = default);
    Task UpsertAsync(FederationPeer peer, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
