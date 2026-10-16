namespace InterviewCopilot.Application.Providers;

/// <summary>The model provider can't be reached or rejected the request. Message is user-facing.</summary>
public sealed class LlmUnavailableException : Exception
{
    public LlmUnavailableException()
    {
    }

    public LlmUnavailableException(string message)
        : base(message)
    {
    }

    public LlmUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Speech-to-text failed (model missing, provider disabled, bad key). Message is user-facing.</summary>
public sealed class TranscriptionUnavailableException : Exception
{
    public TranscriptionUnavailableException()
    {
    }

    public TranscriptionUnavailableException(string message)
        : base(message)
    {
    }

    public TranscriptionUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
