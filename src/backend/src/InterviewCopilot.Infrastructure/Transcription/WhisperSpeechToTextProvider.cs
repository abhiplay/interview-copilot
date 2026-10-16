using System.Diagnostics.CodeAnalysis;
using System.Text;
using InterviewCopilot.Application.Providers;
using InterviewCopilot.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using Whisper.net;
using Whisper.net.Ggml;

namespace InterviewCopilot.Infrastructure.Transcription;

/// <summary>
/// Local, free speech-to-text with whisper.cpp via Whisper.net (default, NFR-01). The model is
/// downloaded once into the user-data models folder and loaded lazily.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Needs native whisper.cpp + a ~500 MB model; covered by the manual release checklist.")]
internal sealed class WhisperSpeechToTextProvider(
    IOptions<TranscriptionOptions> transcriptionOptions,
    IOptions<StorageOptions> storageOptions) : ISpeechToTextProvider, IDisposable
{
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private WhisperFactory? _factory;

    public string Name => ProviderNames.Whisper;

    public async Task<TranscriptionResult> TranscribeAsync(
        AudioChunk chunk,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        var factory = await GetFactoryAsync(cancellationToken);
        var options = transcriptionOptions.Value.Providers.Whisper;

        await using var processor = factory.CreateBuilder().WithLanguage(options.Language).Build();
        using var wav = new MemoryStream(WavEncoder.Encode(chunk.Pcm16.Span, chunk.SampleRate));
        var text = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(wav, cancellationToken))
        {
            text.Append(segment.Text);
        }

        return new TranscriptionResult(text.ToString().Trim());
    }

    public void Dispose()
    {
        _factory?.Dispose();
        _loadLock.Dispose();
    }

    private async Task<WhisperFactory> GetFactoryAsync(CancellationToken cancellationToken)
    {
        if (_factory is not null)
        {
            return _factory;
        }

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (_factory is null)
            {
                var path = await EnsureModelAsync(cancellationToken);
                _factory = WhisperFactory.FromPath(path);
            }

            return _factory;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private async Task<string> EnsureModelAsync(CancellationToken cancellationToken)
    {
        var modelName = transcriptionOptions.Value.Providers.Whisper.Model;
        var directory = storageOptions.Value.ResolveModelsDirectory();
        var path = Path.Combine(directory, modelName);
        if (File.Exists(path))
        {
            return path;
        }

        if (!WhisperModelCatalog.TryGetType(modelName, out var ggmlType))
        {
            throw new TranscriptionUnavailableException(
                $"Whisper model '{modelName}' isn't in the models folder ({directory}) and can't be downloaded automatically.");
        }

        Directory.CreateDirectory(directory);
        var temporary = path + ".download";
        await using (var source = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType, cancellationToken: cancellationToken))
        await using (var target = File.Create(temporary))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        File.Move(temporary, path, overwrite: true);
        return path;
    }
}
