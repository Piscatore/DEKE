using Deke.Infrastructure.Ingestion;

namespace Deke.Tests;

public class ContentHasherTests
{
    private readonly ContentHasher _hasher = new();

    [Fact]
    public void ContentHash_IsStableAndCaseSensitive()
    {
        Assert.Equal(_hasher.ContentHash("Ice fishing tips"), _hasher.ContentHash("Ice fishing tips"));
        Assert.NotEqual(_hasher.ContentHash("Ice fishing tips"), _hasher.ContentHash("ice fishing tips"));
    }

    [Fact]
    public void ContentHash_IsSixtyFourHexChars()
    {
        var hash = _hasher.ContentHash("anything");
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Fact]
    public void NormalizedHash_IgnoresWhitespaceCaseAndTrailingPunctuation()
    {
        var a = _hasher.NormalizedHash("Ice fishing  TIPS!");
        var b = _hasher.NormalizedHash("   ice fishing tips   ");
        Assert.Equal(a, b);
    }

    [Fact]
    public void NormalizedHash_DiffersForDifferentContent()
    {
        Assert.NotEqual(_hasher.NormalizedHash("ice fishing"), _hasher.NormalizedHash("fly fishing"));
    }
}
