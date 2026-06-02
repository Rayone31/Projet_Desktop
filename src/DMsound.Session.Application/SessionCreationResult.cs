using DMsound.Session.Domain;

namespace DMsound.Session.Application;

public sealed record SessionCreationResult(SessionId SessionId, string Code);