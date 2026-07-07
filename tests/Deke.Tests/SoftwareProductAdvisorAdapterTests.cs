using Deke.Core.Models;
using Deke.Infrastructure.Advisory;

namespace Deke.Tests;

public class SoftwareProductAdvisorAdapterTests
{
    private static FactSearchResult Fact(float credibility, float similarity, DateTimeOffset createdAt) => new()
    {
        Id = Guid.NewGuid(),
        Content = "design decision",
        Domain = SoftwareProductAdvisorAdapter.DomainName,
        SourceCredibility = credibility,
        Confidence = 0.9f,
        Similarity = similarity,
        CreatedAt = createdAt
    };

    [Fact]
    public void ActivationCriteria_TargetsSoftwareProductDomain()
    {
        var adapter = new SoftwareProductAdvisorAdapter();

        Assert.Equal("software-product", adapter.ActivationCriteria.Domain);
        Assert.True(adapter.ActivationCriteria.AllowLocalModel);
    }

    [Fact]
    public void SystemPrompt_MentionsDekeAndVersionAwareness()
    {
        var prompt = new SoftwareProductAdvisorAdapter().SystemPrompt();

        Assert.Contains("DEKE", prompt);
        Assert.Contains("version-aware", prompt);
    }

    [Fact]
    public void WeightFacts_RanksHigherCredibilityFirst()
    {
        var low = Fact(0.3f, 0.9f, DateTimeOffset.UtcNow);
        var high = Fact(0.95f, 0.5f, DateTimeOffset.UtcNow.AddDays(-30));
        var adapter = new SoftwareProductAdvisorAdapter();

        var weighted = adapter.WeightFacts([low, high], "query");

        Assert.Equal(high.Id, weighted[0].Id);
    }

    [Fact]
    public void WeightFacts_PrefersRecentAmongEqualCredibility()
    {
        var older = Fact(0.8f, 0.9f, DateTimeOffset.UtcNow.AddDays(-100));
        var newer = Fact(0.8f, 0.9f, DateTimeOffset.UtcNow.AddDays(-1));
        var adapter = new SoftwareProductAdvisorAdapter();

        var weighted = adapter.WeightFacts([older, newer], "query");

        Assert.Equal(newer.Id, weighted[0].Id);
    }

    [Fact]
    public void FormatContext_IncludesFactIdsAndContent()
    {
        var fact = Fact(0.9f, 0.9f, DateTimeOffset.UtcNow);
        var context = new SoftwareProductAdvisorAdapter().FormatContext([fact]);

        Assert.Contains(fact.Id.ToString(), context);
        Assert.Contains("design decision", context);
    }
}
