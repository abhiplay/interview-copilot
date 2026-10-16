using InterviewCopilot.Domain.Entities;

namespace InterviewCopilot.Application.Copilot;

public sealed record DocumentExcerpt(string FileName, string Text);

/// <summary>
/// Picks the document passages most relevant to a question while staying inside a character budget,
/// so up to 10 large documents still fit a small local model's context (FR-02, FR-09).
/// Keyword overlap for v1; embeddings are an open decision for the owner.
/// </summary>
public static class DocumentContextSelector
{
    public const int ChunkSize = 800;
    public const int DefaultCharacterBudget = 6_000;

    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        "the", "a", "an", "and", "or", "of", "to", "in", "on", "for", "with", "is", "are", "was", "were",
        "you", "your", "me", "i", "my", "it", "that", "this", "what", "how", "why", "when", "do", "did",
        "can", "could", "would", "about", "tell", "time",
    };

    public static IReadOnlyList<DocumentExcerpt> Select(
        IEnumerable<SessionDocument> documents,
        string question,
        int characterBudget = DefaultCharacterBudget)
    {
        ArgumentNullException.ThrowIfNull(documents);
        var keywords = Tokenize(question ?? string.Empty);

        var ranked = documents
            .SelectMany(d => Chunk(d.Content).Select((text, index) => (d.FileName, Text: text, Index: index)))
            .Select(c => (c.FileName, c.Text, c.Index, Score: Score(c.Text, keywords)))
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Index);

        var selected = new List<DocumentExcerpt>();
        var used = 0;
        foreach (var chunk in ranked)
        {
            if (used + chunk.Text.Length > characterBudget)
            {
                continue;
            }

            selected.Add(new DocumentExcerpt(chunk.FileName, chunk.Text));
            used += chunk.Text.Length;
        }

        return selected;
    }

    internal static IEnumerable<string> Chunk(string content)
    {
        for (var start = 0; start < content.Length; start += ChunkSize)
        {
            yield return content.Substring(start, Math.Min(ChunkSize, content.Length - start));
        }
    }

    private static int Score(string text, HashSet<string> keywords)
    {
        var words = Tokenize(text);
        return keywords.Count(words.Contains);
    }

    private static HashSet<string> Tokenize(string text) =>
        text.ToLowerInvariant()
            .Split([' ', '\n', '\r', '\t', '.', ',', '?', '!', ';', ':', '(', ')', '"', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !StopWords.Contains(w))
            .ToHashSet(StringComparer.Ordinal);
}
