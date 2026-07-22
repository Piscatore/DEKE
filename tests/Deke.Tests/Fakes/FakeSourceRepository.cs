using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Tests.Fakes;

/// <summary>
/// In-memory <see cref="ISourceRepository"/> for worker tests that only need
/// source-tier lookups; unrelated members throw.
/// </summary>
public sealed class FakeSourceRepository : ISourceRepository
{
    private readonly Dictionary<Guid, Source> _sources = new();

    public void Seed(params Source[] sources)
    {
        foreach (var source in sources)
            _sources[source.Id] = source;
    }

    public Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_sources.GetValueOrDefault(id));

    public Task<Source?> GetByUrlAsync(string url, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<Source>> GetByDomainAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<Source>> GetActiveAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<Source>> GetDueForCheckAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Guid> AddAsync(Source source, CancellationToken ct = default) => throw new NotImplementedException();
    public Task UpdateAsync(Source source, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<Source>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task DeactivateAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
}
