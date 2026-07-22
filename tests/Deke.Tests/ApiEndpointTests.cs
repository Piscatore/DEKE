using Deke.Api.Endpoints;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Deke.Tests;

public class ApiEndpointTests
{
    // ---- PUT /api/facts/{id} ----

    [Fact]
    public async Task UpdateFact_ReturnsNotFound_WhenFactMissing()
    {
        var repo = new FakeFactRepository();
        var embeddings = new FakeEmbeddingService();

        var result = await FactEndpoints.UpdateFact(
            Guid.NewGuid(),
            new UpdateFactRequest("new content", "fishing"),
            repo, embeddings, CancellationToken.None);

        Assert.IsType<NotFound>(result);
        Assert.Empty(repo.Updated);
    }

    [Fact]
    public async Task UpdateFact_ReturnsBadRequest_WhenContentMissing()
    {
        var repo = new FakeFactRepository();
        var embeddings = new FakeEmbeddingService();

        var result = await FactEndpoints.UpdateFact(
            Guid.NewGuid(),
            new UpdateFactRequest("  ", "fishing"),
            repo, embeddings, CancellationToken.None);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, status.StatusCode);
    }

    [Fact]
    public async Task UpdateFact_ReturnsBadRequest_WhenDomainMissing()
    {
        var repo = new FakeFactRepository();
        var embeddings = new FakeEmbeddingService();

        var result = await FactEndpoints.UpdateFact(
            Guid.NewGuid(),
            new UpdateFactRequest("content", ""),
            repo, embeddings, CancellationToken.None);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, status.StatusCode);
    }

    [Fact]
    public async Task UpdateFact_RegeneratesEmbedding_WhenContentChanges()
    {
        var fact = NewFact("old content", confidence: 0.7f);
        var repo = new FakeFactRepository(fact);
        var embeddings = new FakeEmbeddingService();

        var result = await FactEndpoints.UpdateFact(
            fact.Id,
            new UpdateFactRequest("new content", "fishing"),
            repo, embeddings, CancellationToken.None);

        var ok = Assert.IsType<Ok<Fact>>(result);
        Assert.Equal("new content", ok.Value!.Content);
        Assert.Equal(1, embeddings.Calls);
        Assert.Single(repo.Updated);
        Assert.Equal(0.7f, ok.Value.Confidence); // preserved when request omits it
    }

    [Fact]
    public async Task UpdateFact_KeepsEmbedding_WhenContentUnchanged()
    {
        var fact = NewFact("same content");
        var originalEmbedding = fact.Embedding;
        var repo = new FakeFactRepository(fact);
        var embeddings = new FakeEmbeddingService();

        var result = await FactEndpoints.UpdateFact(
            fact.Id,
            new UpdateFactRequest("same content", "fishing", Confidence: 0.4f),
            repo, embeddings, CancellationToken.None);

        var ok = Assert.IsType<Ok<Fact>>(result);
        Assert.Equal(0, embeddings.Calls);
        Assert.Same(originalEmbedding, ok.Value!.Embedding);
        Assert.Equal(0.4f, ok.Value.Confidence); // overridden when provided
    }

    // ---- DELETE /api/facts/{id} ----

    [Fact]
    public async Task DeleteFact_ReturnsNotFound_WhenFactMissing()
    {
        var repo = new FakeFactRepository();

        var result = await FactEndpoints.DeleteFact(
            Guid.NewGuid(), null, repo, CancellationToken.None);

        Assert.IsType<NotFound>(result);
        Assert.Empty(repo.MarkedOutdated);
    }

    [Fact]
    public async Task DeleteFact_MarksOutdated_WithDefaultReason()
    {
        var fact = NewFact("content");
        var repo = new FakeFactRepository(fact);

        var result = await FactEndpoints.DeleteFact(
            fact.Id, null, repo, CancellationToken.None);

        Assert.IsType<NoContent>(result);
        var (id, reason) = Assert.Single(repo.MarkedOutdated);
        Assert.Equal(fact.Id, id);
        Assert.Equal("Deleted via API", reason);
        Assert.True(repo.Facts.ContainsKey(fact.Id)); // soft delete: row survives
    }

    [Fact]
    public async Task DeleteFact_PassesCustomReason()
    {
        var fact = NewFact("content");
        var repo = new FakeFactRepository(fact);

        var result = await FactEndpoints.DeleteFact(
            fact.Id, "superseded by newer fact", repo, CancellationToken.None);

        Assert.IsType<NoContent>(result);
        var (_, reason) = Assert.Single(repo.MarkedOutdated);
        Assert.Equal("superseded by newer fact", reason);
    }

    // ---- PUT /api/sources/{id} ----

    [Fact]
    public async Task UpdateSource_ReturnsNotFound_WhenSourceMissing()
    {
        var repo = new FakeSourceRepository();

        var result = await SourceEndpoints.UpdateSource(
            Guid.NewGuid(),
            new UpdateSourceRequest("https://example.com/feed.rss", "fishing"),
            repo, CancellationToken.None);

        Assert.IsType<NotFound>(result);
        Assert.Empty(repo.Updated);
    }

    [Fact]
    public async Task UpdateSource_ReturnsBadRequest_WhenUrlIsPrivate()
    {
        var source = NewSource();
        var repo = new FakeSourceRepository(source);

        var result = await SourceEndpoints.UpdateSource(
            source.Id,
            new UpdateSourceRequest("http://192.168.1.10/feed.rss", "fishing"),
            repo, CancellationToken.None);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, status.StatusCode);
        Assert.Empty(repo.Updated);
    }

    [Fact]
    public async Task UpdateSource_ReturnsBadRequest_WhenUrlMissing()
    {
        var repo = new FakeSourceRepository();

        var result = await SourceEndpoints.UpdateSource(
            Guid.NewGuid(),
            new UpdateSourceRequest("", "fishing"),
            repo, CancellationToken.None);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, status.StatusCode);
    }

    [Fact]
    public async Task UpdateSource_UpdatesConfiguration_AndPreservesOmittedFields()
    {
        var source = NewSource();
        source.Name = "Original Name";
        source.Type = SourceType.Rss;
        source.CheckInterval = TimeSpan.FromHours(6);
        var repo = new FakeSourceRepository(source);

        var result = await SourceEndpoints.UpdateSource(
            source.Id,
            new UpdateSourceRequest(
                "https://example.org/new-feed.rss",
                "ice-fishing",
                Credibility: 0.9f,
                IsActive: false),
            repo, CancellationToken.None);

        var ok = Assert.IsType<Ok<Source>>(result);
        Assert.Single(repo.Updated);
        Assert.Equal("https://example.org/new-feed.rss", ok.Value!.Url);
        Assert.Equal("ice-fishing", ok.Value.Domain);
        Assert.Equal(0.9f, ok.Value.Credibility);
        Assert.False(ok.Value.IsActive);
        // omitted fields preserved
        Assert.Equal("Original Name", ok.Value.Name);
        Assert.Equal(SourceType.Rss, ok.Value.Type);
        Assert.Equal(TimeSpan.FromHours(6), ok.Value.CheckInterval);
    }

    // ---- helpers & fakes ----

    private static Fact NewFact(string content, float confidence = 1.0f) => new()
    {
        Content = content,
        Domain = "fishing",
        Confidence = confidence,
        Embedding = new float[8]
    };

    private static Source NewSource() => new()
    {
        Url = "https://example.com/feed.rss",
        Domain = "fishing"
    };

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public int Calls { get; private set; }

        public float[] GenerateEmbedding(string text)
        {
            Calls++;
            return new float[8];
        }

        public float[][] GenerateEmbeddings(IEnumerable<string> texts, CancellationToken ct = default) => throw new NotImplementedException();
        public float CosineSimilarity(float[] a, float[] b) => throw new NotImplementedException();
    }

    private sealed class FakeFactRepository : IFactRepository
    {
        public Dictionary<Guid, Fact> Facts { get; } = [];
        public List<Fact> Updated { get; } = [];
        public List<(Guid Id, string Reason)> MarkedOutdated { get; } = [];

        public FakeFactRepository(params Fact[] facts)
        {
            foreach (var fact in facts)
                Facts[fact.Id] = fact;
        }

        public Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Facts.GetValueOrDefault(id));

        public Task UpdateAsync(Fact fact, CancellationToken ct = default)
        {
            Updated.Add(fact);
            Facts[fact.Id] = fact;
            return Task.CompletedTask;
        }

        public Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default)
        {
            MarkedOutdated.Add((id, reason));
            if (Facts.TryGetValue(id, out var fact))
            {
                fact.IsOutdated = true;
                fact.OutdatedReason = reason;
            }
            return Task.CompletedTask;
        }

        public Task<Guid> AddAsync(Fact fact, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<FactSearchResult>> SearchAsync(float[] embedding, string? domain, int limit = 10, float minSimilarity = 0.5f, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<DomainStats>> GetDomainStatsAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Fact?> GetByContentHashAsync(string contentHash, string domain, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Fact?> GetByNormalizedHashAsync(string normalizedHash, string domain, CancellationToken ct = default) => throw new NotImplementedException();
        public Task IncrementCorroborationAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task SetDuplicateOfAsync(Guid id, Guid canonicalId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task SetSimilarityHashAsync(Guid id, long similarityHash, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetPendingSimilarityAsync(int limit, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetPendingSemanticAsync(int limit, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeSourceRepository : ISourceRepository
    {
        public Dictionary<Guid, Source> Sources { get; } = [];
        public List<Source> Updated { get; } = [];

        public FakeSourceRepository(params Source[] sources)
        {
            foreach (var source in sources)
                Sources[source.Id] = source;
        }

        public Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(Sources.GetValueOrDefault(id));

        public Task UpdateAsync(Source source, CancellationToken ct = default)
        {
            Updated.Add(source);
            Sources[source.Id] = source;
            return Task.CompletedTask;
        }

        public Task<Source?> GetByUrlAsync(string url, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetByDomainAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetActiveAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetDueForCheckAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Guid> AddAsync(Source source, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeactivateAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    }
}
