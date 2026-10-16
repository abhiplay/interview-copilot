using InterviewCopilot.Domain.Entities;

namespace InterviewCopilot.Application.Common.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Session>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);

    void Add(Session session);
}

public interface IUserAccountRepository
{
    Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken);

    void Add(UserAccount account);
}

public interface ITranscriptRepository
{
    Task<IReadOnlyList<TranscriptSegment>> GetSegmentsAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SuggestedAnswer>> GetAnswersAsync(Guid sessionId, CancellationToken cancellationToken);

    void AddSegment(TranscriptSegment segment);

    void AddAnswer(SuggestedAnswer answer);
}

public interface IUserPreferencesRepository
{
    Task<UserPreferences?> GetAsync(Guid userId, CancellationToken cancellationToken);

    void Add(UserPreferences preferences);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
