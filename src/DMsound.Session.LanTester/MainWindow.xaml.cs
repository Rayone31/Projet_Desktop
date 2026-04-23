using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using DMsound.Session.Application;
using DMsound.Session.Application.Ports;
using DMsound.Session.Domain;
using DMsound.Session.Infrastructure;
using DMsound.Session.Infrastructure.Network;

namespace DMsound.Session.LanTester;

public partial class MainWindow : Window
{
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private CancellationTokenSource? hostCancellationSource;
    private ClientWebSocket? hostClientWebSocket;
    private Task? hostListenTask;
    private SessionHostWebSocketServer? hostServer;
    private ClientWebSocket? joinClientWebSocket;
    private Task? joinListenTask;

    public MainWindow()
    {
        InitializeComponent();
        HostIpTextBox.Text = GetLocalIPv4Address() ?? "IP LAN non detectee";
        Log("Pret. Tu peux hoster ou rejoindre une session.");
    }

    private async void HostSessionButton_OnClick(object sender, RoutedEventArgs e)
    {
        HostSessionButton.IsEnabled = false;

        try
        {
            var hostName = HostNameTextBox.Text.Trim();
            if (!int.TryParse(HostPortTextBox.Text.Trim(), out var port) || port <= 0)
            {
                throw new InvalidOperationException("Port host invalide.");
            }

            await StopHostResourcesAsync();

            var repository = new InMemorySessionRepository();
            var createUseCase = new CreateSessionUseCase(new DefaultSessionCodeGenerator(), repository);
            var joinUseCase = new JoinSessionByCodeUseCase(repository);
            var allowLan = AllowLanCheckBox.IsChecked == true;

            hostCancellationSource = new CancellationTokenSource();
            hostServer = new SessionHostWebSocketServer(port, createUseCase, joinUseCase, allowLan);
            _ = hostServer.RunAsync(hostCancellationSource.Token);

            hostClientWebSocket = new ClientWebSocket();
            await ConnectWithRetryAsync(hostClientWebSocket, $"ws://localhost:{port}/session/", hostCancellationSource.Token);

            var hostId = Guid.NewGuid().ToString();
            await SendAsync(
                hostClientWebSocket,
                SessionWebSocketMessageTypes.HostSession,
                new HostSessionRequest(hostId, string.IsNullOrWhiteSpace(hostName) ? "Host" : hostName),
                hostCancellationSource.Token);

            var rawResponse = await ReceiveAsync(hostClientWebSocket, hostCancellationSource.Token);
            var envelope = JsonSerializer.Deserialize<SessionWebSocketEnvelope>(rawResponse, jsonOptions)
                ?? throw new InvalidOperationException("Reponse host invalide.");

            if (envelope.Type != SessionWebSocketMessageTypes.HostSessionAccepted)
            {
                var error = envelope.Payload.Deserialize<SessionErrorResponse>(jsonOptions);
                throw new InvalidOperationException(error?.Message ?? "Creation session refusee.");
            }

            var hostResponse = envelope.Payload.Deserialize<HostSessionResponse>(jsonOptions)
                ?? throw new InvalidOperationException("Payload host invalide.");

            HostSessionCodeTextBox.Text = hostResponse.SessionCode;
            JoinSessionCodeTextBox.Text = hostResponse.SessionCode;
            JoinHostPortTextBox.Text = port.ToString();
            HostIpTextBox.Text = GetLocalIPv4Address() ?? "IP LAN non detectee";
            JoinHostIpTextBox.Text = HostIpTextBox.Text;

            Log($"Session creee. Code: {hostResponse.SessionCode}. Port: {port}. LAN: {(allowLan ? "oui" : "non")}");
            hostListenTask = ListenLoopAsync(hostClientWebSocket, "HOST", hostCancellationSource.Token);
        }
        catch (Exception exception)
        {
            Log($"Erreur host: {exception.Message}");
        }
        finally
        {
            HostSessionButton.IsEnabled = true;
        }
    }

    private async void JoinSessionButton_OnClick(object sender, RoutedEventArgs e)
    {
        JoinSessionButton.IsEnabled = false;

        try
        {
            var hostIp = JoinHostIpTextBox.Text.Trim();
            var joinName = JoinNameTextBox.Text.Trim();
            var sessionCode = JoinSessionCodeTextBox.Text.Trim().ToUpperInvariant();

            if (!int.TryParse(JoinHostPortTextBox.Text.Trim(), out var port) || port <= 0)
            {
                throw new InvalidOperationException("Port join invalide.");
            }

            if (!SessionCode.IsValid(sessionCode))
            {
                throw new InvalidOperationException("Code session invalide (8 caracteres alphanumeriques)." );
            }

            await StopJoinResourcesAsync();

            joinClientWebSocket = new ClientWebSocket();
            var cancellationToken = CancellationToken.None;
            await joinClientWebSocket.ConnectAsync(new Uri($"ws://{hostIp}:{port}/session/"), cancellationToken);

            var joinUserId = Guid.NewGuid().ToString();
            await SendAsync(
                joinClientWebSocket,
                SessionWebSocketMessageTypes.JoinSession,
                new JoinSessionRequest(sessionCode, joinUserId, string.IsNullOrWhiteSpace(joinName) ? "Client" : joinName),
                cancellationToken);

            var rawResponse = await ReceiveAsync(joinClientWebSocket, cancellationToken);
            var envelope = JsonSerializer.Deserialize<SessionWebSocketEnvelope>(rawResponse, jsonOptions)
                ?? throw new InvalidOperationException("Reponse join invalide.");

            if (envelope.Type == SessionWebSocketMessageTypes.Error)
            {
                var error = envelope.Payload.Deserialize<SessionErrorResponse>(jsonOptions);
                throw new InvalidOperationException(error?.Message ?? "Join refuse.");
            }

            if (envelope.Type != SessionWebSocketMessageTypes.JoinSessionAccepted)
            {
                throw new InvalidOperationException("Reponse inattendue au join.");
            }

            var joinResponse = envelope.Payload.Deserialize<JoinSessionResponse>(jsonOptions)
                ?? throw new InvalidOperationException("Payload join invalide.");

            JoinStatusTextBlock.Text = $"Statut: connecte ({joinResponse.MembersCount} membre(s))";
            Log($"Join ok. Session {joinResponse.SessionCode}. Membres: {joinResponse.MembersCount}.");

            joinListenTask = ListenLoopAsync(joinClientWebSocket, "JOIN", CancellationToken.None);
        }
        catch (Exception exception)
        {
            JoinStatusTextBlock.Text = "Statut: erreur";
            Log($"Erreur join: {exception.Message}");
        }
        finally
        {
            JoinSessionButton.IsEnabled = true;
        }
    }

    protected override async void OnClosed(EventArgs e)
    {
        await StopJoinResourcesAsync();
        await StopHostResourcesAsync();
        base.OnClosed(e);
    }

    private async Task StopHostResourcesAsync()
    {
        try
        {
            hostCancellationSource?.Cancel();
            if (hostClientWebSocket is not null)
            {
                await CloseSocketSafeAsync(hostClientWebSocket);
                hostClientWebSocket.Dispose();
                hostClientWebSocket = null;
            }

            hostServer?.Dispose();
            hostServer = null;

            if (hostListenTask is not null)
            {
                await hostListenTask;
                hostListenTask = null;
            }
        }
        catch
        {
        }
        finally
        {
            hostCancellationSource?.Dispose();
            hostCancellationSource = null;
        }
    }

    private async Task StopJoinResourcesAsync()
    {
        try
        {
            if (joinClientWebSocket is not null)
            {
                await CloseSocketSafeAsync(joinClientWebSocket);
                joinClientWebSocket.Dispose();
                joinClientWebSocket = null;
            }

            if (joinListenTask is not null)
            {
                await joinListenTask;
                joinListenTask = null;
            }
        }
        catch
        {
        }
    }

    private async Task ListenLoopAsync(ClientWebSocket socket, string source, CancellationToken cancellationToken)
    {
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var rawMessage = await ReceiveAsync(socket, cancellationToken);
                var envelope = JsonSerializer.Deserialize<SessionWebSocketEnvelope>(rawMessage, jsonOptions);
                if (envelope is null)
                {
                    continue;
                }

                if (envelope.Type == SessionWebSocketMessageTypes.MemberJoined)
                {
                    var payload = envelope.Payload.Deserialize<MemberJoinedNotification>(jsonOptions);
                    if (payload is not null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                            Log($"[{source}] {payload.DisplayName} a rejoint la session {payload.SessionCode}."));
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private async Task ConnectWithRetryAsync(ClientWebSocket socket, string url, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await socket.ConnectAsync(new Uri(url), cancellationToken);
                return;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private static async Task SendAsync<T>(
        ClientWebSocket socket,
        string type,
        T payload,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(new { type, payload });
        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static async Task<string> ReceiveAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var bytes = new byte[4096];
        var builder = new StringBuilder();

        while (true)
        {
            var result = await socket.ReceiveAsync(bytes, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new InvalidOperationException("Socket fermee.");
            }

            builder.Append(Encoding.UTF8.GetString(bytes, 0, result.Count));

            if (result.EndOfMessage)
            {
                return builder.ToString();
            }
        }
    }

    private static async Task CloseSocketSafeAsync(ClientWebSocket socket)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }

    private void Log(string message)
    {
        LogsListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private static string? GetLocalIPv4Address()
    {
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            var properties = networkInterface.GetIPProperties();
            foreach (var unicastAddress in properties.UnicastAddresses)
            {
                if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork
                    && !IPAddress.IsLoopback(unicastAddress.Address))
                {
                    return unicastAddress.Address.ToString();
                }
            }
        }

        return null;
    }
}