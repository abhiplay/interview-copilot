using Whisper.net.Ggml;

namespace InterviewCopilot.Infrastructure.Transcription;

/// <summary>Maps configured model file names to downloadable ggml models.</summary>
internal static class WhisperModelCatalog
{
    private static readonly Dictionary<string, GgmlType> Known = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ggml-tiny.en.bin"] = GgmlType.TinyEn,
        ["ggml-base.en.bin"] = GgmlType.BaseEn,
        ["ggml-small.en.bin"] = GgmlType.SmallEn,
        ["ggml-medium.en.bin"] = GgmlType.MediumEn,
    };

    public static bool TryGetType(string modelFileName, out GgmlType type) =>
        Known.TryGetValue(modelFileName, out type);
}
