using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface ITermRepository
{
    Task<Term?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Term>> GetByDomainAsync(string domain, CancellationToken ct = default);
    Task<Term?> GetByCanonicalFormAsync(string canonicalForm, string domain, CancellationToken ct = default);
    Task<Guid> AddAsync(Term term, CancellationToken ct = default);
    Task UpdateAsync(Term term, CancellationToken ct = default);
}
