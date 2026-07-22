using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Tests.Fakes;

/// <summary>
/// Records provenance writes for assertions and upserts on (FactId, SourceId)
/// to mirror the real ON CONFLICT behavior.
/// </summary>
public sealed class FakeFactProvenanceRepository : IFactProvenanceRepository
{
    public List<FactProvenance> Records { get; } = [];

    public Task<List<FactProvenance>> GetByFactIdAsync(Guid factId, CancellationToken ct = default) =>
        Task.FromResult(Records.Where(p => p.FactId == factId).ToList());

    public Task<Guid> AddAsync(FactProvenance provenance, CancellationToken ct = default)
    {
        var existing = Records.FirstOrDefault(p =>
            p.FactId == provenance.FactId && p.SourceId == provenance.SourceId);

        if (existing is not null)
        {
            existing.ExtractionMethod = provenance.ExtractionMethod;
            existing.ExtractionConfidence = provenance.ExtractionConfidence;
            existing.ExtractedAt = provenance.ExtractedAt;
            return Task.FromResult(existing.Id);
        }

        Records.Add(provenance);
        return Task.FromResult(provenance.Id);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Records.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
