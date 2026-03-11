using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IPatternRepository
{
    Task<Pattern?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Pattern>> GetByDomainAsync(string domain, CancellationToken ct = default);
    Task<List<Pattern>> GetActiveByDomainAsync(string domain, CancellationToken ct = default);
    Task<Guid> AddAsync(Pattern pattern, CancellationToken ct = default);
    Task UpdateAsync(Pattern pattern, CancellationToken ct = default);
}
