using InterviewCopilot.Domain.Enums;
using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Domain.Entities;

/// <summary>One transcribed utterance, labelled by speaker (FR-07).</summary>
public sealed class TranscriptSegment
{
    private TranscriptSegment()
    {
        Text = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public Speaker Speaker { get; private set; }

    /// <summary>Milliseconds from session start.</summary>
    public long StartMs { get; private set; }

    public long EndMs { get; private set; }

    public string Text { get; private set; }

    public static TranscriptSegment Create(Guid sessionId, Speaker speaker, long startMs, long endMs, string text)
    {
        if (startMs < 0 || endMs < startMs)
        {
            throw new DomainException("A transcript segment must end after it starts.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A transcript segment needs text.");
        }

        return new TranscriptSegment
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Speaker = speaker,
            StartMs = startMs,
            EndMs = endMs,
            Text = text.Trim(),
        };
    }
}
