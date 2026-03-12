using System.Security.Cryptography;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Harvesters;

public class RssHarvester : IHarvester
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RssHarvester(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public SourceType SupportedType => SourceType.Rss;

    public async Task<HarvestResult> HarvestAsync(Source source, CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(source.Url, ct);

            var xmlSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            using var reader = XmlReader.Create(new StringReader(response), xmlSettings);
            var feed = SyndicationFeed.Load(reader);

            var extractedTexts = new List<string>();
            var contentBuilder = new StringBuilder();

            foreach (var item in feed.Items)
            {
                var title = item.Title?.Text ?? string.Empty;
                var summary = item.Summary?.Text ?? string.Empty;
                var text = $"{title}. {summary}".Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    extractedTexts.Add(text);
                    contentBuilder.Append(text);
                }
            }

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(contentBuilder.ToString()));
            var newHash = Convert.ToHexStringLower(hashBytes);
            var hasChanges = !string.Equals(source.ContentHash, newHash, StringComparison.Ordinal);

            return new HarvestResult
            {
                HasChanges = hasChanges,
                NewContentHash = newHash,
                ExtractedTexts = extractedTexts
            };
        }
        catch (Exception ex)
        {
            return new HarvestResult
            {
                HasChanges = false,
                ErrorMessage = $"RSS harvest failed: {ex.Message}"
            };
        }
    }
}
