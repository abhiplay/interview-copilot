using System.Security.Claims;

namespace InterviewCopilot.Api.Infrastructure;

internal static class UserContext
{
    /// <summary>The signed-in local account's id (NameIdentifier claim).</summary>
    public static Guid UserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessException("No signed-in user.");
}
