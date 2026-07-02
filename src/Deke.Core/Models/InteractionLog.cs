using System.Text.Json;

namespace Deke.Core.Models;

public class InteractionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Domain { get; set; }
    public required string Query { get; set; }
    public required string Model { get; set; }
    public List<Guid> ReturnedFactIds { get; set; } = [];
    public List<float> Scores { get; set; } = [];
    public float MinSimilarity { get; set; }
    public int ResultCount { get; set; }
    public int DurationMs { get; set; }
    public Dictionary<string, JsonElement>? Federation { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
