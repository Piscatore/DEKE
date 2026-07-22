using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Deke.Tests.Fakes;
using Deke.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class SemanticDedupServiceTests
{
    private readonly InMemoryFactRepository _facts = new();
    private readonly FakeFactProvenanceRepository _provenance = new();

    private SemanticDedupService BuildService(DedupConfig config)
    {
        var provider = new ServiceCollection()
            .AddScoped<IFactRepository>(_ => _facts)
            .AddScoped<IFactProvenanceRepository>(_ => _provenance)
            .AddScoped<ILearningLogRepository>(_ => new FakeLearningLogRepository())
            .AddScoped<IDuplicateLinker, DuplicateLinker>()
            .BuildServiceProvider();

        return new SemanticDedupService(
            provider, NullLogger<SemanticDedupService>.Instance, Options.Create(config));
    }

    [Fact]
    public async Task RunCycleAsync_LinksSemanticNeighborToOlderCanonical()
    {
        var sourceA = Guid.NewGuid();
        var sourceB = Guid.NewGuid();

        var canonical = new Fact
        {
            Content = "Walleye feed most actively at dusk.",
            Domain = "fishing",
            SourceId = sourceA,
            Confidence = 0.9f,
            Embedding = [1f, 0f, 0f, 0f],
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        var semanticDup = new Fact
        {
            Content = "At dusk walleye are at their most active feeding.",
            Domain = "fishing",
            SourceId = sourceB,
            Confidence = 0.9f,
            Embedding = [0.99f, 0.14f, 0f, 0f],
            CreatedAt = DateTimeOffset.UtcNow
        };
        var unrelated = new Fact
        {
            Content = "Sailboats need wind to move.",
            Domain = "fishing",
            SourceId = Guid.NewGuid(),
            Confidence = 0.9f,
            Embedding = [0f, 0f, 1f, 0f],
            CreatedAt = DateTimeOffset.UtcNow
        };
        _facts.Seed(canonical, semanticDup, unrelated);

        var service = BuildService(new DedupConfig { SemanticThreshold = 0.92f, BatchSize = 100 });
        await service.RunCycleAsync(CancellationToken.None);

        // Newer semantic neighbor collapses onto the older canonical; canonical is untouched.
        Assert.Equal(canonical.Id, _facts.Get(semanticDup.Id).DuplicateOf);
        Assert.Null(_facts.Get(canonical.Id).DuplicateOf);
        Assert.Equal(1, _facts.Get(canonical.Id).CorroborationCount);
        Assert.Contains(_provenance.Records,
            p => p.FactId == canonical.Id && p.SourceId == sourceB && p.ExtractionMethod == ExtractionMethod.Corroboration);

        // Dissimilar fact is left alone.
        Assert.Null(_facts.Get(unrelated.Id).DuplicateOf);
    }
}
