using Deke.Core.Models;
using Deke.Infrastructure.Advisory;
using Microsoft.Extensions.Options;

namespace Deke.Tests;

public class LlmSelectionPolicyTests
{
    private static LlmSelectionPolicy BuildPolicy(AdvisoryConfig? config = null) =>
        new(Options.Create(config ?? new AdvisoryConfig()));

    [Fact]
    public void Select_WithModelOverride_RoutesToAnthropicWithOverride()
    {
        var policy = BuildPolicy();

        var selection = policy.Select(0.9, ConfidenceBand.High, Stakes.Low, allowLocalModel: true, modelOverride: "claude-opus-4-8");

        Assert.Equal(AdvisoryClientKeys.Anthropic, selection.ClientKey);
        Assert.Equal("claude-opus-4-8", selection.ModelId);
    }

    [Fact]
    public void Select_LocalAllowedAndDeepKnowledge_RoutesToOllama()
    {
        var config = new AdvisoryConfig { OllamaModel = "llama3.1", OllamaDepthThreshold = 0.75 };
        var policy = BuildPolicy(config);

        var selection = policy.Select(0.8, ConfidenceBand.High, Stakes.Low, allowLocalModel: true, modelOverride: null);

        Assert.Equal(AdvisoryClientKeys.Ollama, selection.ClientKey);
        Assert.Equal("llama3.1", selection.ModelId);
    }

    [Fact]
    public void Select_LocalAllowedButShallowKnowledge_DoesNotUseOllama()
    {
        var policy = BuildPolicy(new AdvisoryConfig { OllamaDepthThreshold = 0.75 });

        var selection = policy.Select(0.7, ConfidenceBand.High, Stakes.Low, allowLocalModel: true, modelOverride: null);

        Assert.Equal(AdvisoryClientKeys.Anthropic, selection.ClientKey);
    }

    [Fact]
    public void Select_LowBandHighStakes_EscalatesToSonnet()
    {
        var config = new AdvisoryConfig { SonnetModel = "claude-sonnet-5" };
        var policy = BuildPolicy(config);

        var selection = policy.Select(0.9, ConfidenceBand.Low, Stakes.High, allowLocalModel: false, modelOverride: null);

        Assert.Equal(AdvisoryClientKeys.Anthropic, selection.ClientKey);
        Assert.Equal("claude-sonnet-5", selection.ModelId);
    }

    [Fact]
    public void Select_AdequateDepth_UsesHaikuByDefault()
    {
        var config = new AdvisoryConfig { HaikuModel = "claude-haiku-4-5", HaikuDepthThreshold = 0.6 };
        var policy = BuildPolicy(config);

        var selection = policy.Select(0.65, ConfidenceBand.High, Stakes.Low, allowLocalModel: false, modelOverride: null);

        Assert.Equal(AdvisoryClientKeys.Anthropic, selection.ClientKey);
        Assert.Equal("claude-haiku-4-5", selection.ModelId);
    }

    [Fact]
    public void Select_ThinKnowledge_EscalatesToSonnet()
    {
        var config = new AdvisoryConfig { SonnetModel = "claude-sonnet-5", HaikuDepthThreshold = 0.6 };
        var policy = BuildPolicy(config);

        var selection = policy.Select(0.4, ConfidenceBand.Medium, Stakes.Low, allowLocalModel: false, modelOverride: null);

        Assert.Equal(AdvisoryClientKeys.Anthropic, selection.ClientKey);
        Assert.Equal("claude-sonnet-5", selection.ModelId);
    }

    [Fact]
    public void Select_LocalNotAllowed_NeverUsesOllamaEvenWhenDeep()
    {
        var policy = BuildPolicy(new AdvisoryConfig { OllamaDepthThreshold = 0.75 });

        var selection = policy.Select(0.95, ConfidenceBand.High, Stakes.Low, allowLocalModel: false, modelOverride: null);

        Assert.NotEqual(AdvisoryClientKeys.Ollama, selection.ClientKey);
    }
}
