using InterviewCopilot.Domain.Enums;

namespace InterviewCopilot.Application.Copilot;

public sealed record QuestionClassification(QuestionType Type, double Confidence);

/// <summary>
/// Fast keyword classifier for the eight question types (FR-08). Returns null when nothing matches,
/// so the pinned type (or a model-based classifier) decides.
/// </summary>
public static class QuestionClassifier
{
    private const double StrongMatch = 0.9;
    private const double WeakMatch = 0.6;

    // Order matters: more specific patterns first.
    private static readonly (QuestionType Type, double Confidence, string[] Phrases)[] Rules =
    [
        (QuestionType.TellMeAboutYourself, StrongMatch,
            ["tell me about yourself", "introduce yourself", "walk me through your background", "walk me through your resume"]),
        (QuestionType.Behavioral, StrongMatch,
            ["tell me about a time", "describe a time", "give me an example of a time", "give an example of when", "share a situation where", "a time when you"]),
        (QuestionType.Situational, StrongMatch,
            ["what would you do if", "how would you handle", "imagine that", "suppose you", "if you were"]),
        (QuestionType.SystemDesign, StrongMatch,
            ["design a", "design an", "how would you design", "architect a", "how would you scale", "system design"]),
        (QuestionType.Coding, StrongMatch,
            ["write a function", "write code", "implement a", "implement an", "time complexity", "reverse a", "algorithm to"]),
        (QuestionType.Experience, WeakMatch,
            ["your experience with", "have you worked with", "have you used", "in your last role", "in your previous role", "your current role"]),
        (QuestionType.HowDoYou, WeakMatch,
            ["how do you", "how would you approach", "what is your approach", "what's your process"]),
        (QuestionType.TechnicalKnowledge, WeakMatch,
            ["what is", "what's the difference", "difference between", "explain how", "explain what", "how does"]),
    ];

    public static QuestionClassification? Classify(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return null;
        }

        var lower = question.ToLowerInvariant();
        foreach (var (type, confidence, phrases) in Rules)
        {
            if (phrases.Any(phrase => lower.Contains(phrase, StringComparison.Ordinal)))
            {
                return new QuestionClassification(type, confidence);
            }
        }

        return null;
    }
}
