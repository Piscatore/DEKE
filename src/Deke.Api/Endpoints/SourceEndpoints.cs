using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class SourceEndpoints
{
    public static void MapSourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sources").WithTags("Sources");

        group.MapGet("/", GetSources)
            .WithName("GetSources")
            .WithDescription("Get all sources, optionally filtered by domain");

        group.MapGet("/{id:guid}", GetSource)
            .WithName("GetSource")
            .WithDescription("Get a source by ID");

        group.MapPost("/", AddSource)
            .WithName("AddSource")
            .WithDescription("Add a new source to monitor");

        group.MapDelete("/{id:guid}", DeleteSource)
            .WithName("DeleteSource")
            .WithDescription("Delete a source");

        group.MapPost("/{id:guid}/check", TriggerCheck)
            .WithName("TriggerCheck")
            .WithDescription("Trigger an immediate check for a source");
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

    private static async Task<IResult> TriggerCheck(
        Guid id,
        ISourceRepository sourceRepo,
        CancellationToken ct)
    {
        var source = await sourceRepo.GetByIdAsync(id, ct);
        if (source is null) return Results.NotFound();

        // TODO: Queue actual check via background worker
        return Results.Accepted(value: new { message = $"Check triggered for source {id}", sourceId = id });
    }
}

public record AddSourceRequest(
    string Url,
    string Domain,
    string? Name = null,
    SourceType? Type = null,
    TimeSpan? CheckInterval = null,
    float? Credibility = null);
