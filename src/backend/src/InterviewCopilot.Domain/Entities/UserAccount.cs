using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Domain.Entities;

/// <summary>A local account (FR-12). Passwords are stored only as hashes (NFR-04).</summary>
public sealed class UserAccount
{
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 64;

    private UserAccount()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Username { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static UserAccount Create(string username, string passwordHash, DateTimeOffset now)
    {
        var normalized = username?.Trim() ?? string.Empty;
        if (normalized.Length is < MinUsernameLength or > MaxUsernameLength)
        {
            throw new DomainException(
                $"Usernames must be {MinUsernameLength}-{MaxUsernameLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("A password hash is required.");
        }

        return new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = normalized,
            PasswordHash = passwordHash,
            CreatedAt = now,
        };
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("A password hash is required.");
        }

        PasswordHash = passwordHash;
    }
}
