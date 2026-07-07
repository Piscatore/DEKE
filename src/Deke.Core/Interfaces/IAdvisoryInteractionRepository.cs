using Deke.Core.Models;

namespace Deke.Core.Interfaces;

/// <summary>
/// Append-only persistence for advisory audit records.
/// </summary>
public interface IAdvisoryInteractionRepository
{
    Task<Guid> AddAsync(AdvisoryInteraction interaction, CancellationToken ct = default);
}
