using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using InterviewCopilot.Application.Providers;
using Microsoft.Extensions.Options;

namespace InterviewCopilot.Infrastructure.Transcription;

/// <summary>
/// Deepgram pre-recorded transcription per audio chunk. Implemented but only registered when
/// Transcription:Providers:Deepgram:Enabled is true (zero-cost principle). The user's key comes
/// from the desktop keychain per request and is never stored by the backend.
/// </summary>
internal sealed class DeepgramSpeechToTextProvider(HttpClient http, IOptions<TranscriptionOptions> options)
    : ISpeechToTextProvider
{
    public string Name => ProviderNames.Deepgram;

    public async Task<TranscriptionResult> TranscribeAsync(
        AudioChunk chunk,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        var deepgram = options.Value.Providers.Deepgram;
        if (!deepgram.Enabled)
        {
            throw new TranscriptionUnavailableException("Deepgram is disabled in configuration.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new TranscriptionUnavailableException("Add your Deepgram API key in Settings → Providers.");
        }

        var uri = new Uri(
            deepgram.BaseUrl,
            string.Create(CultureInfo.InvariantCulture, $"v1/listen?model={Uri.EscapeDataString(deepgram.Model)}&smart_format=true&language=en"));
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new ByteArrayContent(WavEncoder.Encode(chunk.Pcm16.Span, chunk.SampleRate)),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        request.Headers.Authorization = new AuthenticationHeaderValue("Token", apiKey);

        using var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new TranscriptionUnavailableException("Deepgram rejected the API key.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new TranscriptionUnavailableException($"Deepgram returned {(int)response.StatusCode}.");
        }

        var body = await response.Content.ReadFromJsonAsync<DeepgramResponse>(cancellationToken);
        var transcript = body?.Results?.Channels is [{ Alternatives: [{ Transcript: var text }, ..] }, ..] ? text : null;
        return new TranscriptionResult(transcript?.Trim() ?? string.Empty);
    }

    private sealed record DeepgramResponse([property: JsonPropertyName("results")] DeepgramResults? Results);

    private sealed record DeepgramResults([property: JsonPropertyName("channels")] IReadOnlyList<DeepgramChannel>? Channels);

    private sealed record DeepgramChannel([property: JsonPropertyName("alternatives")] IReadOnlyList<DeepgramAlternative>? Alternatives);

    private sealed record DeepgramAlternative([property: JsonPropertyName("transcript")] string? Transcript);
}
