namespace InterviewCopilot.Application.Copilot;

/// <summary>
/// Decides whether an interviewer utterance is a question worth answering (FR-08).
/// Heuristic and instant so it fits the 2-5 s latency budget; an LLM check can be layered on later.
/// </summary>
public static class QuestionDetector
{
    private const int MinimumWordCount = 3;

    private static readonly string[] QuestionOpeners =
    [
        "how", "what", "why", "when", "where", "which", "who", "can you", "could you", "would you",
        "will you", "do you", "did you", "have you", "are you", "is there", "tell me", "describe",
        "walk me through", "explain", "give me an example", "talk me through", "share",
    ];

    public static bool IsQuestion(string utterance)
    {
        if (string.IsNullOrWhiteSpace(utterance))
        {
            return false;
        }

        var text = utterance.Trim();
        if (text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < MinimumWordCount)
        {
            return false;
        }

        if (text.EndsWith('?'))
        {
            return true;
        }

        var lower = text.ToLowerInvariant();
        return QuestionOpeners.Any(opener => lower.StartsWith(opener + " ", StringComparison.Ordinal)
            || lower.Contains(". " + opener + " ", StringComparison.Ordinal));
    }
}
