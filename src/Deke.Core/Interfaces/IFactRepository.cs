using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IFactRepository
{
    Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default);
    Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default);

    Task<List<FactSearchResult>> SearchAsync(
        float[] embedding,
        string domain,
        int limit = 10,
        float minSimilarity = 0.5f,
        CancellationToken ct = default);

    Task<Guid> AddAsync(Fact fact, CancellationToken ct = default);
    Task UpdateAsync(Fact fact, CancellationToken ct = default);
    Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default);

    Task<int> GetCountAsync(string domain, CancellationToken ct = default);
    Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default);
    Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default);

    Task<List<DomainStats>> GetDomainStatsAsync(CancellationToken ct = default);
}
