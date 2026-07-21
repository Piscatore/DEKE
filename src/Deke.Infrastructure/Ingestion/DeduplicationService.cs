using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.Extensions.Logging;

namespace Deke.Infrastructure.Ingestion;

/// <summary>
/// Shared ingest gateway. Runs synchronous dedup levels 1-3 and records a
/// provenance link on every insert; duplicates are discarded and the canonical
/// fact is corroborated. Levels 4-5 run asynchronously in the worker.
/// </summary>
public class DeduplicationService : IDeduplicationService
{
    private readonly IFactRepository _facts;
    private readonly IFactProvenanceRepository _provenance;
    private readonly IContentHasher _hasher;
    private readonly IDuplicateLinker _linker;
    private readonly ILogger<DeduplicationService> _logger;

    public DeduplicationService(
        IFactRepository facts,
        IFactProvenanceRepository provenance,
        IContentHasher hasher,
        IDuplicateLinker linker,
        ILogger<DeduplicationService> logger)
    {
        _facts = facts;
        _provenance = provenance;
        _hasher = hasher;
        _linker = linker;
        _logger = logger;
    }

    public async Task<DedupResult> IngestAsync(Fact fact, ExtractionMethod method, CancellationToken ct = default)
    {
        var contentHash = _hasher.ContentHash(fact.Content);
        var normalizedHash = _hasher.NormalizedHash(fact.Content);

        // Level 2: exact content match.
        var existing = await _facts.GetByContentHashAsync(contentHash, fact.Domain, ct);
        if (existing is not null)
            return await CorroborateAsync(existing, fact, 2, ct);

        // Level 3: normalized match.
        existing = await _facts.GetByNormalizedHashAsync(normalizedHash, fact.Domain, ct);
        if (existing is not null)
            return await CorroborateAsync(existing, fact, 3, ct);

        // Novel fact: persist with its hashes. similarity_hash is left NULL for
        // the level-4 job to fill.
        fact.ContentHash = contentHash;
        fact.NormalizedHash = normalizedHash;
        var storedId = await _facts.AddAsync(fact, ct);

        if (storedId != fact.Id)
        {
            // Lost the (domain, normalized_hash) unique-index race between the
            // checks above and the insert: an equal fact landed first.
            var winner = await _facts.GetByIdAsync(storedId, ct);
            if (winner is not null)
                return await CorroborateAsync(winner, fact, 3, ct);
        }

        await WriteProvenanceAsync(storedId, fact.SourceId, method, fact.Confidence, ct);
        return new DedupResult(storedId, false, 0);
    }

    private async Task<DedupResult> CorroborateAsync(
        Fact canonical, Fact incoming, int level, CancellationToken ct)
    {
        await _linker.CorroborateAsync(canonical.Id, incoming.SourceId, incoming.Confidence, ct);

        _logger.LogInformation(
            "Dedup L{Level}: fact {Incoming} duplicates {Canonical} in domain {Domain}",
            level, incoming.Id, canonical.Id, canonical.Domain);

        return new DedupResult(canonical.Id, true, level);
    }

    private async Task WriteProvenanceAsync(
        Guid factId, Guid? sourceId, ExtractionMethod method, float confidence, CancellationToken ct)
    {
        if (sourceId is not Guid source)
            return;

        await _provenance.AddAsync(new FactProvenance
        {
            FactId = factId,
            SourceId = source,
            ExtractionMethod = method,
            ExtractionConfidence = confidence
        }, ct);
    }
}
