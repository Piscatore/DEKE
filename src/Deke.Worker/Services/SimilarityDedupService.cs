using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Microsoft.Extensions.Options;

namespace Deke.Worker.Services;

/// <summary>
/// Deduplication level 4 (async): stamps a SimHash on facts that lack one and
/// links near-duplicates (Hamming distance within the configured threshold) to
/// their canonical fact.
/// </summary>
public class SimilarityDedupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SimilarityDedupService> _logger;
    private readonly DedupConfig _config;

    public SimilarityDedupService(
        IServiceProvider serviceProvider,
        ILogger<SimilarityDedupService> logger,
        IOptions<DedupConfig> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SimilarityDedupService started");
        var interval = TimeSpan.FromMinutes(_config.SimilarityIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in similarity dedup cycle");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var simHasher = scope.ServiceProvider.GetRequiredService<ISimHasher>();
        var linker = scope.ServiceProvider.GetRequiredService<IDuplicateLinker>();
        var learningLogRepo = scope.ServiceProvider.GetRequiredService<ILearningLogRepository>();

        var pending = await factRepo.GetPendingSimilarityAsync(_config.BatchSize, ct);
        if (pending.Count == 0)
            return;

        foreach (var domainGroup in pending.GroupBy(f => f.Domain))
        {
            var domain = domainGroup.Key;
            var log = new LearningLog
            {
                Domain = domain,
                CycleType = "similarity-dedup",
                StartedAt = DateTimeOffset.UtcNow
            };

            try
            {
                // Already-hashed, non-duplicate facts in this domain are the corpus
                // each pending fact is compared against.
                var corpus = (await factRepo.GetByDomainAsync(domain, 1000, ct))
                    .Where(f => f.SimilarityHash is not null && f.DuplicateOf is null)
                    .ToList();

                var duplicates = 0;
                foreach (var fact in domainGroup)
                {
                    var hash = simHasher.Compute(fact.Content);

                    var canonical = corpus.FirstOrDefault(c =>
                        c.Id != fact.Id &&
                        simHasher.HammingDistance(hash, c.SimilarityHash!.Value) <= _config.HammingThreshold);

                    await factRepo.SetSimilarityHashAsync(fact.Id, hash, ct);
                    fact.SimilarityHash = hash;

                    if (canonical is not null)
                    {
                        await factRepo.SetDuplicateOfAsync(fact.Id, canonical.Id, ct);
                        await linker.CorroborateAsync(canonical.Id, fact.SourceId, fact.Confidence, ct);
                        duplicates++;
                    }
                    else
                    {
                        // Unique so far: joins the corpus for later facts in this batch.
                        corpus.Add(fact);
                    }
                }

                log.FactsUpdated = duplicates;
                log.CompletedAt = DateTimeOffset.UtcNow;
                log.Notes = $"Hashed {domainGroup.Count()} facts, linked {duplicates} near-duplicates";
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogError(ex, "Error in similarity dedup for domain {Domain}", domain);
            }

            await learningLogRepo.AddAsync(log, ct);
        }
    }
}
