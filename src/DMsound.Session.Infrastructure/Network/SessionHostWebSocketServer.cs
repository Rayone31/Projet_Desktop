using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DMsound.Session.Application;
using DMsound.Session.Domain;

namespace DMsound.Session.Infrastructure.Network;

public sealed class SessionHostWebSocketServer : IDisposable
{
    private const string LoopbackIp = "127.0.0.1";
    private const string LocalhostName = "localhost";

    private readonly string[] configuredPrefixes;
    private readonly CreateSessionUseCase createSessionUseCase;
    private readonly object lifecycleSync = new();
    private readonly HttpListener listener;
    private volatile bool isDisposed;
    private readonly JoinSessionByCodeUseCase joinSessionUseCase;
    private readonly SessionWebSocketSessionHub sessionHub;

    public event Action<MemberJoinedNotification>? MemberJoined;

    public IReadOnlyList<string> GetConfiguredPrefixes()
    {
        return configuredPrefixes;
    }

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

        configuredPrefixes = BuildPrefixes(port, allowLanConnections)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var prefix in configuredPrefixes)
        {
            listener.Prefixes.Add(prefix);
        }
    }

    public void Dispose()
    {
        lock (lifecycleSync)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            if (listener.IsListening)
            {
                listener.Stop();
            }

            listener.Close();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            lock (lifecycleSync)
            {
                if (isDisposed)
                {
                    throw new InvalidOperationException("Le serveur host est deja arrete ou en cours d'arret.");
                }

                if (listener.IsListening)
                {
                    return Task.CompletedTask;
                }

                listener.Start();
            }
        }
        catch (HttpListenerException exception) when (exception.ErrorCode == 5)
        {
            throw new InvalidOperationException(BuildAccessDeniedMessage(), exception);
        }
        catch (HttpListenerException exception)
        {
            throw new InvalidOperationException(BuildListenerStartupFailureMessage(exception), exception);
        }
        catch (ObjectDisposedException exception)
        {
            throw new InvalidOperationException("Le serveur host a ete dispose pendant le demarrage.", exception);
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<string> BuildPrefixes(int port, bool allowLanConnections)
    {
        var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            BuildPrefix(LoopbackIp, port),
            BuildPrefix(LocalhostName, port)
        };

        if (!allowLanConnections)
        {
            return prefixes;
        }

        foreach (var address in GetLanIPv4Addresses())
        {
            prefixes.Add(BuildPrefix(address.ToString(), port));
        }

        return prefixes;
    }

    private static IEnumerable<IPAddress> GetLanIPv4Addresses()
    {
        IPAddress[] addresses;

        try
        {
            addresses = Dns.GetHostAddresses(Dns.GetHostName());
        }
        catch (SocketException)
        {
            return Array.Empty<IPAddress>();
        }

        return addresses.Where(address =>
            address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            && !IPAddress.IsLoopback(address));
    }

    private static string BuildPrefix(string host, int port)
    {
        return $"http://{host}:{port}/session/";
    }

    private string BuildAccessDeniedMessage()
    {
        var builder = new StringBuilder();
        builder.AppendLine("HttpListener access denied. URL ACL is missing for one or more prefixes.");
        builder.AppendLine("Run an elevated terminal and grant access:");

        foreach (var prefix in configuredPrefixes)
        {
            builder.AppendLine($"netsh http add urlacl url={prefix} user=Everyone");
        }

        return builder.ToString().TrimEnd();
    }

    private string BuildListenerStartupFailureMessage(HttpListenerException exception)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Echec du demarrage HttpListener (code={exception.ErrorCode}).");
        builder.AppendLine($"Detail: {exception.Message}");
        builder.AppendLine("Prefixes configures:");

        foreach (var prefix in configuredPrefixes)
        {
            builder.AppendLine(prefix);
        }

        return builder.ToString().TrimEnd();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (isDisposed)
        {
            return;
        }

        await StartAsync(cancellationToken);

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

        try
        {
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            await HandleWebSocketAsync(webSocketContext.WebSocket, cancellationToken);
        }
        catch (HttpListenerException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
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
        MemberJoined?.Invoke(notification);
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