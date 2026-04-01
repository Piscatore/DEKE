using System.Net.Http.Json;
using Deke.Core.Models;
using Microsoft.Extensions.Logging;

namespace Deke.Infrastructure.Federation;

public class FederationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FederationClient> _logger;

    public FederationClient(
        IHttpClientFactory httpClientFactory,
        ILogger<FederationClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SearchResponse?> SearchPeerAsync(
        string baseUrl,
        FederatedSearchRequest request,
        FederationContext context,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("federation");
        var url = $"{baseUrl.TrimEnd('/')}/api/search";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = JsonContent.Create(request);

        httpRequest.Headers.Add("X-Federation-Hop-Count", context.HopCount.ToString());
        httpRequest.Headers.Add("X-Federation-Query-Origin", context.QueryOrigin);
        httpRequest.Headers.Add("X-Federation-Visited", string.Join(",", context.Visited));
        httpRequest.Headers.Add("X-Federation-Request-Id", context.RequestId.ToString());

        try
        {
            var response = await client.SendAsync(httpRequest, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Peer search to {BaseUrl} returned {StatusCode}",
                    baseUrl, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken: ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Peer search to {BaseUrl} failed", baseUrl);
            return null;
        }
    }
}
