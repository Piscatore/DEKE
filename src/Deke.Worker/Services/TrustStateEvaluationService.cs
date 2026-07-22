using Deke.Core.Interfaces;
using Deke.Core.Models;
using Deke.Infrastructure.Ingestion;
using Microsoft.Extensions.Options;

namespace Deke.Worker.Services;

/// <summary>
/// Quality pipeline (P1-2): classifies Unscored facts into Accepted or Flagged
/// per the fact's domain trust policy. Domains without a configured policy
/// fail open (Accepted) -- see ADR discussion in decisions.md.
/// </summary>
public class TrustStateEvaluationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrustStateEvaluationService> _logger;
    private readonly TrustEvaluationConfig _config;

    public TrustStateEvaluationService(
        IServiceProvider serviceProvider,
        ILogger<TrustStateEvaluationService> logger,
        IOptions<TrustEvaluationConfig> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrustStateEvaluationService started");
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
                _logger.LogError(ex, "Error in trust evaluation cycle");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    internal async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var factRepo = scope.ServiceProvider.GetRequiredService<IFactRepository>();
        var sourceRepo = scope.ServiceProvider.GetRequiredService<ISourceRepository>();
        var policyRepo = scope.ServiceProvider.GetRequiredService<IDomainTrustPolicyRepository>();
        var evaluator = scope.ServiceProvider.GetRequiredService<ITrustEvaluator>();
        var learningLogRepo = scope.ServiceProvider.GetRequiredService<ILearningLogRepository>();

        var pending = await factRepo.GetPendingTrustEvaluationAsync(_config.BatchSize, ct);
        if (pending.Count == 0)
            return;

        foreach (var domainGroup in pending.GroupBy(f => f.Domain))
        {
            var domain = domainGroup.Key;
            var log = new LearningLog
            {
                Domain = domain,
                CycleType = "trust-evaluation",
                StartedAt = DateTimeOffset.UtcNow
            };

            try
            {
                var policy = await policyRepo.GetByDomainAsync(domain, ct);

                var accepted = 0;
                var flagged = 0;
                foreach (var fact in domainGroup)
                {
                    SourceTier? tier = null;
                    if (fact.SourceId is Guid sourceId)
                    {
                        var source = await sourceRepo.GetByIdAsync(sourceId, ct);
                        tier = source?.SourceTier;
                    }

                    var newState = evaluator.Evaluate(fact, tier, policy);
                    await factRepo.SetTrustStateAsync(fact.Id, newState, ct);

                    if (newState == TrustState.Accepted)
                        accepted++;
                    else
                        flagged++;
                }

                log.FactsUpdated = accepted + flagged;
                log.CompletedAt = DateTimeOffset.UtcNow;
                log.Notes = $"Evaluated {domainGroup.Count()} facts: {accepted} accepted, {flagged} flagged";
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
                _logger.LogError(ex, "Error evaluating trust state for domain {Domain}", domain);
            }

            await learningLogRepo.AddAsync(log, ct);
        }
    }
}
