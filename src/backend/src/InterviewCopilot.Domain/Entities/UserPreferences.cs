using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Domain.Entities;

/// <summary>Per-account choices stored on the backend so automatic answers can use them (#31).</summary>
public sealed class UserPreferences
{
    public const int MaxModelLength = 200;

    private UserPreferences()
    {
    }

    /// <summary>The account these preferences belong to (also the key).</summary>
    public Guid UserId { get; private set; }

    /// <summary>Ollama model name, e.g. "llama3.2:3b". Null means the configured default.</summary>
    public string? LlmModel { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static UserPreferences Create(Guid userId, DateTimeOffset now) =>
        new() { UserId = userId, UpdatedAt = now };

    public void SetLlmModel(string? model, DateTimeOffset now)
    {
        var trimmed = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        if (trimmed is { Length: > MaxModelLength })
        {
            throw new DomainException($"Model names can be at most {MaxModelLength} characters.");
        }

        LlmModel = trimmed;
        UpdatedAt = now;
    }
}
