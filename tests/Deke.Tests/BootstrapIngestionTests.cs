using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Extraction;
using Deke.Infrastructure.Harvesters;
using Deke.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deke.Tests;

public class BootstrapIngestionTests : IDisposable
{
    private readonly string _repoRoot;

    public BootstrapIngestionTests()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), "deke-bootstrap-test-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(_repoRoot, "docs"));
        Directory.CreateDirectory(Path.Combine(_repoRoot, "thoughts"));
        File.WriteAllText(Path.Combine(_repoRoot, "docs", "overview.md"), "DEKE is a domain expert knowledge engine.");
        File.WriteAllText(Path.Combine(_repoRoot, "thoughts", "notes.md"), "Some scratch notes about the roadmap.");
    }

    public void Dispose()
    {
        if (Directory.Exists(_repoRoot))
            Directory.Delete(_repoRoot, recursive: true);
    }

    [Fact]
    public async Task RunAsync_IngestsDocsAndThoughts_IntoSoftwareProductDomain()
    {
        var factRepo = new FakeFactRepository();
        var service = BuildService(factRepo, new FakeSourceRepository());

        await service.RunAsync(_repoRoot);

        Assert.NotEmpty(factRepo.Added);
        Assert.All(factRepo.Added, f => Assert.Equal("software-product", f.Domain));
        Assert.All(factRepo.Added, f => Assert.True(f.Confidence > 0.9f));
    }

    [Fact]
    public async Task RunAsync_CalledTwiceWithNoChanges_DoesNotDuplicateFacts()
    {
        var factRepo = new FakeFactRepository();
        var sourceRepo = new FakeSourceRepository();
        var service = BuildService(factRepo, sourceRepo);

        await service.RunAsync(_repoRoot);
        var firstRunCount = factRepo.Added.Count;

        await service.RunAsync(_repoRoot);

        Assert.Equal(firstRunCount, factRepo.Added.Count);
    }

    private static BootstrapIngestionService BuildService(IFactRepository factRepo, ISourceRepository sourceRepo)
    {
        IHarvester[] harvesters = [new FileSystemHarvester()];

        return new BootstrapIngestionService(
            sourceRepo,
            factRepo,
            harvesters,
            new IdentityChunker(),
            new SimpleExtractionService(),
            new FakeEmbeddingService(),
            NullLogger<BootstrapIngestionService>.Instance);
    }

    private sealed class IdentityChunker : IChunker
    {
        public Task<IReadOnlyList<string>> ChunkAsync(string text, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<string>>([text]);
    }

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public float[] GenerateEmbedding(string text) => new float[8];
        public float[][] GenerateEmbeddings(IEnumerable<string> texts, CancellationToken ct = default) => throw new NotImplementedException();
        public float CosineSimilarity(float[] a, float[] b) => throw new NotImplementedException();
    }

    private sealed class FakeSourceRepository : ISourceRepository
    {
        private readonly Dictionary<Guid, Source> _byId = [];

        public Task<Source?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_byId.GetValueOrDefault(id));

        public Task<Source?> GetByUrlAsync(string url, CancellationToken ct = default)
            => Task.FromResult(_byId.Values.FirstOrDefault(s => s.Url == url));

        public Task<Guid> AddAsync(Source source, CancellationToken ct = default)
        {
            _byId[source.Id] = source;
            return Task.FromResult(source.Id);
        }

        public Task UpdateAsync(Source source, CancellationToken ct = default)
        {
            _byId[source.Id] = source;
            return Task.CompletedTask;
        }

        public Task<List<Source>> GetByDomainAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetActiveAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetDueForCheckAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Source>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeactivateAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeFactRepository : IFactRepository
    {
        public List<Fact> Added { get; } = [];

        public Task<Guid> AddAsync(Fact fact, CancellationToken ct = default)
        {
            Added.Add(fact);
            return Task.FromResult(fact.Id);
        }

        public Task<Fact?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetByDomainAsync(string domain, int limit = 100, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetBySourceAsync(Guid sourceId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<FactSearchResult>> SearchAsync(float[] embedding, string? domain, int limit = 10, float minSimilarity = 0.5f, CancellationToken ct = default) => throw new NotImplementedException();
        public Task UpdateAsync(Fact fact, CancellationToken ct = default) => throw new NotImplementedException();
        public Task MarkOutdatedAsync(Guid id, string reason, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(string domain, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetRecentAsync(string domain, int days, int limit = 100, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<Fact>> GetWithoutRelationsAsync(string domain, int limit = 50, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<List<DomainStats>> GetDomainStatsAsync(CancellationToken ct = default) => throw new NotImplementedException();
    }
}
