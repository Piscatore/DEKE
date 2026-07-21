using Deke.Core.Models;

namespace Deke.Core.Interfaces;

/// <summary>
/// Shared ingest gateway. All fact inserts route through this so that
/// synchronous dedup levels 1-3 run and a provenance link is recorded on
/// every insert (first sighting or duplicate).
/// </summary>
public interface IDeduplicationService
{
    /// <summary>
    /// Runs synchronous dedup levels 1-3 for a fully-built fact (embedding set).
    /// On a duplicate the incoming fact is discarded, the canonical fact is
    /// corroborated, and a provenance link is written; otherwise the fact is
    /// inserted with its hashes and a first-sighting provenance row.
    /// </summary>
    Task<DedupResult> IngestAsync(Fact fact, ExtractionMethod method, CancellationToken ct = default);
}

/// <summary>Outcome of an ingest: the canonical fact id and whether it was a duplicate.</summary>
public readonly record struct DedupResult(Guid FactId, bool WasDuplicate, int MatchedLevel);
