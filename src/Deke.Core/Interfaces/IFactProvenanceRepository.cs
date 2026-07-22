using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IFactProvenanceRepository
{
    Task<List<FactProvenance>> GetByFactIdAsync(Guid factId, CancellationToken ct = default);
    Task<Guid> AddAsync(FactProvenance provenance, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
