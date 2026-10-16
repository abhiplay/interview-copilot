using InterviewCopilot.Application.Common.Interfaces;
using InterviewCopilot.Domain.Entities;
using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Application.Auth;

/// <summary>Hashes and verifies passwords (NFR-04). Never stores or logs the plain password.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}

public sealed record AccountDto(Guid Id, string Username);

/// <summary>Local accounts for v1 (FR-12). Mode C swaps in Google/GitHub sign-in behind the API.</summary>
public sealed class AccountService(
    IUserAccountRepository accounts,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public const int MinPasswordLength = 8;

    public async Task<AccountDto> RegisterAsync(string username, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
        {
            throw new DomainException($"Passwords must be at least {MinPasswordLength} characters.");
        }

        var existing = await accounts.FindByUsernameAsync(username?.Trim() ?? string.Empty, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException("That username is already taken.");
        }

        var account = UserAccount.Create(username ?? string.Empty, hasher.Hash(password), clock.GetUtcNow());
        accounts.Add(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AccountDto(account.Id, account.Username);
    }

    /// <summary>Returns the account, or null for a wrong username or password (same answer for both).</summary>
    public async Task<AccountDto?> SignInAsync(string username, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var account = await accounts.FindByUsernameAsync(username.Trim(), cancellationToken);
        if (account is null || !hasher.Verify(account.PasswordHash, password))
        {
            return null;
        }

        return new AccountDto(account.Id, account.Username);
    }
}
