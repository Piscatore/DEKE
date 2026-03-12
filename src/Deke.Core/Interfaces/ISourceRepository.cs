using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface ISourceRepository
{
    Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Source>> GetByDomainAsync(string domain, CancellationToken ct = default);
    Task<List<Source>> GetActiveAsync(CancellationToken ct = default);
    Task<List<Source>> GetDueForCheckAsync(CancellationToken ct = default);
    Task<Guid> AddAsync(Source source, CancellationToken ct = default);
    Task UpdateAsync(Source source, CancellationToken ct = default);
    Task<List<Source>> GetAllAsync(CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
