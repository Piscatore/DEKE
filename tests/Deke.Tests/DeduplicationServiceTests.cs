using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Deke.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deke.Tests;

public class DeduplicationServiceTests
{
    private readonly InMemoryFactRepository _facts = new();
    private readonly FakeFactProvenanceRepository _provenance = new();
    private readonly DeduplicationService _service;

    public DeduplicationServiceTests()
    {
        var linker = new DuplicateLinker(_facts, _provenance);
        _service = new DeduplicationService(
            _facts, _provenance, new ContentHasher(), linker,
            NullLogger<DeduplicationService>.Instance);
    }

    private static Fact NewFact(string content, string domain = "fishing", Guid? sourceId = null) => new()
    {
        Content = content,
        Domain = domain,
        SourceId = sourceId ?? Guid.NewGuid(),
        Confidence = 0.9f
    };

    [Fact]
    public async Task IngestAsync_NovelFact_InsertsWithHashesAndFirstSightingProvenance()
    {
        var fact = NewFact("Walleye bite best at dusk.");

        var result = await _service.IngestAsync(fact, ExtractionMethod.RssHarvest);

        Assert.False(result.WasDuplicate);
        Assert.Equal(fact.Id, result.FactId);
        var stored = _facts.Get(fact.Id);
        Assert.NotNull(stored.ContentHash);
        Assert.NotNull(stored.NormalizedHash);
        var prov = Assert.Single(_provenance.Records);
        Assert.Equal(ExtractionMethod.RssHarvest, prov.ExtractionMethod);
    }

    [Fact]
    public async Task IngestAsync_ExactDuplicateFromNewSource_DiscardsAndCorroborates()
    {
        var first = NewFact("Walleye bite best at dusk.");
        await _service.IngestAsync(first, ExtractionMethod.RssHarvest);

        var dup = NewFact("Walleye bite best at dusk.");
        var result = await _service.IngestAsync(dup, ExtractionMethod.WebHarvest);

        Assert.True(result.WasDuplicate);
        Assert.Equal(2, result.MatchedLevel);
        Assert.Equal(first.Id, result.FactId);
        Assert.Single(_facts.All);
        Assert.Equal(1, _facts.Get(first.Id).CorroborationCount);
        Assert.Equal(2, _provenance.Records.Count);
        Assert.Contains(_provenance.Records, p => p.ExtractionMethod == ExtractionMethod.Corroboration);
    }

    [Fact]
    public async Task IngestAsync_NormalizedDuplicate_MatchesAtLevel3()
    {
        await _service.IngestAsync(NewFact("Walleye bite best at dusk."), ExtractionMethod.RssHarvest);

        var dup = NewFact("walleye   bite BEST at dusk!!");
        var result = await _service.IngestAsync(dup, ExtractionMethod.WebHarvest);

        Assert.True(result.WasDuplicate);
        Assert.Equal(3, result.MatchedLevel);
        Assert.Single(_facts.All);
    }

    [Fact]
    public async Task IngestAsync_SameContentDifferentDomain_IsNotDuplicate()
    {
        await _service.IngestAsync(NewFact("Cast into the wind.", domain: "fishing"), ExtractionMethod.RssHarvest);

        var other = NewFact("Cast into the wind.", domain: "sailing");
        var result = await _service.IngestAsync(other, ExtractionMethod.RssHarvest);

        Assert.False(result.WasDuplicate);
        Assert.Equal(2, _facts.All.Count);
    }

    [Fact]
    public async Task IngestAsync_DuplicateFromSameSource_DoesNotDoubleCountCorroboration()
    {
        var source = Guid.NewGuid();
        await _service.IngestAsync(NewFact("Tie a clinch knot.", sourceId: source), ExtractionMethod.RssHarvest);

        await _service.IngestAsync(NewFact("Tie a clinch knot.", sourceId: source), ExtractionMethod.RssHarvest);

        var canonical = _facts.All.Single();
        Assert.Equal(0, canonical.CorroborationCount);
    }
}
