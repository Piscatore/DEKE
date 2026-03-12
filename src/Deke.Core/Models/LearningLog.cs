namespace Deke.Core.Models;

public class LearningLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Domain { get; set; }
    public required string CycleType { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int FactsAdded { get; set; }
    public int FactsUpdated { get; set; }
    public int FactsOutdated { get; set; }
    public int PatternsDiscovered { get; set; }
    public int RelationsAdded { get; set; }
    public string? Notes { get; set; }
    public string? ErrorMessage { get; set; }
}
