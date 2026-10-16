using InterviewCopilot.Domain.Enums;
using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Domain.Entities;

/// <summary>An answer the copilot suggested for a detected question (FR-09, FR-10).</summary>
public sealed class SuggestedAnswer
{
    private SuggestedAnswer()
    {
        Question = string.Empty;
        Text = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public string Question { get; private set; }

    public QuestionType QuestionType { get; private set; }

    public string Text { get; private set; }

    /// <summary>1 for the first answer to a question; incremented by Regenerate/Shorter/Longer.</summary>
    public int Version { get; private set; }

    public AnswerModifier Modifier { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static SuggestedAnswer Create(
        Guid sessionId,
        string question,
        QuestionType questionType,
        string text,
        int version,
        AnswerModifier modifier,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new DomainException("A suggested answer needs the question it answers.");
        }

        if (version < 1)
        {
            throw new DomainException("Answer versions start at 1.");
        }

        return new SuggestedAnswer
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Question = question.Trim(),
            QuestionType = questionType,
            Text = text,
            Version = version,
            Modifier = modifier,
            CreatedAt = now,
        };
    }
}
