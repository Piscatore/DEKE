using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Microsoft.Extensions.Options;

namespace Deke.Worker.Services;

/// <summary>
/// Deduplication level 5 (async): links facts whose embedding is within the
/// configured cosine threshold of an earlier fact to that canonical fact.
/// </summary>
public class SemanticDedupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SemanticDedupService> _logger;
    private readonly DedupConfig _config;

    public SemanticDedupService(
        IServiceProvider serviceProvider,
        ILogger<SemanticDedupService> logger,
        IOptions<DedupConfig> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SemanticDedupService started");
        var interval = TimeSpan.FromMinutes(_config.SemanticIntervalMinutes);

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
                _logger.LogError(ex, "Error in semantic dedup cycle");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    internal async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var linker = scope.ServiceProvider.GetRequiredService<IDuplicateLinker>();
        var learningLogRepo = scope.ServiceProvider.GetRequiredService<ILearningLogRepository>();

        var pending = await factRepo.GetPendingSemanticAsync(_config.BatchSize, ct);
        if (pending.Count == 0)
            return;

        foreach (var domainGroup in pending.GroupBy(f => f.Domain))
        {
            var domain = domainGroup.Key;
            var log = new LearningLog
            {
                Domain = domain,
                CycleType = "semantic-dedup",
                StartedAt = DateTimeOffset.UtcNow
            };

            try
            {
                var duplicates = 0;
                foreach (var fact in domainGroup)
                {
                    if (fact.Embedding is not { Length: > 0 })
                        continue;

                    var matches = await factRepo.SearchAsync(
                        fact.Embedding, domain, limit: 5, minSimilarity: _config.SemanticThreshold, ct: ct);

                    // Canonical = most-similar strictly-older fact, so a pair collapses
                    // onto its earlier member only (never marks both as duplicates).
                    var canonical = matches
                        .Where(m => m.Id != fact.Id && m.CreatedAt < fact.CreatedAt)
                        .OrderByDescending(m => m.Similarity)
                        .FirstOrDefault();

                    if (canonical is not null)
                    {
                        await factRepo.SetDuplicateOfAsync(fact.Id, canonical.Id, ct);
                        await linker.CorroborateAsync(canonical.Id, fact.SourceId, fact.Confidence, ct);
                        duplicates++;
                    }
                }

                log.FactsUpdated = duplicates;
                log.CompletedAt = DateTimeOffset.UtcNow;
                log.Notes = $"Checked {domainGroup.Count()} facts, linked {duplicates} semantic duplicates";
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogError(ex, "Error in semantic dedup for domain {Domain}", domain);
            }

            await learningLogRepo.AddAsync(log, ct);
        }
    }
}
