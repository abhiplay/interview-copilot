using InterviewCopilot.Application.Common.Interfaces;
using InterviewCopilot.Application.Sessions;
using InterviewCopilot.Domain.Exceptions;

namespace InterviewCopilot.Application.Documents;

/// <summary>Extracts readable text from an uploaded file.</summary>
public interface IDocumentTextExtractor
{
    bool CanExtract(string fileName);

    Task<string> ExtractAsync(string fileName, Stream content, CancellationToken cancellationToken);
}

/// <summary>Attaches and removes the up-to-10 grounding documents of a session (FR-02).</summary>
public sealed class DocumentService(
    SessionService sessions,
    IEnumerable<IDocumentTextExtractor> extractors,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;

    public async Task<DocumentSummaryDto> AttachAsync(
        Guid ownerId,
        Guid sessionId,
        string fileName,
        Stream content,
        long length,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);
        if (length > MaxFileSizeBytes)
        {
            throw new DomainException($"'{fileName}' is larger than {MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var extractor = extractors.FirstOrDefault(e => e.CanExtract(fileName))
            ?? throw new DomainException($"'{fileName}' isn't a supported document type.");

        var session = await sessions.LoadOwnedAsync(ownerId, sessionId, cancellationToken);
        var text = await extractor.ExtractAsync(fileName, content, cancellationToken);
        var document = session.AttachDocument(fileName, text, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DocumentSummaryDto(document.Id, document.FileName, document.Content.Length, document.CreatedAt);
    }

    public async Task RemoveAsync(Guid ownerId, Guid sessionId, Guid documentId, CancellationToken cancellationToken)
    {
        var session = await sessions.LoadOwnedAsync(ownerId, sessionId, cancellationToken);
        session.RemoveDocument(documentId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
