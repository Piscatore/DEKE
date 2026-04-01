using System.Net.Http.Json;
using System.Text.Json;
using Deke.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Deke.Infrastructure.Llm;

public class GeminiLlmService : ILlmService
{
    private readonly HttpClient _http;
    private readonly LlmConfig _config;
    private readonly ILogger<GeminiLlmService> _logger;

    public GeminiLlmService(HttpClient http, LlmConfig config, ILogger<GeminiLlmService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_config.ApiKey);

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Gemini API key is not configured.");

        var model = string.IsNullOrWhiteSpace(_config.Model) ? "gemini-2.0-flash" : _config.Model;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_config.ApiKey}";

        var request = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        _logger.LogDebug("Calling Gemini model {Model}", model);

        var response = await _http.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        var text = json
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
