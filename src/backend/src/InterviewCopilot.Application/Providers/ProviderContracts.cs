using InterviewCopilot.Domain.Enums;

namespace InterviewCopilot.Application.Providers;

public enum LlmRole
{
    System = 0,
    User = 1,
    Assistant = 2,
}

public sealed record LlmMessage(LlmRole Role, string Content);

/// <summary>One chat completion request.</summary>
/// <param name="Model">Provider-specific model id, e.g. "llama3.2:3b".</param>
/// <param name="ApiKey">BYOK key passed per request from the desktop keychain. Never persisted or logged.</param>
public sealed record LlmRequest(
    string Model,
    IReadOnlyList<LlmMessage> Messages,
    double Temperature,
    string? ApiKey = null);

/// <summary>A chat model that streams its reply token by token.</summary>
public interface ILlmProvider
{
    string Name { get; }

    IAsyncEnumerable<string> StreamAsync(LlmRequest request, CancellationToken cancellationToken);
}

/// <summary>A buffered chunk of one speaker's audio: PCM16, mono.</summary>
public sealed record AudioChunk(Speaker Speaker, ReadOnlyMemory<byte> Pcm16, int SampleRate, long StartMs);

public sealed record TranscriptionResult(string Text);

/// <summary>Turns audio into text. Whisper (local) by default; Deepgram when enabled (FR-15).</summary>
public interface ISpeechToTextProvider
{
    string Name { get; }

    Task<TranscriptionResult> TranscribeAsync(AudioChunk chunk, string? apiKey, CancellationToken cancellationToken);
}
