namespace DMsound.Session.Domain;

public sealed class Session
{
    private readonly List<SessionMember> members = new();

    private Session(SessionId id, SessionCode code, SessionMember host)
    {
        Id = id;
        Code = code;
        Host = host;
        members.Add(host);
    }

    public SessionId Id { get; }

    public SessionCode Code { get; }

    public SessionMember Host { get; }

    public SessionStatus Status { get; private set; } = SessionStatus.Active;

    public IReadOnlyCollection<SessionMember> Members => members.AsReadOnly();

    public static Session CreateHost(SessionId id, SessionCode code, UserId userId, string displayName)
    {
        return new Session(id, code, new SessionMember(userId, displayName, true));
    }

    public void Join(UserId userId, string displayName)
    {
        EnsureActive();

        if (members.Any(member => member.UserId == userId))
        {
            return;
        }

        members.Add(new SessionMember(userId, displayName, false));
    }

    public void Close()
    {
        Status = SessionStatus.Closed;
    }

    private void EnsureActive()
    {
        if (Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("Session is closed.");
        }
    }
}