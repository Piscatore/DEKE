using Anthropic.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace Deke.Infrastructure.Advisory;

/// <summary>
/// Registers the keyed <see cref="IChatClient"/> backends used by the advisory pipeline.
/// The Anthropic client serves both haiku and sonnet — the model is chosen per call via
/// <see cref="ChatOptions.ModelId"/>, so no separate per-model clients are needed.
/// </summary>
public static class ChatClientRegistration
{
    public static IServiceCollection AddAdvisoryChatClients(
        this IServiceCollection services, AdvisoryConfig config)
    {
        services.AddKeyedSingleton<IChatClient>(
            AdvisoryClientKeys.Anthropic,
            (_, _) => new AnthropicClient(config.AnthropicApiKey).Messages);

        services.AddKeyedSingleton<IChatClient>(
            AdvisoryClientKeys.Ollama,
            (_, _) => new OllamaApiClient(new Uri(config.OllamaBaseUrl)));

        return services;
    }
}
