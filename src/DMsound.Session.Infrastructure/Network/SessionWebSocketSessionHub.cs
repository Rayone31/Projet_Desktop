using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace DMsound.Session.Infrastructure.Network;

public sealed class SessionWebSocketSessionHub
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<WebSocket, byte>> socketsByCode =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(string sessionCode, WebSocket socket)
    {
        var sockets = socketsByCode.GetOrAdd(sessionCode, _ => new ConcurrentDictionary<WebSocket, byte>());
        sockets[socket] = 0;
    }

    public void Remove(WebSocket socket)
    {
        foreach (var sockets in socketsByCode.Values)
        {
            sockets.TryRemove(socket, out _);
        }
    }

    public async Task BroadcastAsync(string sessionCode, string message, CancellationToken cancellationToken)
    {
        if (!socketsByCode.TryGetValue(sessionCode, out var sockets))
        {
            return;
        }

        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var socket in sockets.Keys)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
        }
    }
}