using Deke.Infrastructure.Ingestion;

namespace Deke.Tests;

public class SimHasherTests
{
    private readonly SimHasher _hasher = new();

    [Fact]
    public void HammingDistance_OfIdenticalHashes_IsZero()
    {
        var h = _hasher.Compute("the quick brown fox jumps over the lazy dog");
        Assert.Equal(0, _hasher.HammingDistance(h, h));
    }

    [Fact]
    public void NearDuplicate_IsCloserThanUnrelated()
    {
        var baseline = _hasher.Compute("the quick brown fox jumps over the lazy dog in the yard");
        var near = _hasher.Compute("the quick brown fox jumps over the lazy dog in the garden");
        var far = _hasher.Compute("quarterly earnings beat analyst expectations across the technology sector");

        var nearDist = _hasher.HammingDistance(baseline, near);
        var farDist = _hasher.HammingDistance(baseline, far);

        Assert.True(nearDist < farDist, $"near={nearDist} far={farDist}");
        Assert.True(nearDist <= 12, $"near={nearDist}");
    }
}
