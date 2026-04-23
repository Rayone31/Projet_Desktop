using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DMsound.Session.Application;
using DMsound.Session.Domain;

namespace DMsound.Session.Infrastructure.Network;

public sealed class SessionHostWebSocketServer : IDisposable
{
    private const string LanPrefixHost = "+";
    private const string LocalPrefixHost = "127.0.0.1";

    private readonly CreateSessionUseCase createSessionUseCase;
    private readonly HttpListener listener;
    private readonly JoinSessionByCodeUseCase joinSessionUseCase;
    private readonly SessionWebSocketSessionHub sessionHub;

    public SessionHostWebSocketServer(
        int port,
        CreateSessionUseCase createSessionUseCase,
        JoinSessionByCodeUseCase joinSessionUseCase,
        bool allowLanConnections = false,
        SessionWebSocketSessionHub? sessionHub = null)
    {
        this.createSessionUseCase = createSessionUseCase;
        this.joinSessionUseCase = joinSessionUseCase;
        this.sessionHub = sessionHub ?? new SessionWebSocketSessionHub();
        listener = new HttpListener();

        var hostPrefix = allowLanConnections ? LanPrefixHost : LocalPrefixHost;
        listener.Prefixes.Add($"http://{hostPrefix}:{port}/session/");
    }

    public void Dispose()
    {
        if (listener.IsListening)
        {
            listener.Stop();
        }

        listener.Close();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext? context = null;

            try
            {
                context = await listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            if (context is null)
            {
                continue;
            }

            _ = HandleContextAsync(context, cancellationToken);
        }
    }

    private async Task HandleContextAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        if (!context.Request.IsWebSocketRequest)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
            return;
        }

        var webSocketContext = await context.AcceptWebSocketAsync(null);
        await HandleWebSocketAsync(webSocketContext.WebSocket, cancellationToken);
    }

    private async Task HandleWebSocketAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        try
        {
            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveTextMessageAsync(socket, cancellationToken);
                if (message is null)
                {
                    break;
                }

                await HandleMessageAsync(socket, message, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            sessionHub.Remove(socket);

            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }

    private async Task HandleMessageAsync(WebSocket socket, string message, CancellationToken cancellationToken)
    {
        var envelope = SessionWebSocketJson.DeserializeEnvelope(message);

        if (envelope.Type == SessionWebSocketMessageTypes.HostSession)
        {
            await HandleHostSessionAsync(socket, envelope.Payload, cancellationToken);
            return;
        }

        if (envelope.Type == SessionWebSocketMessageTypes.JoinSession)
        {
            await HandleJoinSessionAsync(socket, envelope.Payload, cancellationToken);
            return;
        }

        await SendErrorAsync(socket, "unsupported_message", "Unsupported message type.", cancellationToken);
    }

    private async Task HandleHostSessionAsync(WebSocket socket, JsonElement payload, CancellationToken cancellationToken)
    {
        var request = SessionWebSocketJson.DeserializePayload<HostSessionRequest>(payload);
        var hostUserId = UserId.From(Guid.Parse(request.UserId));
        var result = await createSessionUseCase.ExecuteAsync(hostUserId, request.DisplayName, cancellationToken);

        sessionHub.Add(result.Code, socket);
        await SendAsync(socket, SessionWebSocketMessageTypes.HostSessionAccepted, new HostSessionResponse(result.SessionId.ToString(), result.Code), cancellationToken);
    }

    private async Task HandleJoinSessionAsync(WebSocket socket, JsonElement payload, CancellationToken cancellationToken)
    {
        try
        {
            var request = SessionWebSocketJson.DeserializePayload<JoinSessionRequest>(payload);
            var sessionCode = new SessionCode(request.SessionCode);
            var userId = UserId.From(Guid.Parse(request.UserId));
            var result = await joinSessionUseCase.ExecuteAsync(sessionCode, userId, request.DisplayName, cancellationToken);

            sessionHub.Add(sessionCode.Value, socket);
            await SendAsync(socket, SessionWebSocketMessageTypes.JoinSessionAccepted, new JoinSessionResponse(result.SessionId.ToString(), result.Code.Value, result.MembersCount), cancellationToken);
            await BroadcastMemberJoinedAsync(sessionCode.Value, request, cancellationToken);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or InvalidOperationException)
        {
            await SendErrorAsync(socket, "join_rejected", exception.Message, cancellationToken);
        }
    }

    private Task BroadcastMemberJoinedAsync(
        string sessionCode,
        JoinSessionRequest request,
        CancellationToken cancellationToken)
    {
        var notification = new MemberJoinedNotification(sessionCode, request.UserId, request.DisplayName);
        return sessionHub.BroadcastAsync(
            sessionCode,
            SessionWebSocketJson.Serialize(SessionWebSocketMessageTypes.MemberJoined, notification),
            cancellationToken);
    }

    private static async Task<string?> ReceiveTextMessageAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var builder = new StringBuilder();

        while (true)
        {
            WebSocketReceiveResult result;

            try
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return null;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (result.EndOfMessage)
            {
                return builder.ToString();
            }
        }
    }

    private static Task SendAsync<T>(WebSocket socket, string type, T payload, CancellationToken cancellationToken)
    {
        var message = SessionWebSocketJson.Serialize(type, payload);
        var buffer = Encoding.UTF8.GetBytes(message);
        return socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static Task SendErrorAsync(
        WebSocket socket,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        return SendAsync(socket, SessionWebSocketMessageTypes.Error, new SessionErrorResponse(code, message), cancellationToken);
    }
}