using Deke.Core.Models;

namespace Deke.Tests;

public class FederatedSearchRequestTests
{
    [Fact]
    public void FederatedSearchRequest_DomainIsOptional()
    {
        var request = new FederatedSearchRequest { Query = "test query" };

        Assert.Equal("test query", request.Query);
        Assert.Null(request.Domain);
        Assert.Equal(10, request.Limit);
        Assert.Equal(0.5f, request.MinSimilarity);
    }

    [Fact]
    public void FederatedSearchRequest_AcceptsDomain()
    {
        var request = new FederatedSearchRequest
        {
            Query = "ice fishing",
            Domain = "fishing",
            Limit = 20,
            MinSimilarity = 0.7f
        };

        Assert.Equal("fishing", request.Domain);
        Assert.Equal(20, request.Limit);
        Assert.Equal(0.7f, request.MinSimilarity);
    }
}

public class FederatedContextRequestTests
{
    [Fact]
    public void FederatedContextRequest_DomainIsOptional()
    {
        var request = new FederatedContextRequest { Topic = "brook trout" };

        Assert.Equal("brook trout", request.Topic);
        Assert.Null(request.Domain);
        Assert.Equal(2000, request.MaxTokens);
    }
}

public class FederationContextTests
{
    [Fact]
    public void FederationContext_HasDefaults()
    {
        var context = new FederationContext { QueryOrigin = "test-instance" };

        Assert.Equal(0, context.HopCount);
        Assert.Equal("test-instance", context.QueryOrigin);
        Assert.Empty(context.Visited);
        Assert.NotEqual(Guid.Empty, context.RequestId);
    }

    [Fact]
    public void FederationContext_VisitedSetTracksInstances()
    {
        var context = new FederationContext
        {
            QueryOrigin = "origin",
            Visited = ["instance-a", "instance-b"]
        };

        Assert.Contains("instance-a", context.Visited);
        Assert.Contains("instance-b", context.Visited);
        Assert.DoesNotContain("instance-c", context.Visited);
    }
}

public class ResultProvenanceTests
{
    [Fact]
    public void ResultProvenance_ConstructsCorrectly()
    {
        var provenance = new ResultProvenance
        {
            InstanceId = "wildlife-expert",
            Hops = 1
        };

        Assert.Equal("wildlife-expert", provenance.InstanceId);
        Assert.Equal(1, provenance.Hops);
        Assert.True(provenance.RetrievedAt <= DateTimeOffset.UtcNow);
    }
}

public class FederationMetadataTests
{
    [Fact]
    public void FederationMetadata_ConstructsCorrectly()
    {
        var metadata = new FederationMetadata
        {
            RequestId = Guid.NewGuid(),
            PeersConsulted = ["wildlife-expert"],
            PeersSkipped = ["regulations-expert"],
            TotalHops = 1,
            LocalResultCount = 3,
            FederatedResultCount = 2
        };

        Assert.Single(metadata.PeersConsulted);
        Assert.Single(metadata.PeersSkipped);
        Assert.Equal(1, metadata.TotalHops);
        Assert.Equal(3, metadata.LocalResultCount);
        Assert.Equal(2, metadata.FederatedResultCount);
    }
}

public class LocalityWeightTests
{
    [Fact]
    public void FederationConfig_LocalityWeights_HasDefaults()
    {
        var config = new FederationConfig();

        Assert.Equal(1.0f, config.LocalityWeights["Local"]);
        Assert.Equal(0.9f, config.LocalityWeights["Hop1"]);
        Assert.Equal(0.75f, config.LocalityWeights["Hop2"]);
        Assert.Equal(0.6f, config.LocalityWeights["Hop3"]);
    }

    [Fact]
    public void GetLocalityWeight_ReturnsLocal_ForZeroHops()
    {
        var config = new FederationConfig();
        Assert.Equal(1.0f, config.GetLocalityWeight(0));
    }

    [Fact]
    public void GetLocalityWeight_ReturnsCorrectWeight_ForEachHop()
    {
        var config = new FederationConfig();

        Assert.Equal(0.9f, config.GetLocalityWeight(1));
        Assert.Equal(0.75f, config.GetLocalityWeight(2));
        Assert.Equal(0.6f, config.GetLocalityWeight(3));
    }

    [Fact]
    public void GetLocalityWeight_ReturnsFallback_ForUnconfiguredHops()
    {
        var config = new FederationConfig();

        // Hops beyond configured weights get fallback of 0.5
        Assert.Equal(0.5f, config.GetLocalityWeight(4));
        Assert.Equal(0.5f, config.GetLocalityWeight(10));
    }

    [Fact]
    public void ScoreCalculation_AppliesLocalityWeight()
    {
        var config = new FederationConfig();

        var similarity = 0.85f;
        var confidence = 0.9f;

        var localScore = similarity * confidence * config.GetLocalityWeight(0);
        var hop1Score = similarity * confidence * config.GetLocalityWeight(1);
        var hop2Score = similarity * confidence * config.GetLocalityWeight(2);

        Assert.Equal(0.85f * 0.9f * 1.0f, localScore, precision: 4);
        Assert.Equal(0.85f * 0.9f * 0.9f, hop1Score, precision: 4);
        Assert.Equal(0.85f * 0.9f * 0.75f, hop2Score, precision: 4);

        // Local score > hop1 > hop2
        Assert.True(localScore > hop1Score);
        Assert.True(hop1Score > hop2Score);
    }
}

public class ExtendedSearchResultTests
{
    [Fact]
    public void FactSearchResult_SupportsProvenance()
    {
        var result = new FactSearchResult
        {
            Id = Guid.NewGuid(),
            Content = "Brook trout thrive in cold water",
            Domain = "wildlife",
            Confidence = 0.9f,
            Similarity = 0.85f,
            Provenance = new ResultProvenance
            {
                InstanceId = "wildlife-expert",
                Hops = 1
            }
        };

        Assert.NotNull(result.Provenance);
        Assert.Equal("wildlife-expert", result.Provenance.InstanceId);
    }

    [Fact]
    public void FactSearchResult_ProvenanceNullForLocal()
    {
        var result = new FactSearchResult
        {
            Id = Guid.NewGuid(),
            Content = "Local fact",
            Domain = "fishing",
            Confidence = 0.9f,
            Similarity = 0.85f
        };

        Assert.Null(result.Provenance);
    }

    [Fact]
    public void SearchResponse_SupportsNullDomain()
    {
        var response = new SearchResponse
        {
            Query = "test",
            Domain = null,
            Results = [],
            TotalCount = 0
        };

        Assert.Null(response.Domain);
    }

    [Fact]
    public void SearchResponse_SupportsFederationMetadata()
    {
        var response = new SearchResponse
        {
            Query = "test",
            Domain = "fishing",
            Results = [],
            TotalCount = 0,
            Federation = new FederationMetadata
            {
                RequestId = Guid.NewGuid(),
                PeersConsulted = ["wildlife-expert"],
                LocalResultCount = 5,
                FederatedResultCount = 3
            }
        };

        Assert.NotNull(response.Federation);
        Assert.Single(response.Federation.PeersConsulted);
    }

    [Fact]
    public void ContextResponse_SupportsNullDomainAndFederation()
    {
        var response = new ContextResponse
        {
            Topic = "trout",
            Domain = null,
            Context = "some context",
            FactCount = 1,
            ApproximateTokens = 10,
            Federation = new FederationMetadata
            {
                RequestId = Guid.NewGuid(),
                PeersConsulted = ["peer-1"],
                LocalResultCount = 0,
                FederatedResultCount = 1
            }
        };

        Assert.Null(response.Domain);
        Assert.NotNull(response.Federation);
    }
}
