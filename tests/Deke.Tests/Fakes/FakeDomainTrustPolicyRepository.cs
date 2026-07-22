using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Tests.Fakes;

public sealed class FakeDomainTrustPolicyRepository : IDomainTrustPolicyRepository
{
    private readonly Dictionary<string, DomainTrustPolicy> _policies = new();

    public void Seed(DomainTrustPolicy policy) => _policies[policy.Domain] = policy;

    public Task<DomainTrustPolicy?> GetByDomainAsync(string domain, CancellationToken ct = default)
        => Task.FromResult(_policies.GetValueOrDefault(domain));

    public Task<List<DomainTrustPolicy>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult(_policies.Values.ToList());

    public Task UpsertAsync(DomainTrustPolicy policy, CancellationToken ct = default)
    {
        _policies[policy.Domain] = policy;
        return Task.CompletedTask;
    }
}
