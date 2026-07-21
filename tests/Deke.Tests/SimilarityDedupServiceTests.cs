using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Deke.Tests.Fakes;
using Deke.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class SimilarityDedupServiceTests
{
    private readonly InMemoryFactRepository _facts = new();
    private readonly FakeFactProvenanceRepository _provenance = new();

    private SimilarityDedupService BuildService(DedupConfig config)
    {
        var provider = new ServiceCollection()
            .AddScoped<IFactRepository>(_ => _facts)
            .AddScoped<IFactProvenanceRepository>(_ => _provenance)
            .AddScoped<ILearningLogRepository>(_ => new FakeLearningLogRepository())
            .AddScoped<ISimHasher, SimHasher>()
            .AddScoped<IDuplicateLinker, DuplicateLinker>()
            .BuildServiceProvider();

        return new SimilarityDedupService(
            provider, NullLogger<SimilarityDedupService>.Instance, Options.Create(config));
    }

    [Fact]
    public async Task RunCycleAsync_LinksNearDuplicateToCanonicalAndStampsHash()
    {
        var simHasher = new SimHasher();
        var sourceA = Guid.NewGuid();
        var sourceB = Guid.NewGuid();

        var canonicalText = "the quick brown fox jumps over the lazy dog beside the river bank";
        var canonical = new Fact
        {
            Content = canonicalText,
            Domain = "fishing",
            SourceId = sourceA,
            Confidence = 0.9f,
            SimilarityHash = simHasher.Compute(canonicalText),
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        var nearDup = new Fact
        {
            Content = "the quick brown fox jumps over the lazy dog beside the river shore",
            Domain = "fishing",
            SourceId = sourceB,
            Confidence = 0.9f,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var unrelated = new Fact
        {
            Content = "quarterly earnings beat analyst expectations across the technology sector",
            Domain = "fishing",
            SourceId = Guid.NewGuid(),
            Confidence = 0.9f,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _facts.Seed(canonical, nearDup, unrelated);

        var service = BuildService(new DedupConfig { HammingThreshold = 12, BatchSize = 100 });
        await service.RunCycleAsync(CancellationToken.None);

        // Near-duplicate linked to the canonical, hash stamped, canonical corroborated.
        Assert.Equal(canonical.Id, _facts.Get(nearDup.Id).DuplicateOf);
        Assert.NotNull(_facts.Get(nearDup.Id).SimilarityHash);
        Assert.Equal(1, _facts.Get(canonical.Id).CorroborationCount);
        Assert.Contains(_provenance.Records,
            p => p.FactId == canonical.Id && p.SourceId == sourceB && p.ExtractionMethod == ExtractionMethod.Corroboration);

        // Unrelated fact is hashed but not linked.
        Assert.Null(_facts.Get(unrelated.Id).DuplicateOf);
        Assert.NotNull(_facts.Get(unrelated.Id).SimilarityHash);
    }
}
