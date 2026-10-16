using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Domain.Entities;

/// <summary>A document the AI reads to ground its answers: resume, JD, sales script, product docs (FR-02).</summary>
public sealed class SessionDocument
{
    private SessionDocument()
    {
        FileName = string.Empty;
        Content = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public string FileName { get; private set; }

    /// <summary>Extracted plain text. Never logged.</summary>
    public string Content { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    internal static SessionDocument Create(Guid sessionId, string fileName, string content, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException("A document needs a file name.");
        }

        if (fileName.Length > SessionLimits.MaxFileNameLength)
        {
            throw new DomainException($"File names can be at most {SessionLimits.MaxFileNameLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException($"No readable text was found in '{fileName}'.");
        }

        return new SessionDocument
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            FileName = fileName.Trim(),
            Content = content,
            CreatedAt = now,
        };
    }
}
