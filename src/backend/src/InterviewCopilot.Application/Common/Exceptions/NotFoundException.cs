namespace InterviewCopilot.Application.Common.Exceptions;

/// <summary>The requested resource doesn't exist (or isn't the caller's). Mapped to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
