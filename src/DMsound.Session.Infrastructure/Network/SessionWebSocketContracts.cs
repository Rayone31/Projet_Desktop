using System.Text.Json;

namespace DMsound.Session.Infrastructure.Network;

public sealed record SessionWebSocketEnvelope(string Type, JsonElement Payload);

public sealed record HostSessionRequest(string UserId, string DisplayName);

public sealed record JoinSessionRequest(string SessionCode, string UserId, string DisplayName);

public sealed record HostSessionResponse(string SessionId, string SessionCode);

public sealed record JoinSessionResponse(string SessionId, string SessionCode, int MembersCount);

public sealed record MemberJoinedNotification(string SessionCode, string UserId, string DisplayName);

public sealed record SessionErrorResponse(string Code, string Message);