using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Domain.Entities;

/// <summary>A scored answer in mock interview mode (FR-11).</summary>
public sealed class PracticeAttempt
{
    public const int MinScore = 1;
    public const int MaxScore = 10;

    private PracticeAttempt()
    {
        Question = string.Empty;
        AnswerText = string.Empty;
        Feedback = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public string Question { get; private set; }

    public string AnswerText { get; private set; }

    public int Score { get; private set; }

    public string Feedback { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static PracticeAttempt Create(
        Guid sessionId,
        string question,
        string answerText,
        int score,
        string feedback,
        DateTimeOffset now)
    {
        if (score is < MinScore or > MaxScore)
        {
            throw new DomainException($"Scores must be between {MinScore} and {MaxScore}.");
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new DomainException("A practice attempt needs the question that was asked.");
        }

        return new PracticeAttempt
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Question = question.Trim(),
            AnswerText = answerText.Trim(),
            Score = score,
            Feedback = feedback.Trim(),
            CreatedAt = now,
        };
    }
}
