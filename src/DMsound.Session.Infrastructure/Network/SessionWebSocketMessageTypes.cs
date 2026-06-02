namespace DMsound.Session.Infrastructure.Network;

public static class SessionWebSocketMessageTypes
{
    public const string HostSession = "host-session";

    public const string JoinSession = "join-session";

    public const string HostSessionAccepted = "host-session-accepted";

    public const string JoinSessionAccepted = "join-session-accepted";

    public const string MemberJoined = "member-joined";

    public const string Error = "error";
}