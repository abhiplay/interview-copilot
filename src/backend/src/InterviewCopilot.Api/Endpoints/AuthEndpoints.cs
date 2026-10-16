using System.Security.Claims;
using InterviewCopilot.Application.Auth;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;

namespace InterviewCopilot.Api.Endpoints;

public sealed record CredentialsRequest(string Username, string Password);

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (CredentialsRequest request, AccountService accounts, CancellationToken ct) =>
        {
            var account = await accounts.RegisterAsync(request.Username, request.Password, ct);
            return TypedResults.Created($"/api/auth/accounts/{account.Id}", account);
        });

        group.MapPost("/sign-in", async Task<Results<SignInHttpResult, UnauthorizedHttpResult>> (
            CredentialsRequest request,
            AccountService accounts,
            CancellationToken ct) =>
        {
            var account = await accounts.SignInAsync(request.Username, request.Password, ct);
            if (account is null)
            {
                return TypedResults.Unauthorized();
            }

            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.Username),
                ],
                BearerTokenDefaults.AuthenticationScheme);
            return TypedResults.SignIn(new ClaimsPrincipal(identity), authenticationScheme: BearerTokenDefaults.AuthenticationScheme);
        });
    }
}
