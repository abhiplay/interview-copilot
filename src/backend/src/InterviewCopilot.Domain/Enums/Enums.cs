namespace InterviewCopilot.Domain.Enums;

/// <summary>What kind of conversation a session covers (FR-01).</summary>
public enum SessionMode
{
    LiveInterview = 0,
    MockInterview = 1,
    Conversation = 2,
}

/// <summary>How long suggested answers should be (FR-03).</summary>
#pragma warning disable CA1720 // Short/Long are the user-facing names agreed in FR-03, not type names.
public enum AnswerLength
{
    Short = 0,
    Balanced = 1,
    Long = 2,
}
#pragma warning restore CA1720

/// <summary>Structure applied to suggested answers (FR-04).</summary>
public enum AnswerFramework
{
    None = 0,
    Star = 1,
    Smart = 2,
}

/// <summary>Question categories the copilot detects or the user pins (FR-05, FR-08).</summary>
public enum QuestionType
{
    Behavioral = 0,
    HowDoYou = 1,
    Situational = 2,
    TechnicalKnowledge = 3,
    SystemDesign = 4,
    Coding = 5,
    TellMeAboutYourself = 6,
    Experience = 7,
}

/// <summary>Who said a transcript segment. Comes from the audio source, not diarization.</summary>
public enum Speaker
{
    Interviewer = 0,
    Candidate = 1,
}

public enum SessionStatus
{
    Draft = 0,
    Live = 1,
    Ended = 2,
}

/// <summary>Variation requested with the Regenerate / Shorter / Longer buttons (FR-10).</summary>
public enum AnswerModifier
{
    None = 0,
    Regenerate = 1,
    Shorter = 2,
    Longer = 3,
}
