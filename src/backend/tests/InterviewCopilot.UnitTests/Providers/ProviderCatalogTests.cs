using FluentAssertions;
using InterviewCopilot.Application.Providers;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.UnitTests.Providers;

public class ProviderCatalogTests
{
    private static ProviderCatalog Catalog(bool deepgramEnabled, string provider = ProviderNames.Whisper)
    {
        var transcription = new TranscriptionOptions { Provider = provider };
        transcription.Providers.Deepgram.Enabled = deepgramEnabled;
        return new ProviderCatalog(Options.Create(transcription), Options.Create(new LlmOptions()));
    }

    [Fact]
    public void Should_OfferOnlyWhisper_When_DeepgramDisabled()
    {
        var catalog = Catalog(deepgramEnabled: false);

        catalog.SpeechToTextProviders().Should().ContainSingle().Which.Name.Should().Be(ProviderNames.Whisper);
        catalog.IsSpeechToTextProviderAvailable(ProviderNames.Deepgram).Should().BeFalse();
    }

    [Fact]
    public void Should_OfferDeepgram_When_Enabled()
    {
        var catalog = Catalog(deepgramEnabled: true, provider: ProviderNames.Deepgram);

        var providers = catalog.SpeechToTextProviders();

        providers.Should().HaveCount(2);
        providers.Single(p => p.Name == ProviderNames.Deepgram).IsDefault.Should().BeTrue();
        providers.Single(p => p.Name == ProviderNames.Deepgram).RequiresApiKey.Should().BeTrue();
        catalog.IsSpeechToTextProviderAvailable("DEEPGRAM").Should().BeTrue();
    }

    [Fact]
    public void Should_DefaultToLocalFreeProviders()
    {
        var llm = Catalog(deepgramEnabled: false).LlmProviders();
        var defaults = new LlmOptions();

        llm.Should().ContainSingle().Which.Should().Match<ProviderInfo>(p => p.IsLocal && !p.RequiresApiKey && p.IsDefault);
        defaults.Model.Should().Be("llama3.2:3b");
        new TranscriptionOptions().Providers.Deepgram.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Should_KeepMessages_When_ProviderExceptionsConstructed()
    {
        new LlmUnavailableException().Should().NotBeNull();
        new LlmUnavailableException("x").Message.Should().Be("x");
        new TranscriptionUnavailableException().Should().NotBeNull();
        new TranscriptionUnavailableException("y", new InvalidOperationException()).InnerException.Should().NotBeNull();
    }
}
