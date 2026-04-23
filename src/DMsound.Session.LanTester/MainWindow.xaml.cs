using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Diagnostics;
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
    private SessionHostWebSocketServer? hostServer;
    private Task? hostServerRunTask;
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
            var resolvedHostName = string.IsNullOrWhiteSpace(hostName) ? "Host" : hostName;
            Log($"Host init. Name={resolvedHostName}; Port={port}; LAN={allowLan}; LocalIPs={string.Join(",", GetLocalIPv4Addresses())}");

            hostCancellationSource = new CancellationTokenSource();
            hostServer = new SessionHostWebSocketServer(port, createUseCase, joinUseCase, allowLan);
            hostServer.MemberJoined += OnHostMemberJoined;
            Log($"HttpListener prefixes: {string.Join(" | ", hostServer.GetConfiguredPrefixes())}");
            await hostServer.StartAsync(hostCancellationSource.Token);
            hostServerRunTask = hostServer.RunAsync(hostCancellationSource.Token);

            var hostUserId = UserId.From(Guid.NewGuid());
            var hostResponse = await createUseCase.ExecuteAsync(hostUserId, resolvedHostName, hostCancellationSource.Token);

            HostSessionCodeTextBox.Text = hostResponse.Code;
            JoinSessionCodeTextBox.Text = hostResponse.Code;
            JoinHostPortTextBox.Text = port.ToString();
            HostIpTextBox.Text = GetLocalIPv4Address() ?? "IP LAN non detectee";
            JoinHostIpTextBox.Text = HostIpTextBox.Text;

            Log($"Session creee. Code: {hostResponse.Code}. Port: {port}. LAN: {(allowLan ? "oui" : "non")}");
        }
        catch (Exception exception)
        {
            Log($"Erreur host: {FormatExceptionForLog(exception)} | State={BuildHostStateForLog()}");
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

            var cancellationToken = CancellationToken.None;
            joinClientWebSocket = await CreateConnectedSocketWithRetryAsync(
                $"ws://{hostIp}:{port}/session/",
                cancellationToken,
                message => Log($"Join connect: {message}"));

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
            Log($"Erreur join: {FormatExceptionForLog(exception)}");
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

            if (hostServer is not null)
            {
                hostServer.MemberJoined -= OnHostMemberJoined;
            }

            hostServer?.Dispose();
            hostServer = null;

            if (hostServerRunTask is not null)
            {
                await hostServerRunTask;
            }
            hostServerRunTask = null;
        }
        catch (Exception exception)
        {
            Log($"Erreur stop host: {FormatExceptionForLog(exception)}");
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
            }

            if (joinListenTask is not null)
            {
                await joinListenTask;
            }

            joinClientWebSocket?.Dispose();
            joinClientWebSocket = null;
            joinListenTask = null;
        }
        catch (Exception exception)
        {
            Log($"Erreur stop join: {FormatExceptionForLog(exception)}");
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
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception exception)
        {
            Log($"Erreur loop {source}: {FormatExceptionForLog(exception)}");
        }
    }

    private static async Task<ClientWebSocket> CreateConnectedSocketWithRetryAsync(
        string url,
        CancellationToken cancellationToken,
        Action<string>? onAttemptFailure = null)
    {
        const int maxAttempts = 10;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var socket = new ClientWebSocket();

            try
            {
                await socket.ConnectAsync(new Uri(url), cancellationToken);
                return socket;
            }
            catch (Exception exception)
            {
                lastError = exception;
                onAttemptFailure?.Invoke(
                    $"attempt={attempt}/{maxAttempts}; url={url}; error={exception.GetType().Name}: {exception.Message}");
                socket.Dispose();

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken);
                }
            }
        }

        throw new InvalidOperationException(
            $"Connexion WebSocket impossible apres plusieurs tentatives vers {url}.",
            lastError);
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

    private void OnHostMemberJoined(MemberJoinedNotification notification)
    {
        _ = Dispatcher.InvokeAsync(() =>
            Log($"[HOST] {notification.DisplayName} a rejoint la session {notification.SessionCode}."));
    }

    private static string FormatExceptionForLog(Exception exception)
    {
        var builder = new StringBuilder();
        var current = exception;
        var level = 0;

        while (current is not null)
        {
            var prefix = level == 0 ? "" : $" -> inner[{level}] ";
            builder.Append(prefix)
                .Append(current.GetType().Name)
                .Append(": ")
                .Append(current.Message)
                .Append(" (HResult=")
                .Append(current.HResult)
                .Append(')');

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
            {
                var stack = current.StackTrace
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Take(4);
                builder.Append(" [stack: ")
                    .Append(string.Join(" || ", stack))
                    .Append(']');
            }

            current = current.InnerException;
            level++;

            if (current is not null)
            {
                builder.Append(';');
            }
        }

        return builder.ToString();
    }

    private string BuildHostStateForLog()
    {
        var process = Process.GetCurrentProcess();
        return $"pid={process.Id}; hostServer={(hostServer is null ? "null" : "set")}; " +
               $"hostRunTask={(hostServerRunTask is null ? "null" : hostServerRunTask.Status.ToString())}; " +
               $"hostCts={(hostCancellationSource is null ? "null" : "set")}";
    }

    private void Log(string message)
    {
        LogsListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private static string? GetLocalIPv4Address()
    {
        return GetLocalIPv4Addresses().FirstOrDefault();
    }

    private static IReadOnlyList<string> GetLocalIPv4Addresses()
    {
        var addresses = new List<string>();

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
                    addresses.Add(unicastAddress.Address.ToString());
                }
            }
        }

        return addresses;
    }
}