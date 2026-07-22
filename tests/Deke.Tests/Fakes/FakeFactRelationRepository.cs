using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Tests.Fakes;

public sealed class FakeFactRelationRepository : IFactRelationRepository
{
    public List<FactRelation> Records { get; } = [];

    public Task<List<FactRelation>> GetByFactIdAsync(Guid factId, CancellationToken ct = default) =>
        Task.FromResult(Records.Where(r => r.FromFactId == factId || r.ToFactId == factId).ToList());

    public Task<Guid> AddAsync(FactRelation relation, CancellationToken ct = default)
    {
        Records.Add(relation);
        return Task.FromResult(relation.Id);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Records.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task<List<FactRelation>> GetByRelationTypeAsync(string relationType, string domain, CancellationToken ct = default) =>
        Task.FromResult(Records.Where(r => r.RelationType == relationType).ToList());

    public Task<bool> ExistsAsync(Guid fromFactId, Guid toFactId, string relationType, CancellationToken ct = default) =>
        Task.FromResult(Records.Any(r =>
            r.FromFactId == fromFactId && r.ToFactId == toFactId && r.RelationType == relationType));
}
