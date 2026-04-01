using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Deke.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Deke.Infrastructure.Llm;

public class OpenAiLlmService : ILlmService
{
    private readonly HttpClient _http;
    private readonly LlmConfig _config;
    private readonly ILogger<OpenAiLlmService> _logger;

    public OpenAiLlmService(HttpClient http, LlmConfig config, ILogger<OpenAiLlmService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_config.ActiveApiKey);

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("OpenAI API key is not configured.");

        var model = _config.ActiveModel;

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ActiveApiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        });

        _logger.LogDebug("Calling OpenAI model {Model}", model);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        var text = json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return text ?? string.Empty;
    }
}
