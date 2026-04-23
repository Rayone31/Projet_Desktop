using System.Text.Json;

namespace DMsound.Session.Infrastructure.Network;

internal static class SessionWebSocketJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<T>(string type, T payload)
    {
        return JsonSerializer.Serialize(new { type, payload }, Options);
    }

    public static SessionWebSocketEnvelope DeserializeEnvelope(string json)
    {
        return JsonSerializer.Deserialize<SessionWebSocketEnvelope>(json, Options)
            ?? throw new InvalidOperationException("Invalid WebSocket envelope.");
    }

    public static T DeserializePayload<T>(JsonElement payload)
    {
        return payload.Deserialize<T>(Options)
            ?? throw new InvalidOperationException("Invalid WebSocket payload.");
    }
}