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
        IFactRepository factRepo,
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

        var id = await factRepo.AddAsync(fact, ct);
        return Results.Created($"/api/facts/{id}", new { id });
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
