namespace Deke.Core.Models;

public class Term
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string CanonicalForm { get; set; }
    public required string Domain { get; set; }
    public List<TermContext> Contexts { get; set; } = [];
    public Dictionary<string, string> Translations { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class TermContext
{
    public required string Name { get; set; }
    public required string Definition { get; set; }
    public List<string> Signals { get; set; } = [];
}
