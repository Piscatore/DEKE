using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IFactRelationRepository
{
    Task<List<FactRelation>> GetByFactIdAsync(Guid factId, CancellationToken ct = default);
    Task<Guid> AddAsync(FactRelation relation, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<FactRelation>> GetByRelationTypeAsync(string relationType, string domain, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid fromFactId, Guid toFactId, string relationType, CancellationToken ct = default);
}
