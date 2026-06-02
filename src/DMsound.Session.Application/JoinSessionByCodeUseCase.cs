using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;

namespace DMsound.Session.Application;

public sealed class JoinSessionByCodeUseCase
{
    private readonly ISessionRepository repository;

    public JoinSessionByCodeUseCase(ISessionRepository repository)
    {
        this.repository = repository;
    }

    public async Task<SessionJoinResult> ExecuteAsync(SessionCode code, UserId userId, string displayName, CancellationToken cancellationToken)
    {
        var session = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        session.Join(userId, displayName);
        await repository.SaveAsync(session, cancellationToken);

        return new SessionJoinResult(session.Id, session.Code, session.Members.Count);
    }
}