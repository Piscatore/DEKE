using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IInteractionLogRepository
{
    Task<Guid> AddAsync(InteractionLog log, CancellationToken ct = default);
}
