using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Embeddings;
using Deke.Infrastructure.Federation;
using Deke.Infrastructure.Trust;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class InteractionLoggingTests
{
    [Fact]
    public async Task SearchAsync_TopLevelQuery_WritesInteractionLog()
    {
        var factResult = new FactSearchResult
        {
            Id = Guid.NewGuid(),
            Content = "Brook trout thrive in cold water",
            Domain = "fishing",
            Confidence = 0.9f,
            Similarity = 0.85f,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var interactionLogs = new FakeInteractionLogRepository();
        var service = BuildService(new FakeFactRepository([factResult]), interactionLogs);

        var request = new FederatedSearchRequest
        {
            Query = "brook trout",
            Domain = "fishing",
            Limit = 10,
            MinSimilarity = 0.5f
        };

        var response = await service.SearchAsync(request);

        Assert.Single(response.Results);
        var log = Assert.Single(interactionLogs.Logged);
        Assert.Equal("brook trout", log.Query);
        Assert.Equal("fishing", log.Domain);
        Assert.Equal([factResult.Id], log.ReturnedFactIds);
        Assert.Single(log.Scores);
        Assert.Equal(1, log.ResultCount);
        Assert.False(string.IsNullOrEmpty(log.Model));
    }

    [Fact]
    public async Task SearchAsync_RelayedPeerQuery_DoesNotWriteInteractionLog()
    {
        var interactionLogs = new FakeInteractionLogRepository();
        var service = BuildService(new FakeFactRepository([]), interactionLogs);

        var request = new FederatedSearchRequest { Query = "relayed", Limit = 10, MinSimilarity = 0.5f };
        var federationContext = new FederationContext { QueryOrigin = "peer-instance" };

        await service.SearchAsync(request, federationContext);

        Assert.Empty(interactionLogs.Logged);
    }

    private static FederatedSearchService BuildService(
        IFactRepository factRepo, IInteractionLogRepository interactionLogs)
    {
        var embeddingsConfig = new EmbeddingsConfig { ModelPath = "test-model.onnx", VocabPath = "vocab.txt" };
        var federationClient = new FederationClient(new FakeHttpClientFactory(), NullLogger<FederationClient>.Instance);

        return new FederatedSearchService(
            factRepo,
            new FakeFederationPeerRepository(),
            new FakeEmbeddingService(),
            federationClient,
            new TrustScoringService(),
            interactionLogs,
            embeddingsConfig,
            Options.Create(new FederationConfig()),
            NullLogger<FederatedSearchService>.Instance);
    }

    private sealed class FakeFactRepository(List<FactSearchResult> results) : IFactRepository
    {
        public Task<List<FactSearchResult>> SearchAsync(
            float[] embedding, string? domain, int limit = 10, float minSimilarity = 0.5f, CancellationToken ct = default)
            => Task.FromResult(results);

        public Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Guid> AddAsync(Fact fact, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpdateAsync(Fact fact, CancellationToken ct = default) => throw new NotImplementedException();
        public Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default) => throw new NotImplementedException();
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

    private sealed class FakeInteractionLogRepository : IInteractionLogRepository
    {
        public List<InteractionLog> Logged { get; } = [];

        public Task<Guid> AddAsync(InteractionLog log, CancellationToken ct = default)
        {
            Logged.Add(log);
            return Task.FromResult(log.Id);
        }
    }

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public float[] GenerateEmbedding(string text) => new float[8];
        public float[][] GenerateEmbeddings(IEnumerable<string> texts, CancellationToken ct = default) => throw new NotImplementedException();
        public float CosineSimilarity(float[] a, float[] b) => throw new NotImplementedException();
    }

    private sealed class FakeFederationPeerRepository : IFederationPeerRepository
    {
        public Task<FederationPeer?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<FederationPeer?> GetByInstanceIdAsync(string instanceId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<FederationPeer>> GetHealthyAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<FederationPeer>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Guid> AddAsync(FederationPeer peer, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpdateAsync(FederationPeer peer, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpsertAsync(FederationPeer peer, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => throw new NotImplementedException();
    }
}
