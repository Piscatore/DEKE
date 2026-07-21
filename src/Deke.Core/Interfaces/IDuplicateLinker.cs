namespace Deke.Core.Interfaces;

/// <summary>
/// Records that a duplicate's source corroborates a canonical fact: writes a
/// Corroboration provenance link and raises the canonical fact's corroboration
/// count once per distinct source. Shared by the synchronous gateway and the
/// asynchronous dedup jobs so the rule lives in one place.
/// </summary>
public interface IDuplicateLinker
{
    /// <summary>
    /// Links <paramref name="duplicateSourceId"/> (when present) to the canonical
    /// fact and increments its corroboration count if that source is new to it.
    /// Returns true when the source was newly corroborating.
    /// </summary>
    Task<bool> CorroborateAsync(Guid canonicalId, Guid? duplicateSourceId, float confidence, CancellationToken ct = default);
}
