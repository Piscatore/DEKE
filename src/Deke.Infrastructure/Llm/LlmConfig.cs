namespace Deke.Infrastructure.Llm;

public class LlmConfig
{
    public LlmProvider Provider { get; set; } = LlmProvider.None;
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-2.0-flash";
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiModel { get; set; } = "gpt-4o-mini";

    public string ActiveApiKey => Provider switch
    {
        LlmProvider.Gemini => GeminiApiKey,
        LlmProvider.OpenAi => OpenAiApiKey,
        _ => string.Empty
    };

    public string ActiveModel => Provider switch
    {
        LlmProvider.Gemini => GeminiModel,
        LlmProvider.OpenAi => OpenAiModel,
        _ => string.Empty
    };
}

public enum LlmProvider
{
    None,
    Gemini,
    OpenAi
}
