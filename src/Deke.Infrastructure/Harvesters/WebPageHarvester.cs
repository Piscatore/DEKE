using System.Security.Cryptography;
using System.Text;
using AngleSharp;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Harvesters;

public class WebPageHarvester : IHarvester
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebPageHarvester(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public SourceType SupportedType => SourceType.WebPage;

    public async Task<HarvestResult> HarvestAsync(Source source, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var html = await client.GetStringAsync(source.Url, ct);

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html), ct);

            var contentElement = document.QuerySelector("article")
                ?? document.QuerySelector("main")
                ?? document.QuerySelector("body");

            var text = contentElement?.TextContent?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return new HarvestResult
                {
                    HasChanges = false,
                    ErrorMessage = "No text content found on page"
                };
            }

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            var newHash = Convert.ToHexStringLower(hashBytes);
            var hasChanges = !string.Equals(source.ContentHash, newHash, StringComparison.Ordinal);

            return new HarvestResult
            {
                HasChanges = hasChanges,
                NewContentHash = newHash,
                ExtractedTexts = [text]
            };
        }
        catch (Exception ex)
        {
            return new HarvestResult
            {
                HasChanges = false,
                ErrorMessage = $"Web page harvest failed: {ex.Message}"
            };
        }
    }
}
