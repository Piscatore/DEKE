using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/search").WithTags("Search").AllowAnonymous();

        group.MapPost("/", SearchFacts)
            .WithName("SearchFacts")
            .WithDescription("Search for facts using semantic similarity, with optional federation");

        group.MapPost("/context", GetContext)
            .WithName("GetContext")
            .WithDescription("Get relevant context for a topic, formatted for LLM consumption");
    }

    private static async Task<IResult> SearchFacts(
        FederatedSearchRequest request,
        HttpContext httpContext,
        IFederatedSearchService searchService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Results.BadRequest(new { error = "Query is required." });

        var federation = ParseFederationContext(httpContext);
        var response = await searchService.SearchAsync(request, federation, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetContext(
        FederatedContextRequest request,
        HttpContext httpContext,
        IFederatedSearchService searchService,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Topic))
            return Results.BadRequest(new { error = "Topic is required." });

        var federation = ParseFederationContext(httpContext);
        var response = await searchService.GetContextAsync(request, federation, ct);
        return Results.Ok(response);
    }

    private static FederationContext? ParseFederationContext(HttpContext httpContext)
    {
        var headers = httpContext.Request.Headers;

        if (!headers.TryGetValue("X-Federation-Hop-Count", out var hopCountHeader))
            return null;

        if (!int.TryParse(hopCountHeader.ToString(), out var hopCount))
            return null;

        var queryOrigin = headers.TryGetValue("X-Federation-Query-Origin", out var origin)
            ? origin.ToString()
            : "unknown";

        var visited = headers.TryGetValue("X-Federation-Visited", out var visitedHeader)
            ? visitedHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : [];

        var requestId = headers.TryGetValue("X-Federation-Request-Id", out var requestIdHeader)
            && Guid.TryParse(requestIdHeader.ToString(), out var parsedId)
            ? parsedId
            : Guid.NewGuid();

        return new FederationContext
        {
            HopCount = hopCount,
            QueryOrigin = queryOrigin,
            Visited = visited,
            RequestId = requestId
        };
    }
}
