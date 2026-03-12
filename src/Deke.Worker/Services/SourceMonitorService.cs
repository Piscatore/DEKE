using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Worker.Services;

public class SourceMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SourceMonitorService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    public SourceMonitorService(IServiceProvider serviceProvider, ILogger<SourceMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SourceMonitorService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckSourcesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in source monitoring cycle");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckSourcesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var sourceRepo = scope.ServiceProvider.GetRequiredService<ISourceRepository>();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var harvesters = scope.ServiceProvider.GetRequiredService<IEnumerable<IHarvester>>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var extractionService = scope.ServiceProvider.GetRequiredService<IExtractionService>();

        var harvesterMap = harvesters.ToDictionary(h => h.SupportedType);
        var dueSources = await sourceRepo.GetDueForCheckAsync(ct);

        _logger.LogInformation("Found {Count} sources due for check", dueSources.Count);

        foreach (var source in dueSources)
        {
            try
            {
                if (!harvesterMap.TryGetValue(source.Type, out var harvester))
                {
                    _logger.LogWarning("No harvester found for source type {Type}", source.Type);
                    continue;
                }

                var result = await harvester.HarvestAsync(source, ct);

                source.LastCheckedAt = DateTimeOffset.UtcNow;

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    _logger.LogWarning("Harvest error for source {Url}: {Error}", source.Url, result.ErrorMessage);
                    await sourceRepo.UpdateAsync(source, ct);
                    continue;
                }

                if (result.HasChanges)
                {
                    source.LastChangedAt = DateTimeOffset.UtcNow;
                    source.ContentHash = result.NewContentHash;

                    var factsAdded = 0;
                    foreach (var text in result.ExtractedTexts)
                    {
                        var extractedFacts = await extractionService.ExtractFactsAsync(text, source.Domain, source.Url, ct);

                        foreach (var extracted in extractedFacts)
                        {
                            var embedding = embeddingService.GenerateEmbedding(extracted.Content);
                            var fact = new Fact
                            {
                                Content = extracted.Content,
                                Domain = source.Domain,
                                Embedding = embedding,
                                Confidence = extracted.Confidence * source.Credibility,
                                SourceId = source.Id,
                                Entities = extracted.Entities
                            };

                            await factRepo.AddAsync(fact, ct);
                            factsAdded++;
                        }
                    }

                    _logger.LogInformation("Source {Url}: added {Count} facts", source.Url, factsAdded);
                }

                await sourceRepo.UpdateAsync(source, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing source {Url}", source.Url);
            }
        }
    }
}
