using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Deke.Infrastructure.Trust;
using Deke.Tests.Fakes;
using Deke.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class TrustStateEvaluationServiceTests
{
    private readonly InMemoryFactRepository _facts = new();
    private readonly FakeSourceRepository _sources = new();
    private readonly FakeDomainTrustPolicyRepository _policies = new();
    private readonly FakeLearningLogRepository _learningLog = new();

    private TrustStateEvaluationService BuildService(TrustEvaluationConfig config)
    {
        var provider = new ServiceCollection()
            .AddScoped<IFactRepository>(_ => _facts)
            .AddScoped<ISourceRepository>(_ => _sources)
            .AddScoped<IDomainTrustPolicyRepository>(_ => _policies)
            .AddScoped<ILearningLogRepository>(_ => _learningLog)
            .AddScoped<ITrustEvaluator, TrustEvaluator>()
            .BuildServiceProvider();

        return new TrustStateEvaluationService(
            provider, NullLogger<TrustStateEvaluationService>.Instance, Options.Create(config));
    }

    [Fact]
    public async Task RunCycleAsync_NoPolicyConfigured_TransitionsToAccepted()
    {
        var fact = new Fact { Content = "content", Domain = "fishing" };
        _facts.Seed(fact);

        var service = BuildService(new TrustEvaluationConfig { BatchSize = 100 });
        await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(TrustState.Accepted, _facts.Get(fact.Id).TrustState);
    }

    [Fact]
    public async Task RunCycleAsync_TierInFlagForReviewList_TransitionsToFlagged()
    {
        var source = new Source { Url = "https://example.com/feed.rss", Domain = "fishing", SourceTier = SourceTier.Unverified };
        var fact = new Fact { Content = "content", Domain = "fishing", SourceId = source.Id };
        _facts.Seed(fact);
        _sources.Seed(source);
        _policies.Seed(new DomainTrustPolicy
        {
            Domain = "fishing",
            FlagForReviewTiers = [SourceTier.Unverified]
        });

        var service = BuildService(new TrustEvaluationConfig { BatchSize = 100 });
        await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(TrustState.Flagged, _facts.Get(fact.Id).TrustState);
    }

    [Fact]
    public async Task RunCycleAsync_AlreadyScoredFacts_AreNotReEvaluated()
    {
        var fact = new Fact { Content = "content", Domain = "fishing", TrustState = TrustState.Flagged };
        _facts.Seed(fact);

        var service = BuildService(new TrustEvaluationConfig { BatchSize = 100 });
        await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(TrustState.Flagged, _facts.Get(fact.Id).TrustState);
    }
}
