namespace InterviewCopilot.Domain;

/// <summary>Limits agreed in the requirements. Change only with the owner's OK.</summary>
public static class SessionLimits
{
    /// <summary>FR-02: up to 10 documents per session.</summary>
    public const int MaxDocumentsPerSession = 10;

    public const int MaxSessionPromptLength = 4_000;

    public const int MaxTitleLength = 200;

    public const int MaxFileNameLength = 255;
}
