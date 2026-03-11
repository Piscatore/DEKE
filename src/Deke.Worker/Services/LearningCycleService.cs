using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Worker.Services;

public class LearningCycleService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LearningCycleService> _logger;
    private static readonly TimeSpan CycleInterval = TimeSpan.FromHours(2);

    public LearningCycleService(IServiceProvider serviceProvider, ILogger<LearningCycleService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LearningCycleService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MapRelationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in learning cycle");
            }

            await Task.Delay(CycleInterval, stoppingToken);
        }
    }

    private async Task MapRelationsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var factRelationRepo = scope.ServiceProvider.GetRequiredService<IFactRelationRepository>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var learningLogRepo = scope.ServiceProvider.GetRequiredService<ILearningLogRepository>();
        var sourceRepo = scope.ServiceProvider.GetRequiredService<ISourceRepository>();

        var sources = await sourceRepo.GetActiveAsync(ct);
        var domains = sources.Select(s => s.Domain).Distinct().ToList();

        foreach (var domain in domains)
        {
            var log = new LearningLog
            {
                Domain = domain,
                CycleType = "RelationMapping",
                StartedAt = DateTimeOffset.UtcNow
            };

            try
            {
                var factsWithoutRelations = await factRepo.GetWithoutRelationsAsync(domain, limit: 50, ct: ct);
                var relationsAdded = 0;

                foreach (var fact in factsWithoutRelations)
                {
                    if (fact.Embedding is not { Length: > 0 })
                    {
                        continue;
                    }

                    var similarFacts = await factRepo.SearchAsync(
                        fact.Embedding,
                        domain,
                        limit: 10,
                        minSimilarity: 0.7f,
                        ct: ct);

                    foreach (var similar in similarFacts)
                    {
                        if (similar.Id == fact.Id)
                        {
                            continue;
                        }

                        if (similar.Similarity <= 0.7f)
                        {
                            continue;
                        }

                        var exists = await factRelationRepo.ExistsAsync(fact.Id, similar.Id, "related", ct);
                        if (exists)
                        {
                            continue;
                        }

                        // Also check reverse direction
                        var reverseExists = await factRelationRepo.ExistsAsync(similar.Id, fact.Id, "related", ct);
                        if (reverseExists)
                        {
                            continue;
                        }

                        var relation = new FactRelation
                        {
                            FromFactId = fact.Id,
                            ToFactId = similar.Id,
                            RelationType = "related",
                            Confidence = similar.Similarity
                        };

                        await factRelationRepo.AddAsync(relation, ct);
                        relationsAdded++;
                    }
                }

                log.RelationsAdded = relationsAdded;
                log.CompletedAt = DateTimeOffset.UtcNow;
                log.Notes = $"Processed {factsWithoutRelations.Count} facts, added {relationsAdded} relations";
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogError(ex, "Error mapping relations for domain {Domain}", domain);
            }

            await learningLogRepo.AddAsync(log, ct);
        }
    }
}
