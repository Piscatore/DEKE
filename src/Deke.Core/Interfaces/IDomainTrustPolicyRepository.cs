using Deke.Core.Models;

namespace Deke.Core.Interfaces;

public interface IDomainTrustPolicyRepository
{
    Task<DomainTrustPolicy?> GetByDomainAsync(string domain, CancellationToken ct = default);
    Task<List<DomainTrustPolicy>> GetAllAsync(CancellationToken ct = default);
    Task UpsertAsync(DomainTrustPolicy policy, CancellationToken ct = default);
}
