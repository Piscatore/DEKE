namespace Deke.Core.Models;

public class DomainTrustPolicy
{
    public required string Domain { get; set; }
    public bool RequirePrimarySource { get; set; }
    public int MinCorroboration { get; set; }
    public List<SourceTier> AutoAcceptTiers { get; set; } = [];
    public List<SourceTier> FlagForReviewTiers { get; set; } = [];
    public bool TemporalValidityRequired { get; set; }
    public float MinConfidenceScore { get; set; }
}
