using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Microsoft.Extensions.Options;

namespace Deke.Worker.Services;

/// <summary>
/// Quality pipeline (P1-2): flags facts with opposing evidence using a basic,
/// non-version-aware embedding-similarity heuristic (candidate band below L5's
/// dedup threshold). Version-aware resolution is deferred -- see OI-07 in
/// decisions.md.
/// </summary>
public class ContradictionDetectionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContradictionDetectionService> _logger;
    private readonly ContradictionDetectionConfig _config;

    public ContradictionDetectionService(
        IServiceProvider serviceProvider,
        ILogger<ContradictionDetectionService> logger,
        IOptions<ContradictionDetectionConfig> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ContradictionDetectionService started");
        var interval = TimeSpan.FromMinutes(_config.IntervalMinutes);

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
                _logger.LogError(ex, "Error in contradiction detection cycle");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    internal async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var relationRepo = scope.ServiceProvider.GetRequiredService<IFactRelationRepository>();
        var learningLogRepo = scope.ServiceProvider.GetRequiredService<ILearningLogRepository>();

        var pending = await factRepo.GetContradictionScanCandidatesAsync(_config.BatchSize, ct);
        if (pending.Count == 0)
            return;

        foreach (var domainGroup in pending.GroupBy(f => f.Domain))
        {
            var domain = domainGroup.Key;
            var log = new LearningLog
            {
                Domain = domain,
                CycleType = "contradiction-detection",
                StartedAt = DateTimeOffset.UtcNow
            };

            try
            {
                var flagged = 0;
                foreach (var fact in domainGroup)
                {
                    if (fact.Embedding is null)
                        continue;

                    var candidates = await factRepo.SearchAsync(
                        fact.Embedding, fact.Domain, limit: 5,
                        minSimilarity: _config.MinSimilarity, maxSimilarity: _config.MaxSimilarity, ct: ct);

                    foreach (var candidate in candidates.Where(c => c.Id != fact.Id))
                    {
                        var alreadyLinked = await relationRepo.ExistsAsync(fact.Id, candidate.Id, "contradicts", ct)
                            || await relationRepo.ExistsAsync(candidate.Id, fact.Id, "contradicts", ct);
                        if (alreadyLinked)
                            continue;

                        await relationRepo.AddAsync(new FactRelation
                        {
                            FromFactId = fact.Id,
                            ToFactId = candidate.Id,
                            RelationType = "contradicts"
                        }, ct);
                        await factRepo.MarkContradictedAsync(fact.Id, ct);
                        await factRepo.MarkContradictedAsync(candidate.Id, ct);
                        flagged++;
                    }
                }

                log.FactsUpdated = flagged;
                log.CompletedAt = DateTimeOffset.UtcNow;
                log.Notes = $"Scanned {domainGroup.Count()} facts, flagged {flagged} contradicting pairs";
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogError(ex, "Error detecting contradictions for domain {Domain}", domain);
            }

            await learningLogRepo.AddAsync(log, ct);
        }
    }
}
