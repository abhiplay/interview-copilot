using Microsoft.AspNetCore.Identity;
using AppPasswordHasher = InterviewCopilot.Application.Auth.IPasswordHasher;

namespace InterviewCopilot.Infrastructure.Security;

/// <summary>PBKDF2 hashing from ASP.NET Core Identity (NFR-04).</summary>
internal sealed class AspNetPasswordHasher : AppPasswordHasher
{
    private static readonly object User = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(User, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(User, passwordHash, password) != PasswordVerificationResult.Failed;
}
