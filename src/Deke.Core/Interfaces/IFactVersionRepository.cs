using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IFactVersionRepository
{
    Task<List<FactVersion>> GetByFactIdAsync(Guid factId, CancellationToken ct = default);
    Task<FactVersion?> GetAsOfAsync(Guid factId, DateTimeOffset asOf, CancellationToken ct = default);
    Task<Guid> AddAsync(FactVersion version, CancellationToken ct = default);
}
