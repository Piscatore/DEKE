using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Advisory;
using Deke.Infrastructure.Trust;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class AdvisoryPipelineTests
{
    private const string Domain = "software-product";

    [Fact]
    public async Task AdviseAsync_WithFacts_ReturnsGroundedResponseWithCitedIds()
    {
        var facts = StrongFacts(10);
        var chat = new FakeChatClient { ResponseText = "Grounded answer." };
        var interactions = new FakeAdvisoryInteractionRepository();
        var pipeline = BuildPipeline(new FakeFactRepository(facts), chat, interactions);

        var response = await pipeline.AdviseAsync(new AdvisoryRequest { Query = "how to version", Domain = Domain });

        Assert.Equal("Grounded answer.", response.Content);
        Assert.NotEqual(ConfidenceBand.Insufficient, response.Confidence);
        Assert.Equal(facts.Select(f => f.Id), response.CitedFactIds);
        Assert.False(string.IsNullOrEmpty(response.InteractionId));
        Assert.Equal(10, response.Metadata.FactsRetrieved);
    }

    [Fact]
    public async Task AdviseAsync_EmptyRetrieval_ReturnsInsufficientAndDoesNotCallModel()
    {
        var chat = new FakeChatClient { ThrowIfCalled = true };
        var interactions = new FakeAdvisoryInteractionRepository();
        var pipeline = BuildPipeline(new FakeFactRepository([]), chat, interactions);

        var response = await pipeline.AdviseAsync(new AdvisoryRequest { Query = "unknown", Domain = Domain });

        Assert.Equal(ConfidenceBand.Insufficient, response.Confidence);
        Assert.Empty(response.CitedFactIds);
        Assert.NotEmpty(response.KnowledgeGaps);
        Assert.Single(interactions.Logged);
        Assert.Equal("none", interactions.Logged[0].Model);
    }

    [Fact]
    public async Task AdviseAsync_SetsMetadataModelFromSelectedModel()
    {
        var chat = new FakeChatClient();
        var pipeline = BuildPipeline(new FakeFactRepository(StrongFacts(10)), chat, new FakeAdvisoryInteractionRepository());

        var response = await pipeline.AdviseAsync(new AdvisoryRequest { Query = "q", Domain = Domain });

        Assert.Equal(chat.LastOptions?.ModelId, response.Metadata.Model);
        Assert.Equal(AdvisoryClientKeys.Anthropic, response.Metadata.ModelKey);
    }

    [Fact]
    public async Task AdviseAsync_LogsAuditRecordWithCitedFactsAndConfidences()
    {
        var facts = StrongFacts(3);
        var interactions = new FakeAdvisoryInteractionRepository();
        var pipeline = BuildPipeline(new FakeFactRepository(facts), new FakeChatClient(), interactions);

        var response = await pipeline.AdviseAsync(new AdvisoryRequest { Query = "q", Domain = Domain });

        var logged = Assert.Single(interactions.Logged);
        Assert.Equal(facts.Select(f => f.Id), logged.CitedFactIds);
        Assert.Equal(3, logged.FactConfidences.Count);
        Assert.Equal(response.InteractionId, logged.Id.ToString());
        Assert.Equal(response.Confidence, logged.ConfidenceBand);
    }

    [Fact]
    public async Task AdviseAsync_ModelOverride_RoutesToOverriddenModel()
    {
        var chat = new FakeChatClient();
        var pipeline = BuildPipeline(new FakeFactRepository(StrongFacts(5)), chat, new FakeAdvisoryInteractionRepository());
        var request = new AdvisoryRequest
        {
            Query = "q",
            Domain = Domain,
            Hints = new AdvisoryHints { ModelOverride = "claude-opus-4-8" }
        };

        var response = await pipeline.AdviseAsync(request);

        Assert.Equal("claude-opus-4-8", response.Metadata.Model);
    }

    // ---- helpers ----

    private static List<FactSearchResult> StrongFacts(int count) =>
        Enumerable.Range(0, count).Select(i => new FactSearchResult
        {
            Id = Guid.NewGuid(),
            Content = $"fact {i}",
            Domain = Domain,
            Similarity = 0.9f,
            Confidence = 0.9f,
            SourceCredibility = 0.9f,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        }).ToList();

    private static AdvisoryPipeline BuildPipeline(
        IFactRepository facts, IChatClient chat, IAdvisoryInteractionRepository interactions)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton(AdvisoryClientKeys.Anthropic, chat);
        services.AddKeyedSingleton(AdvisoryClientKeys.Ollama, chat);
        var provider = services.BuildServiceProvider();

        var config = Options.Create(new AdvisoryConfig());
        return new AdvisoryPipeline(
            new FakeEmbeddingService(),
            facts,
            new TrustScoringService(),
            [new DefaultAdvisoryAdapter()],
            new LlmSelectionPolicy(config),
            provider,
            interactions,
            config,
            NullLogger<AdvisoryPipeline>.Instance);
    }

    private sealed class FakeChatClient : IChatClient
    {
        public string ResponseText { get; set; } = "answer";
        public bool ThrowIfCalled { get; set; }
        public ChatOptions? LastOptions { get; private set; }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (ThrowIfCalled)
                throw new InvalidOperationException("Model should not have been called.");

            LastOptions = options;
            var reply = new ChatMessage(ChatRole.Assistant, ResponseText);
            return Task.FromResult(new ChatResponse(reply) { ModelId = options?.ModelId });
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    private sealed class FakeAdvisoryInteractionRepository : IAdvisoryInteractionRepository
    {
        public List<AdvisoryInteraction> Logged { get; } = [];

        public Task<Guid> AddAsync(AdvisoryInteraction interaction, CancellationToken ct = default)
        {
            Logged.Add(interaction);
            return Task.FromResult(interaction.Id);
        }
    }

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public float[] GenerateEmbedding(string text) => new float[8];
        public float[][] GenerateEmbeddings(IEnumerable<string> texts, CancellationToken ct = default) => throw new NotImplementedException();
        public float CosineSimilarity(float[] a, float[] b) => throw new NotImplementedException();
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
    }
}
