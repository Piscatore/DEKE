using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Api.Endpoints;

public static class DomainTrustPolicyEndpoints
{
    public static void MapDomainTrustPolicyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/domains").WithTags("DomainTrustPolicy");

        group.MapGet("/{domain}/trust-policy", GetTrustPolicy)
            .WithName("GetTrustPolicy")
            .WithDescription("Get a domain's trust policy configuration")
            .AllowAnonymous();

        group.MapPut("/{domain}/trust-policy", UpdateTrustPolicy)
            .WithName("UpdateTrustPolicy")
            .WithDescription("Create or replace a domain's trust policy configuration")
            .RequireAuthorization();
    }

    internal static async Task<IResult> GetTrustPolicy(
        string domain,
        IDomainTrustPolicyRepository policyRepo,
        CancellationToken ct)
    {
        var policy = await policyRepo.GetByDomainAsync(domain, ct);
        return policy is null ? Results.NotFound() : Results.Ok(policy);
    }

    internal static async Task<IResult> UpdateTrustPolicy(
        string domain,
        UpdateTrustPolicyRequest request,
        IDomainTrustPolicyRepository policyRepo,
        CancellationToken ct)
    {
        var policy = new DomainTrustPolicy
        {
            Domain = domain,
            RequirePrimarySource = request.RequirePrimarySource,
            MinCorroboration = request.MinCorroboration,
            AutoAcceptTiers = request.AutoAcceptTiers,
            FlagForReviewTiers = request.FlagForReviewTiers,
            TemporalValidityRequired = request.TemporalValidityRequired,
            MinConfidenceScore = request.MinConfidenceScore
        };

        await policyRepo.UpsertAsync(policy, ct);
        return Results.Ok(policy);
    }
}

public record UpdateTrustPolicyRequest(
    bool RequirePrimarySource,
    int MinCorroboration,
    List<SourceTier> AutoAcceptTiers,
    List<SourceTier> FlagForReviewTiers,
    bool TemporalValidityRequired,
    float MinConfidenceScore);
