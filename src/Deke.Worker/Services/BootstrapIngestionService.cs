using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Worker.Services;

public class BootstrapIngestionService
{
    private const string Domain = "software-product";
    private const float ElevatedConfidence = 0.95f;
    private static readonly string[] BootstrapPaths = ["docs", "thoughts"];

    private readonly ISourceRepository _sourceRepo;
    private readonly IFactRepository _factRepo;
    private readonly IEnumerable<IHarvester> _harvesters;
    private readonly IChunker _chunker;
    private readonly IExtractionService _extractionService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<BootstrapIngestionService> _logger;

    public BootstrapIngestionService(
        ISourceRepository sourceRepo,
        IFactRepository factRepo,
        IEnumerable<IHarvester> harvesters,
        IChunker chunker,
        IExtractionService extractionService,
        IEmbeddingService embeddingService,
        ILogger<BootstrapIngestionService> logger)
    {
        _sourceRepo = sourceRepo;
        _factRepo = factRepo;
        _harvesters = harvesters;
        _chunker = chunker;
        _extractionService = extractionService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task RunAsync(string repoRoot, CancellationToken ct = default)
    {
        var harvester = _harvesters.FirstOrDefault(h => h.SupportedType == SourceType.File)
            ?? throw new InvalidOperationException("No harvester registered for SourceType.File");

        foreach (var relativePath in BootstrapPaths)
        {
            var path = Path.Combine(repoRoot, relativePath);

            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Bootstrap path not found: {Path}", path);
                continue;
            }

            await IngestPathAsync(harvester, path, relativePath, ct);
        }
    }

    private async Task IngestPathAsync(IHarvester harvester, string path, string label, CancellationToken ct)
    {
        var source = await FindOrCreateSourceAsync(path, ct);

        var result = await harvester.HarvestAsync(source, ct);

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            _logger.LogWarning("Bootstrap harvest error for {Path}: {Error}", path, result.ErrorMessage);
            return;
        }

        if (!result.HasChanges)
        {
            _logger.LogInformation("Bootstrap source {Path} unchanged, skipping", path);
            return;
        }

        source.LastCheckedAt = DateTimeOffset.UtcNow;
        source.LastChangedAt = DateTimeOffset.UtcNow;
        source.ContentHash = result.NewContentHash;

        var factsAdded = 0;
        foreach (var text in result.ExtractedTexts)
        {
            var chunks = await _chunker.ChunkAsync(text, ct);

            foreach (var chunk in chunks)
            {
                var extractedFacts = await _extractionService.ExtractFactsAsync(chunk, Domain, path, ct);

                foreach (var extracted in extractedFacts)
                {
                    var embedding = _embeddingService.GenerateEmbedding(extracted.Content);
                    var fact = new Fact
                    {
                        Content = extracted.Content,
                        Domain = Domain,
                        Embedding = embedding,
                        Confidence = ElevatedConfidence,
                        SourceId = source.Id,
                        Entities = extracted.Entities
                    };

                    await _factRepo.AddAsync(fact, ct);
                    factsAdded++;
                }
            }
        }

        await _sourceRepo.UpdateAsync(source, ct);
        _logger.LogInformation("Bootstrap {Label}: added {Count} facts", label, factsAdded);
    }

    private async Task<Source> FindOrCreateSourceAsync(string path, CancellationToken ct)
    {
        var existing = await _sourceRepo.GetByUrlAsync(path, ct);
        if (existing is not null)
            return existing;

        var source = new Source
        {
            Url = path,
            Domain = Domain,
            Name = $"DEKE bootstrap: {Path.GetFileName(path)}",
            Type = SourceType.File,
            Credibility = 1.0f
        };

        await _sourceRepo.AddAsync(source, ct);
        return source;
    }
}
