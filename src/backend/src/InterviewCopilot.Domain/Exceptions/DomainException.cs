namespace InterviewCopilot.Domain.Exceptions;

/// <summary>A business rule was violated. The API maps this to HTTP 400/409 ProblemDetails.</summary>
public class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
