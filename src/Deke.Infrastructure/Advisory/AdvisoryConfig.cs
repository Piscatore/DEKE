namespace Deke.Infrastructure.Advisory;

/// <summary>
/// Configuration for the advisory pipeline: model backends, routing thresholds,
/// and retrieval parameters. Bound from the "Advisory" configuration section.
/// The Anthropic API key should come from user-secrets (local) or an env var (deploy).
/// </summary>
public class AdvisoryConfig
{
    public string AnthropicApiKey { get; set; } = string.Empty;
    public string HaikuModel { get; set; } = "claude-haiku-4-5";
    public string SonnetModel { get; set; } = "claude-sonnet-5";
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3.1";

    /// <summary>Minimum knowledge_depth_score to use the cheap default (haiku) instead of escalating.</summary>
    public double HaikuDepthThreshold { get; set; } = 0.6;

    /// <summary>Minimum knowledge_depth_score to allow the local (Ollama) backend.</summary>
    public double OllamaDepthThreshold { get; set; } = 0.75;

    public int RetrievalLimit { get; set; } = 10;
    public float MinSimilarity { get; set; } = 0.5f;
    public int MaxOutputTokens { get; set; } = 1024;
}

/// <summary>Keys for the registered <c>IChatClient</c> backends.</summary>
public static class AdvisoryClientKeys
{
    public const string Anthropic = "anthropic";
    public const string Ollama = "ollama";
}
