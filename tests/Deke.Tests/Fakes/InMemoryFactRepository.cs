using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IFactRepository"/> for dedup tests. Implements the
/// dedup-relevant behavior (hash lookups, ON CONFLICT on domain+normalized
/// hash, corroboration, duplicate marking, cosine search) with real semantics;
/// unrelated members throw.
/// </summary>
public sealed class InMemoryFactRepository : IFactRepository
{
    private readonly Dictionary<Guid, Fact> _store = new();

    public IReadOnlyCollection<Fact> All => _store.Values;
    public Fact Get(Guid id) => _store[id];

    public void Seed(params Fact[] facts)
    {
        foreach (var fact in facts)
            _store[fact.Id] = fact;
    }

    public Task<Guid> AddAsync(Fact fact, CancellationToken ct = default)
    {
        // Emulate UNIQUE(domain, normalized_hash) ON CONFLICT DO NOTHING.
        if (fact.NormalizedHash is not null)
        {
            var clash = _store.Values.FirstOrDefault(f =>
                f.Domain == fact.Domain && f.NormalizedHash == fact.NormalizedHash);
            if (clash is not null)
                return Task.FromResult(clash.Id);
        }

        _store[fact.Id] = fact;
        return Task.FromResult(fact.Id);
    }

    public Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.TryGetValue(id, out var f) ? f : null);

    public Task<Fact?> GetByContentHashAsync(string contentHash, string domain, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(f => f.Domain == domain && f.ContentHash == contentHash));

    public Task<Fact?> GetByNormalizedHashAsync(string normalizedHash, string domain, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(f => f.Domain == domain && f.NormalizedHash == normalizedHash));

    public Task IncrementCorroborationAsync(Guid id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var f))
            f.CorroborationCount++;
        return Task.CompletedTask;
    }

    public Task SetDuplicateOfAsync(Guid id, Guid canonicalId, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var f))
            f.DuplicateOf = canonicalId;
        return Task.CompletedTask;
    }

    public Task SetSimilarityHashAsync(Guid id, long similarityHash, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var f))
            f.SimilarityHash = similarityHash;
        return Task.CompletedTask;
    }

    public Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.Where(f => f.Domain == domain)
            .OrderByDescending(f => f.CreatedAt).Take(limit).ToList());

    public Task<List<Fact>> GetPendingSimilarityAsync(int limit, CancellationToken ct = default) =>
        Task.FromResult(_store.Values
            .Where(f => f.SimilarityHash is null && f.DuplicateOf is null && !f.IsOutdated)
            .OrderByDescending(f => f.CreatedAt).Take(limit).ToList());

    public Task<List<Fact>> GetPendingSemanticAsync(int limit, CancellationToken ct = default) =>
        Task.FromResult(_store.Values
            .Where(f => f.Embedding is { Length: > 0 } && f.DuplicateOf is null && !f.IsOutdated)
            .OrderByDescending(f => f.CreatedAt).Take(limit).ToList());

    public Task<List<FactSearchResult>> SearchAsync(
        float[] embedding, string? domain, int limit = 10, float minSimilarity = 0.5f, CancellationToken ct = default)
    {
        var results = _store.Values
            .Where(f => f.Embedding is { Length: > 0 } && (domain is null || f.Domain == domain))
            .Select(f => (Fact: f, Sim: Cosine(embedding, f.Embedding!)))
            .Where(x => x.Sim > minSimilarity)
            .OrderByDescending(x => x.Sim)
            .Take(limit)
            .Select(x => new FactSearchResult
            {
                Id = x.Fact.Id,
                Content = x.Fact.Content,
                Domain = x.Fact.Domain,
                Confidence = x.Fact.Confidence,
                Similarity = x.Sim,
                SourceId = x.Fact.SourceId,
                CreatedAt = x.Fact.CreatedAt
            })
            .ToList();
        return Task.FromResult(results);
    }

    public Task UpdateAsync(Fact fact, CancellationToken ct = default)
    {
        _store[fact.Id] = fact;
        return Task.CompletedTask;
    }

    private static float Cosine(float[] a, float[] b)
    {
        float dot = 0, na = 0, nb = 0;
        var len = Math.Min(a.Length, b.Length);
        for (var i = 0; i < len; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        return na == 0 || nb == 0 ? 0 : dot / (MathF.Sqrt(na) * MathF.Sqrt(nb));
    }

    public Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<int> GetCountAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<List<DomainStats>> GetDomainStatsAsync(CancellationToken ct = default) => throw new NotImplementedException();
}
