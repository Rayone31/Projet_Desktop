using DMsound.Session.Domain;

namespace DMsound.Session.Application;

public sealed record SessionJoinResult(SessionId SessionId, SessionCode Code, int MembersCount);