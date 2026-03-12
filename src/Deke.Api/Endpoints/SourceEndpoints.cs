using System.Net;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class SourceEndpoints
{
    private static readonly string[] AllowedSchemes = ["http", "https"];

    private static bool IsValidPublicUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (!AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
            return false;

        if (IPAddress.TryParse(uri.Host, out var ip))
        {
            if (IPAddress.IsLoopback(ip) || ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal)
                return false;

            // Reject private IPv4 ranges
            var bytes = ip.GetAddressBytes();
            if (bytes.Length == 4)
            {
                if (bytes[0] == 10) return false;
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
                if (bytes[0] == 192 && bytes[1] == 168) return false;
                if (bytes[0] == 169 && bytes[1] == 254) return false;
            }
        }
        else if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public static void MapSourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sources").WithTags("Sources");

        group.MapGet("/", GetSources)
            .WithName("GetSources")
            .WithDescription("Get all sources, optionally filtered by domain")
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetSource)
            .WithName("GetSource")
            .WithDescription("Get a source by ID")
            .AllowAnonymous();

        group.MapPost("/", AddSource)
            .WithName("AddSource")
            .WithDescription("Add a new source to monitor")
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteSource)
            .WithName("DeleteSource")
            .WithDescription("Delete a source")
            .RequireAuthorization();

    }

    private static async Task<IResult> GetSources(
        string? domain,
        ISourceRepository sourceRepo,
        CancellationToken ct)
    {
        var sources = domain is not null
            ? await sourceRepo.GetByDomainAsync(domain, ct)
            : await sourceRepo.GetAllAsync(ct);

        return Results.Ok(sources);
    }

    private static async Task<IResult> GetSource(
        Guid id,
        ISourceRepository sourceRepo,
        CancellationToken ct)
    {
        var source = await sourceRepo.GetByIdAsync(id, ct);
        return source is null ? Results.NotFound() : Results.Ok(source);
    }

    private static async Task<IResult> AddSource(
        AddSourceRequest request,
        ISourceRepository sourceRepo,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return Results.BadRequest(new { error = "URL is required." });
        if (string.IsNullOrWhiteSpace(request.Domain))
            return Results.BadRequest(new { error = "Domain is required." });
        if (!IsValidPublicUrl(request.Url))
            return Results.BadRequest(new { error = "URL must be a valid public HTTP(S) URL. Private/loopback addresses are not allowed." });

        var source = new Source
        {
            Url = request.Url,
            Domain = request.Domain,
            Name = request.Name,
            Type = request.Type ?? SourceType.WebPage,
            CheckInterval = request.CheckInterval ?? TimeSpan.FromDays(1),
            Credibility = request.Credibility ?? 0.5f
        };

        var id = await sourceRepo.AddAsync(source, ct);
        return Results.Created($"/api/sources/{id}", new { id });
    }

    private static async Task<IResult> DeleteSource(
        Guid id,
        ISourceRepository sourceRepo,
        CancellationToken ct)
    {
        await sourceRepo.DeleteAsync(id, ct);
        return Results.NoContent();
    }

}

public record AddSourceRequest(
    string Url,
    string Domain,
    string? Name = null,
    SourceType? Type = null,
    TimeSpan? CheckInterval = null,
    float? Credibility = null);
