namespace DMsound.Session.Application.Ports;

public interface ISessionRepository
{
    Task SaveAsync(global::DMsound.Session.Domain.Session session, CancellationToken cancellationToken);

    Task<global::DMsound.Session.Domain.Session?> GetByCodeAsync(
        global::DMsound.Session.Domain.SessionCode code,
        CancellationToken cancellationToken);
}