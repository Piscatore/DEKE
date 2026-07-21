using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Tests.Fakes;

public sealed class FakeLearningLogRepository : ILearningLogRepository
{
    public List<LearningLog> Logs { get; } = [];

    public Task<Guid> AddAsync(LearningLog log, CancellationToken ct = default)
    {
        Logs.Add(log);
        return Task.FromResult(log.Id);
    }

    public Task UpdateAsync(LearningLog log, CancellationToken ct = default) => Task.CompletedTask;
    public Task<List<LearningLog>> GetByDomainAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<LearningLog?> GetLatestAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
}
