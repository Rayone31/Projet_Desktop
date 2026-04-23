using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;

namespace DMsound.Session.Infrastructure;

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly Dictionary<string, global::DMsound.Session.Domain.Session> sessions =
        new(StringComparer.OrdinalIgnoreCase);

    public Task SaveAsync(global::DMsound.Session.Domain.Session session, CancellationToken cancellationToken)
    {
        sessions[session.Code.Value] = session;
        return Task.CompletedTask;
    }

    public Task<global::DMsound.Session.Domain.Session?> GetByCodeAsync(
        SessionCode code,
        CancellationToken cancellationToken)
    {
        sessions.TryGetValue(code.Value, out var session);
        return Task.FromResult(session);
    }
}