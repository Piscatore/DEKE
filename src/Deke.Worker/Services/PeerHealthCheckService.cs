using System.Net.Http.Json;
using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.Extensions.Options;

namespace Deke.Worker.Services;

public class PeerHealthCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<FederationConfig> _config;
    private readonly ILogger<PeerHealthCheckService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public PeerHealthCheckService(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<FederationConfig> config,
        ILogger<PeerHealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PeerHealthCheckService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_config.CurrentValue.Enabled)
                {
                    await CheckPeersAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in peer health check cycle");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckPeersAsync(CancellationToken ct)
    {
        var peers = _config.CurrentValue.Peers;
        if (peers.Count == 0)
        {
            _logger.LogDebug("No federation peers configured");
            return;
        }

        _logger.LogInformation("Checking {Count} federation peers", peers.Count);

        using var scope = _serviceProvider.CreateScope();
        var peerRepo = scope.ServiceProvider.GetRequiredService<IFederationPeerRepository>();
        var client = _httpClientFactory.CreateClient("federation");

        foreach (var peerConfig in peers)
        {
            try
            {
                var manifest = await client.GetFromJsonAsync<FederationManifest>(
                    $"{peerConfig.BaseUrl.TrimEnd('/')}/api/federation/manifest", ct);

                if (manifest is null)
                {
                    _logger.LogWarning("Peer {InstanceId} returned null manifest", peerConfig.InstanceId);
                    await MarkPeerUnhealthy(peerRepo, peerConfig, ct);
                    continue;
                }

                var peer = new FederationPeer
                {
                    InstanceId = peerConfig.InstanceId,
                    BaseUrl = peerConfig.BaseUrl,
                    Domains = manifest.Domains,
                    Capabilities = manifest.Capabilities,
                    ProtocolVersion = manifest.ProtocolVersion,
                    LastSeenAt = DateTimeOffset.UtcNow,
                    IsHealthy = true
                };

                await peerRepo.UpsertAsync(peer, ct);
                _logger.LogInformation("Peer {InstanceId}: healthy, {DomainCount} domains",
                    peerConfig.InstanceId, manifest.Domains.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Peer {InstanceId} unreachable", peerConfig.InstanceId);
                await MarkPeerUnhealthy(peerRepo, peerConfig, ct);
            }
        }
    }

    private static async Task MarkPeerUnhealthy(
        IFederationPeerRepository peerRepo,
        PeerConfigEntry peerConfig,
        CancellationToken ct)
    {
        var peer = new FederationPeer
        {
            InstanceId = peerConfig.InstanceId,
            BaseUrl = peerConfig.BaseUrl,
            IsHealthy = false,
            LastSeenAt = DateTimeOffset.UtcNow
        };

        await peerRepo.UpsertAsync(peer, ct);
    }
}
