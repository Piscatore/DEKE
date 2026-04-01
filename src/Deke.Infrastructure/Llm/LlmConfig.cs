namespace Deke.Infrastructure.Llm;

public class LlmConfig
{
    public LlmProvider Provider { get; set; } = LlmProvider.None;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}

public enum LlmProvider
{
    None,
    Gemini,
    OpenAi
}
