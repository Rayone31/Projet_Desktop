namespace DMsound.Session.Domain;

public sealed record SessionMember(UserId UserId, string DisplayName, bool IsHost);