using InterviewCopilot.Domain.Enums;

namespace InterviewCopilot.Application.Copilot;

/// <summary>
/// Combines the pinned question type with auto-detection (FR-05, FR-08): the pin applies until the
/// conversation clearly shifts, then auto-detection overrides it.
/// </summary>
public static class QuestionTypeResolver
{
    /// <summary>Detections at or above this confidence override a pinned type.</summary>
    public const double OverrideThreshold = 0.8;

    /// <summary>Used when nothing is pinned and nothing was detected.</summary>
    public const QuestionType Fallback = QuestionType.Experience;

    public static QuestionType Resolve(QuestionType? pinned, QuestionClassification? detected)
    {
        if (detected is null)
        {
            return pinned ?? Fallback;
        }

        if (pinned is null || pinned == detected.Type)
        {
            return detected.Type;
        }

        return detected.Confidence >= OverrideThreshold ? detected.Type : pinned.Value;
    }
}
