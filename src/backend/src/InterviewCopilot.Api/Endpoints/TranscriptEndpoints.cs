using System.Security.Claims;
using InterviewCopilot.Api.Infrastructure;
using InterviewCopilot.Application.Transcripts;

namespace InterviewCopilot.Api.Endpoints;

internal static class TranscriptEndpoints
{
    public static void MapTranscriptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions/{sessionId:guid}/transcript").WithTags("Transcripts").RequireAuthorization();

        group.MapGet("/", (Guid sessionId, ClaimsPrincipal user, TranscriptService transcripts, CancellationToken ct) =>
            transcripts.GetAsync(user.UserId(), sessionId, ct));

        group.MapGet("/pdf", async (Guid sessionId, ClaimsPrincipal user, TranscriptService transcripts, CancellationToken ct) =>
        {
            var (fileName, content) = await transcripts.ExportPdfAsync(user.UserId(), sessionId, ct);
            return TypedResults.File(content, "application/pdf", fileName);
        });
    }
}
