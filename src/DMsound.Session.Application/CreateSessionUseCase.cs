using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;

namespace DMsound.Session.Application;

public sealed class CreateSessionUseCase
{
    private readonly ISessionCodeGenerator codeGenerator;
    private readonly ISessionRepository repository;

    public CreateSessionUseCase(ISessionCodeGenerator codeGenerator, ISessionRepository repository)
    {
        this.codeGenerator = codeGenerator;
        this.repository = repository;
    }

    public async Task<SessionCreationResult> ExecuteAsync(UserId hostUserId, string displayName, CancellationToken cancellationToken)
    {
        var sessionId = SessionId.New();
        var code = new SessionCode(codeGenerator.Generate());
        var session = global::DMsound.Session.Domain.Session.CreateHost(
            sessionId,
            code,
            hostUserId,
            displayName);

        await repository.SaveAsync(session, cancellationToken);

        return new SessionCreationResult(sessionId, code.Value);
    }
}