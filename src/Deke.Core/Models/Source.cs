using System.Text.Json;

namespace Deke.Core.Models;

public class Source
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Url { get; set; }
    public required string Domain { get; set; }
    public string? Name { get; set; }
    public SourceType Type { get; set; } = SourceType.WebPage;
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromDays(1);
    public DateTimeOffset? LastCheckedAt { get; set; }
    public DateTimeOffset? LastChangedAt { get; set; }
    public string? ContentHash { get; set; }
    public float Credibility { get; set; } = 0.5f;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, JsonElement> Metadata { get; set; } = [];

    // Navigation
    public List<Fact> Facts { get; set; } = [];

    // Computed
    public DateTimeOffset? NextCheckAt => LastCheckedAt?.Add(CheckInterval);
    public bool IsDueForCheck => NextCheckAt == null || NextCheckAt <= DateTimeOffset.UtcNow;
}

public enum SourceType
{
    WebPage,
    Rss,
    Api,
    Manual
}
