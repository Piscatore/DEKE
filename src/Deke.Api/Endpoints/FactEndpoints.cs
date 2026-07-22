using System.Text.Json;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class FactEndpoints
{
    public static void MapFactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/facts").WithTags("Facts");

        group.MapGet("/{id:guid}", GetFact)
            .WithName("GetFact")
            .WithDescription("Get a fact by ID")
            .AllowAnonymous();

        group.MapGet("/domain/{domain}", GetFactsByDomain)
            .WithName("GetFactsByDomain")
            .WithDescription("Get all facts for a domain")
            .AllowAnonymous();

        group.MapPost("/", AddFact)
            .WithName("AddFact")
            .WithDescription("Add a new fact with auto-generated embedding")
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateFact)
            .WithName("UpdateFact")
            .WithDescription("Update a fact, regenerating the embedding when content changes")
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteFact)
            .WithName("DeleteFact")
            .WithDescription("Soft-delete a fact by marking it outdated")
            .RequireAuthorization();

        group.MapGet("/stats/{domain}", GetStats)
            .WithName("GetStats")
            .WithDescription("Get fact statistics for a domain")
            .AllowAnonymous();
    }

    private static async Task<IResult> GetFact(
        Guid id,
        IFactRepository factRepo,
        CancellationToken ct)
    {
        var fact = await factRepo.GetByIdAsync(id, ct);
        return fact is null ? Results.NotFound() : Results.Ok(fact);
    }

    private static async Task<IResult> GetFactsByDomain(
        string domain,
        int limit = 100,
        IFactRepository factRepo = null!,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return Results.BadRequest(new { error = "Domain is required." });

        limit = Math.Clamp(limit, 1, 500);

        var facts = await factRepo.GetByDomainAsync(domain, limit, ct);
        return Results.Ok(facts);
    }

    private static async Task<IResult> AddFact(
        AddFactRequest request,
        IDeduplicationService dedup,
        IEmbeddingService embeddings,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return Results.BadRequest(new { error = "Content is required." });
        if (string.IsNullOrWhiteSpace(request.Domain))
            return Results.BadRequest(new { error = "Domain is required." });

        var fact = new Fact
        {
            Content = request.Content,
            Domain = request.Domain,
            Embedding = embeddings.GenerateEmbedding(request.Content),
            Confidence = request.Confidence ?? 1.0f,
            SourceId = request.SourceId,
            Metadata = request.Metadata ?? []
        };

        var result = await dedup.IngestAsync(fact, ExtractionMethod.ManualApi, ct);
        return Results.Created($"/api/facts/{result.FactId}", new { id = result.FactId, duplicate = result.WasDuplicate });
    }

    internal static async Task<IResult> UpdateFact(
        Guid id,
        UpdateFactRequest request,
        IFactRepository factRepo,
        IEmbeddingService embeddings,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return Results.BadRequest(new { error = "Content is required." });
        if (string.IsNullOrWhiteSpace(request.Domain))
            return Results.BadRequest(new { error = "Domain is required." });

        var fact = await factRepo.GetByIdAsync(id, ct);
        if (fact is null)
            return Results.NotFound();

        var contentChanged = !string.Equals(fact.Content, request.Content, StringComparison.Ordinal);

        fact.Content = request.Content;
        fact.Domain = request.Domain;
        if (contentChanged)
            fact.Embedding = embeddings.GenerateEmbedding(request.Content);
        fact.Confidence = request.Confidence ?? fact.Confidence;
        fact.SourceId = request.SourceId ?? fact.SourceId;
        fact.Metadata = request.Metadata ?? fact.Metadata;

        await factRepo.UpdateAsync(fact, ct);
        return Results.Ok(fact);
    }

    internal static async Task<IResult> DeleteFact(
        Guid id,
        string? reason,
        IFactRepository factRepo,
        CancellationToken ct)
    {
        var fact = await factRepo.GetByIdAsync(id, ct);
        if (fact is null)
            return Results.NotFound();

        await factRepo.MarkOutdatedAsync(id, reason ?? "Deleted via API", ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetStats(
        string domain,
        IFactRepository factRepo,
        CancellationToken ct)
    {
        var count = await factRepo.GetCountAsync(domain, ct);
        return Results.Ok(new { domain, factCount = count });
    }
}

public record AddFactRequest(
    string Content,
    string Domain,
    float? Confidence = null,
    Guid? SourceId = null,
    Dictionary<string, JsonElement>? Metadata = null);

public record UpdateFactRequest(
    string Content,
    string Domain,
    float? Confidence = null,
    Guid? SourceId = null,
    Dictionary<string, JsonElement>? Metadata = null);
