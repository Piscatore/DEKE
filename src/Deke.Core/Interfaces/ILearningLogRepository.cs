using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface ILearningLogRepository
{
    Task<Guid> AddAsync(LearningLog log, CancellationToken ct = default);
    Task UpdateAsync(LearningLog log, CancellationToken ct = default);
    Task<List<LearningLog>> GetByDomainAsync(string domain, CancellationToken ct = default);
    Task<LearningLog?> GetLatestAsync(string domain, CancellationToken ct = default);
}
