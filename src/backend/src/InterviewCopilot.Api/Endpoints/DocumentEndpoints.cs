using System.Security.Claims;
using InterviewCopilot.Api.Infrastructure;
using InterviewCopilot.Application.Documents;

namespace InterviewCopilot.Api.Endpoints;

internal static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions/{sessionId:guid}/documents").WithTags("Documents").RequireAuthorization();

        // Bearer-token API (no cookies), so antiforgery tokens add nothing here.
        group.MapPost("/", async (Guid sessionId, IFormFile file, ClaimsPrincipal user, DocumentService documents, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var document = await documents.AttachAsync(user.UserId(), sessionId, file.FileName, stream, file.Length, ct);
            return TypedResults.Created($"/api/sessions/{sessionId}/documents/{document.Id}", document);
        })
        .DisableAntiforgery();

        group.MapDelete("/{documentId:guid}", async (
            Guid sessionId,
            Guid documentId,
            ClaimsPrincipal user,
            DocumentService documents,
            CancellationToken ct) =>
        {
            await documents.RemoveAsync(user.UserId(), sessionId, documentId, ct);
            return TypedResults.NoContent();
        });
    }
}
