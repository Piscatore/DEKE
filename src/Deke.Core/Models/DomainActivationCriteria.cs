namespace Deke.Core.Models;

/// <summary>
/// Declares when a domain adapter is active and what backends it permits.
/// A domain is "activated" when an adapter is registered for it and the
/// knowledge base holds at least <see cref="MinFacts"/> facts in that domain.
/// </summary>
public record DomainActivationCriteria
{
    public required string Domain { get; init; }
    public int MinFacts { get; init; } = 1;

    /// <summary>Whether this domain permits routing to the local (Ollama) backend.</summary>
    public bool AllowLocalModel { get; init; }
}
