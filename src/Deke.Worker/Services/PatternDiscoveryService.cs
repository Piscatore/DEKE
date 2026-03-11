using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Worker.Services;

public class PatternDiscoveryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PatternDiscoveryService> _logger;
    private static readonly TimeSpan CycleInterval = TimeSpan.FromHours(1);

    public PatternDiscoveryService(IServiceProvider serviceProvider, ILogger<PatternDiscoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PatternDiscoveryService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DiscoverPatternsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in pattern discovery cycle");
            }

            await Task.Delay(CycleInterval, stoppingToken);
        }
    }

    private async Task DiscoverPatternsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var patternRepo = scope.ServiceProvider.GetRequiredService<IPatternRepository>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var llmService = scope.ServiceProvider.GetRequiredService<ILlmService>();
        var learningLogRepo = scope.ServiceProvider.GetRequiredService<ILearningLogRepository>();
        var sourceRepo = scope.ServiceProvider.GetRequiredService<ISourceRepository>();

        // Get all active sources to discover which domains to process
        var sources = await sourceRepo.GetActiveAsync(ct);
        var domains = sources.Select(s => s.Domain).Distinct().ToList();

        foreach (var domain in domains)
        {
            var log = new LearningLog
            {
                Domain = domain,
                CycleType = "PatternDiscovery",
                StartedAt = DateTimeOffset.UtcNow
            };

            try
            {
                var recentFacts = await factRepo.GetRecentAsync(domain, days: 7, limit: 100, ct: ct);

                if (recentFacts.Count < 3)
                {
                    continue;
                }

                // Build similarity clusters using union-find approach
                var clusters = BuildClusters(recentFacts, embeddingService);

                var patternsDiscovered = 0;
                foreach (var cluster in clusters.Where(c => c.Count >= 3))
                {
                    var existingPatterns = await patternRepo.GetActiveByDomainAsync(domain, ct);
                    var clusterIds = cluster.Select(f => f.Id).ToHashSet();

                    // Check if a pattern already covers these facts
                    var alreadyExists = existingPatterns.Any(p =>
                        p.EvidenceFactIds.Count > 0 &&
                        p.EvidenceFactIds.All(id => clusterIds.Contains(id)));

                    if (alreadyExists)
                    {
                        continue;
                    }

                    var firstContent = cluster[0].Content;
                    var truncated = firstContent.Length > 100 ? firstContent[..100] + "..." : firstContent;
                    var description = $"Cluster of {cluster.Count} related facts about: {truncated}";

                    if (llmService.IsAvailable)
                    {
                        var prompt = $"Summarize the following related facts into a single pattern description:\n\n" +
                            string.Join("\n", cluster.Select(f => $"- {f.Content}"));
                        var generated = await llmService.GenerateAsync(prompt, ct);
                        if (!string.IsNullOrWhiteSpace(generated))
                        {
                            description = generated;
                        }
                    }

                    var avgSimilarity = ComputeAverageClusterSimilarity(cluster, embeddingService);

                    var pattern = new Pattern
                    {
                        Description = description,
                        Domain = domain,
                        PatternType = PatternType.Observation,
                        EvidenceFactIds = cluster.Select(f => f.Id).ToList(),
                        Confidence = avgSimilarity,
                        OccurrenceCount = cluster.Count
                    };

                    await patternRepo.AddAsync(pattern, ct);
                    patternsDiscovered++;
                }

                log.PatternsDiscovered = patternsDiscovered;
                log.CompletedAt = DateTimeOffset.UtcNow;
                log.Notes = $"Processed {recentFacts.Count} facts, discovered {patternsDiscovered} patterns";
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogError(ex, "Error discovering patterns for domain {Domain}", domain);
            }

            await learningLogRepo.AddAsync(log, ct);
        }
    }

    private static List<List<Fact>> BuildClusters(List<Fact> facts, IEmbeddingService embeddingService)
    {
        var factsWithEmbeddings = facts.Where(f => f.Embedding is { Length: > 0 }).ToList();
        var assigned = new HashSet<int>();
        var clusters = new List<List<Fact>>();

        for (var i = 0; i < factsWithEmbeddings.Count; i++)
        {
            if (assigned.Contains(i))
            {
                continue;
            }

            var cluster = new List<Fact> { factsWithEmbeddings[i] };
            assigned.Add(i);

            for (var j = i + 1; j < factsWithEmbeddings.Count; j++)
            {
                if (assigned.Contains(j))
                {
                    continue;
                }

                var similarity = embeddingService.CosineSimilarity(
                    factsWithEmbeddings[i].Embedding!,
                    factsWithEmbeddings[j].Embedding!);

                if (similarity > 0.8f)
                {
                    cluster.Add(factsWithEmbeddings[j]);
                    assigned.Add(j);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private static float ComputeAverageClusterSimilarity(List<Fact> cluster, IEmbeddingService embeddingService)
    {
        if (cluster.Count < 2)
        {
            return 1.0f;
        }

        var totalSimilarity = 0f;
        var pairCount = 0;

        for (var i = 0; i < cluster.Count; i++)
        {
            for (var j = i + 1; j < cluster.Count; j++)
            {
                if (cluster[i].Embedding is { Length: > 0 } && cluster[j].Embedding is { Length: > 0 })
                {
                    totalSimilarity += embeddingService.CosineSimilarity(cluster[i].Embedding!, cluster[j].Embedding!);
                    pairCount++;
                }
            }
        }

        return pairCount > 0 ? totalSimilarity / pairCount : 0f;
    }
}
