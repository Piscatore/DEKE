namespace Deke.Core.Models;

/// <summary>
/// Optional caller hints influencing model selection and formatting.
/// </summary>
public record AdvisoryHints
{
    public Stakes? Stakes { get; init; }
    public bool PreferCitations { get; init; } = true;
    public string? ModelOverride { get; init; }
}

/// <summary>
/// Fixed advisory contract (Layer 1) — request side. Never changes; new fields are additive.
/// </summary>
public record AdvisoryRequest
{
    public required string Query { get; init; }
    public required string Domain { get; init; }
    public string? SessionId { get; init; }

    /// <summary>Prior turns for multi-turn continuity. Accepted but unused in the MVP prompt.</summary>
    public string[] PriorExchanges { get; init; } = [];

    public AdvisoryHints? Hints { get; init; }
}
