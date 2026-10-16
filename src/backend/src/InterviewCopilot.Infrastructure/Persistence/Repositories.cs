using InterviewCopilot.Application.Common.Interfaces;
using InterviewCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewCopilot.Infrastructure.Persistence;

internal sealed class SessionRepository(InterviewCopilotDbContext db) : ISessionRepository
{
    public Task<Session?> GetAsync(Guid sessionId, CancellationToken cancellationToken) =>
        db.Sessions.Include(s => s.Documents).FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

    public async Task<IReadOnlyList<Session>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken) =>
        await db.Sessions
            .Include(s => s.Documents)
            .Where(s => s.OwnerId == ownerId)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public void Add(Session session) => db.Sessions.Add(session);
}

internal sealed class UserAccountRepository(InterviewCopilotDbContext db) : IUserAccountRepository
{
    public Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken) =>
        db.UserAccounts.FirstOrDefaultAsync(a => a.Username == username, cancellationToken);

    public void Add(UserAccount account) => db.UserAccounts.Add(account);
}

internal sealed class TranscriptRepository(InterviewCopilotDbContext db) : ITranscriptRepository
{
    public async Task<IReadOnlyList<TranscriptSegment>> GetSegmentsAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await db.TranscriptSegments
            .Where(t => t.SessionId == sessionId)
            .OrderBy(t => t.StartMs)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SuggestedAnswer>> GetAnswersAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await db.SuggestedAnswers
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public void AddSegment(TranscriptSegment segment) => db.TranscriptSegments.Add(segment);

    public void AddAnswer(SuggestedAnswer answer) => db.SuggestedAnswers.Add(answer);
}
