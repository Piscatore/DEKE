using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Deke.Tests.Fakes;
using Deke.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class ContradictionDetectionServiceTests
{
    private readonly InMemoryFactRepository _facts = new();
    private readonly FakeFactRelationRepository _relations = new();
    private readonly FakeLearningLogRepository _learningLog = new();

    private ContradictionDetectionService BuildService(ContradictionDetectionConfig config)
    {
        var provider = new ServiceCollection()
            .AddScoped<IFactRepository>(_ => _facts)
            .AddScoped<IFactRelationRepository>(_ => _relations)
            .AddScoped<ILearningLogRepository>(_ => _learningLog)
            .BuildServiceProvider();

        return new ContradictionDetectionService(
            provider, NullLogger<ContradictionDetectionService>.Instance, Options.Create(config));
    }

    /// <summary>
    /// Unit vector at the given angle in the first two dimensions (zero-padded
    /// to 8). Cosine similarity between Embedding(a) and Embedding(b) is exactly
    /// cos(a - b) degrees, so tests can target a precise similarity band.
    /// </summary>
    private static float[] Embedding(double angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180.0;
        return [(float)Math.Cos(radians), (float)Math.Sin(radians), 0, 0, 0, 0, 0, 0];
    }

    [Fact]
    public async Task RunCycleAsync_FactsInSimilarityBand_AreFlaggedAndLinked()
    {
        var factA = new Fact
        {
            Content = "content A",
            Domain = "fishing",
            TrustState = TrustState.Accepted,
            Embedding = Embedding(0),
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        var factB = new Fact
        {
            Content = "content B",
            Domain = "fishing",
            TrustState = TrustState.Accepted,
            Embedding = Embedding(30), // cosine similarity to factA: cos(30 deg) = ~0.866
            CreatedAt = DateTimeOffset.UtcNow
        };
        _facts.Seed(factA, factB);

        // Band wide enough to catch these two near-but-not-duplicate embeddings.
        var service = BuildService(new ContradictionDetectionConfig
        {
            BatchSize = 100,
            MinSimilarity = 0.0f,
            MaxSimilarity = 0.999999f
        });
        await service.RunCycleAsync(CancellationToken.None);

        Assert.True(_facts.Get(factA.Id).ContradictionFlag);
        Assert.True(_facts.Get(factB.Id).ContradictionFlag);
        Assert.Equal(TrustState.Contested, _facts.Get(factA.Id).TrustState);
        Assert.Equal(TrustState.Contested, _facts.Get(factB.Id).TrustState);
        Assert.Contains(_relations.Records, r => r.RelationType == "contradicts"
            && ((r.FromFactId == factA.Id && r.ToFactId == factB.Id) || (r.FromFactId == factB.Id && r.ToFactId == factA.Id)));
    }

    [Fact]
    public async Task RunCycleAsync_SamePairTwice_DoesNotDuplicateTheRelation()
    {
        var factA = new Fact { Content = "content A", Domain = "fishing", TrustState = TrustState.Accepted, Embedding = Embedding(0) };
        var factB = new Fact { Content = "content B", Domain = "fishing", TrustState = TrustState.Accepted, Embedding = Embedding(30) };
        _facts.Seed(factA, factB);

        var config = new ContradictionDetectionConfig { BatchSize = 100, MinSimilarity = 0.0f, MaxSimilarity = 0.999999f };
        var service = BuildService(config);
        await service.RunCycleAsync(CancellationToken.None);

        // Reset the flag-driven exclusion so the same pair is re-scanned...
        _facts.Get(factA.Id).ContradictionFlag = false;
        _facts.Get(factB.Id).ContradictionFlag = false;
        await service.RunCycleAsync(CancellationToken.None);

        var relationCount = _relations.Records.Count(r => r.RelationType == "contradicts");
        Assert.Equal(1, relationCount);
    }

    [Fact]
    public async Task RunCycleAsync_RejectedFact_IsNotBumpedToContested()
    {
        // factA is Rejected, so it is not itself a scan candidate; factB is, and
        // discovers factA as a similarity-band match while scanning.
        var factA = new Fact { Content = "content A", Domain = "fishing", TrustState = TrustState.Rejected, Embedding = Embedding(0) };
        var factB = new Fact { Content = "content B", Domain = "fishing", TrustState = TrustState.Accepted, Embedding = Embedding(30) };
        _facts.Seed(factA, factB);

        var service = BuildService(new ContradictionDetectionConfig { BatchSize = 100, MinSimilarity = 0.0f, MaxSimilarity = 0.999999f });
        await service.RunCycleAsync(CancellationToken.None);

        Assert.Equal(TrustState.Rejected, _facts.Get(factA.Id).TrustState);
        Assert.True(_facts.Get(factA.Id).ContradictionFlag);
    }
}
