using Deke.Core.Interfaces;
using Deke.Core.Models;
using Microsoft.Extensions.Options;

namespace Deke.Api.Endpoints;

public static class FederationEndpoints
{
    public static void MapFederationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/federation").WithTags("Federation");

        group.MapGet("/manifest", GetManifest)
            .WithName("GetManifest")
            .WithDescription("Get this instance's federation manifest")
            .AllowAnonymous();

        group.MapGet("/peers", GetPeers)
            .WithName("GetPeers")
            .WithDescription("List all known federation peers")
            .AllowAnonymous();

        group.MapPost("/peers", AddPeer)
            .WithName("AddPeer")
            .WithDescription("Manually register a federation peer")
            .RequireAuthorization();

        group.MapDelete("/peers/{id:guid}", DeletePeer)
            .WithName("DeletePeer")
            .WithDescription("Remove a federation peer")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetManifest(
        IOptions<FederationConfig> config,
        IFactRepository factRepo,
        CancellationToken ct)
    {
        var cfg = config.Value;
        var domainStats = await factRepo.GetDomainStatsAsync(ct);

        var manifest = new FederationManifest
        {
            InstanceId = cfg.InstanceId,
            Version = "1.0.0",
            ProtocolVersion = "1",
            Domains = domainStats.Select(d => new PeerDomainInfo
            {
                Name = d.Domain,
                FactCount = d.FactCount,
                LastUpdatedAt = d.LastUpdatedAt,
                Confidence = 1.0f
            }).ToList(),
            Capabilities = ["search"],
            RegisteredAt = DateTimeOffset.UtcNow
        };

        return Results.Ok(manifest);
    }

    private static async Task<IResult> GetPeers(
        IFederationPeerRepository peerRepo,
        CancellationToken ct)
    {
        var peers = await peerRepo.GetAllAsync(ct);
        return Results.Ok(peers);
    }

    private static async Task<IResult> AddPeer(
        AddPeerRequest request,
        IFederationPeerRepository peerRepo,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.InstanceId))
            return Results.BadRequest(new { error = "InstanceId is required." });
        if (string.IsNullOrWhiteSpace(request.BaseUrl))
            return Results.BadRequest(new { error = "BaseUrl is required." });

        var peer = new FederationPeer
        {
            InstanceId = request.InstanceId,
            BaseUrl = request.BaseUrl
        };

        var id = await peerRepo.AddAsync(peer, ct);
        return Results.Created($"/api/federation/peers/{id}", new { id });
    }

    private static async Task<IResult> DeletePeer(
        Guid id,
        IFederationPeerRepository peerRepo,
        CancellationToken ct)
    {
        await peerRepo.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}

public record AddPeerRequest(string InstanceId, string BaseUrl);
