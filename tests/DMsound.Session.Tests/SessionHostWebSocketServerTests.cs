using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DMsound.Session.Application;
using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;
using DMsound.Session.Infrastructure;
using DMsound.Session.Infrastructure.Network;

namespace DMsound.Session.Tests;

public sealed class SessionHostWebSocketServerTests
{
    [Fact]
    public async Task Host_session_returns_a_code()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = CreateServer("A1B2C3D4", out var port);
        var serverTask = server.RunAsync(cancellationSource.Token);

        using var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"ws://localhost:{port}/session/"), cancellationSource.Token);

        await SendAsync(client, SessionWebSocketMessageTypes.HostSession, new HostSessionRequest(Guid.NewGuid().ToString(), "Host"), cancellationSource.Token);

        var response = await ReceiveAsync(client, cancellationSource.Token);
        var envelope = JsonSerializer.Deserialize<SessionWebSocketEnvelope>(response, JsonOptions())!;
        var payload = envelope.Payload.Deserialize<HostSessionResponse>(JsonOptions())!;

        Assert.Equal(SessionWebSocketMessageTypes.HostSessionAccepted, envelope.Type);
        Assert.Equal("A1B2C3D4", payload.SessionCode);

        cancellationSource.Cancel();
        server.Dispose();
        await serverTask;
    }

    [Fact]
    public async Task Join_session_notifies_host_and_returns_join_confirmation()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = CreateServer("A1B2C3D4", out var port);
        var serverTask = server.RunAsync(cancellationSource.Token);

        using var hostClient = new ClientWebSocket();
        await hostClient.ConnectAsync(new Uri($"ws://localhost:{port}/session/"), cancellationSource.Token);
        await SendAsync(hostClient, SessionWebSocketMessageTypes.HostSession, new HostSessionRequest(Guid.NewGuid().ToString(), "Host"), cancellationSource.Token);
        await ReceiveAsync(hostClient, cancellationSource.Token);

        using var guestClient = new ClientWebSocket();
        await guestClient.ConnectAsync(new Uri($"ws://localhost:{port}/session/"), cancellationSource.Token);
        var guestId = Guid.NewGuid().ToString();

        await SendAsync(
            guestClient,
            SessionWebSocketMessageTypes.JoinSession,
            new JoinSessionRequest("A1B2C3D4", guestId, "Guest"),
            cancellationSource.Token);

        var joinResponse = await ReceiveAsync(guestClient, cancellationSource.Token);
        var joinEnvelope = JsonSerializer.Deserialize<SessionWebSocketEnvelope>(joinResponse, JsonOptions())!;
        var joinPayload = joinEnvelope.Payload.Deserialize<JoinSessionResponse>(JsonOptions())!;

        var notification = await ReceiveAsync(hostClient, cancellationSource.Token);
        var notificationEnvelope = JsonSerializer.Deserialize<SessionWebSocketEnvelope>(notification, JsonOptions())!;
        var notificationPayload = notificationEnvelope.Payload.Deserialize<MemberJoinedNotification>(JsonOptions())!;

        Assert.Equal(SessionWebSocketMessageTypes.JoinSessionAccepted, joinEnvelope.Type);
        Assert.Equal(2, joinPayload.MembersCount);
        Assert.Equal(SessionWebSocketMessageTypes.MemberJoined, notificationEnvelope.Type);
        Assert.Equal(guestId, notificationPayload.UserId);
        Assert.Equal("Guest", notificationPayload.DisplayName);

        cancellationSource.Cancel();
        server.Dispose();
        await serverTask;
    }

    private static SessionHostWebSocketServer CreateServer(string code, out int port)
    {
        port = GetFreePort();
        var repository = new InMemorySessionRepository();
        var createSessionUseCase = new CreateSessionUseCase(new FixedCodeGenerator(code), repository);
        var joinSessionUseCase = new JoinSessionByCodeUseCase(repository);

        return new SessionHostWebSocketServer(port, createSessionUseCase, joinSessionUseCase);
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task SendAsync<T>(
        ClientWebSocket socket,
        string type,
        T payload,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(new { type, payload }, JsonOptions());
        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static async Task<string> ReceiveAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var builder = new StringBuilder();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new InvalidOperationException("WebSocket closed unexpectedly.");
            }

            builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (result.EndOfMessage)
            {
                return builder.ToString();
            }
        }
    }

    private sealed class FixedCodeGenerator : ISessionCodeGenerator
    {
        private readonly string code;

        public FixedCodeGenerator(string code)
        {
            this.code = code;
        }

        public string Generate() => code;
    }
}