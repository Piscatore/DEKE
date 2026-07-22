using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Ingestion;

public class DuplicateLinker : IDuplicateLinker
{
    private readonly IFactRepository _facts;
    private readonly IFactProvenanceRepository _provenance;

    public DuplicateLinker(IFactRepository facts, IFactProvenanceRepository provenance)
    {
        _facts = facts;
        _provenance = provenance;
    }

    public async Task<bool> CorroborateAsync(
        Guid canonicalId, Guid? duplicateSourceId, float confidence, CancellationToken ct = default)
    {
        if (duplicateSourceId is not Guid source)
            return false;

        var links = await _provenance.GetByFactIdAsync(canonicalId, ct);
        var isNewSource = !links.Any(p => p.SourceId == source);

        await _provenance.AddAsync(new FactProvenance
        {
            FactId = canonicalId,
            SourceId = source,
            ExtractionMethod = ExtractionMethod.Corroboration,
            ExtractionConfidence = confidence
        }, ct);

        if (isNewSource)
            await _facts.IncrementCorroborationAsync(canonicalId, ct);

        return isNewSource;
    }
}
