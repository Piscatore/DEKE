using System.Security.Cryptography;
using System.Text;
using Deke.Core.Interfaces;
using Deke.Core.Models;

namespace Deke.Infrastructure.Harvesters;

public class FileSystemHarvester : IHarvester
{
    private static readonly string[] SupportedExtensions = [".md", ".markdown", ".txt"];

    public SourceType SupportedType => SourceType.File;

    public async Task<HarvestResult> HarvestAsync(Source source, CancellationToken ct = default)
    {
        try
        {
            var path = source.Url;

            var files = Directory.Exists(path)
                ? Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => SupportedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(f => f, StringComparer.Ordinal)
                    .ToList()
                : File.Exists(path)
                    ? [path]
                    : [];

            var extractedTexts = new List<string>();
            var contentBuilder = new StringBuilder();

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                var text = (await File.ReadAllTextAsync(file, ct)).Trim();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                extractedTexts.Add(text);
                contentBuilder.Append(text);
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
                ErrorMessage = $"File harvest failed: {ex.Message}"
            };
        }
    }
}
