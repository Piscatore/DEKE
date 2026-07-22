using Deke.Core.Interfaces;

namespace Deke.Api.Endpoints;

public static class ReviewQueueEndpoints
{
    public static void MapReviewQueueEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/review-queue").WithTags("ReviewQueue");

        group.MapGet("/", GetReviewQueue)
            .WithName("GetReviewQueue")
            .WithDescription("Get facts pending human review")
            .AllowAnonymous();
    }

    internal static async Task<IResult> GetReviewQueue(
        string? domain,
        int limit = 100,
        IFactRepository factRepo = null!,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        var facts = await factRepo.GetPendingReviewAsync(domain, limit, ct);
        return Results.Ok(facts);
    }
}
