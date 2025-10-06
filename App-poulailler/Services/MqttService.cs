using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text;

namespace App_poulailler.Services;

public class MqttService : IMqttService
{
    private IMqttClient? _client;
    private readonly string _host;
    private readonly int _port;
    private readonly string? _username;
    private readonly string? _password;
    private readonly SemaphoreSlim _connectionLock = new(1,1);

    public bool IsConnected => _client?.IsConnected ?? false;

    public event EventHandler<string>? MessageReceived;

    public MqttService(string host, int port = 1883, string? username = null, string? password = null)
    {
        _host = host;
        _port = port;
        _username = username;
        _password = password;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_client == null)
            {
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                _client.ApplicationMessageReceivedAsync += e =>
                {
                    try
                    {
                        var payload = e.ApplicationMessage?.Payload == null ? string.Empty : Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        MessageReceived?.Invoke(this, payload);
                    }
                    catch { }
                    return Task.CompletedTask;
                };

                _client.DisconnectedAsync += async e =>
                {
                    // Auto-reconnect simple
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    try { await ConnectInternalAsync(cancellationToken); } catch { }
                };
            }

            if (_client!.IsConnected)
                return true;

            return await ConnectInternalAsync(cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<bool> ConnectInternalAsync(CancellationToken cancellationToken)
    {
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_host, _port)
            .WithCleanSession();

        if (!string.IsNullOrEmpty(_username))
        {
            optionsBuilder = optionsBuilder.WithCredentials(_username, _password);
        }

        var options = optionsBuilder.Build();

        var result = await _client!.ConnectAsync(options, cancellationToken);
        return result.ResultCode == MqttClientConnectResultCode.Success;
    }

    public async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 1, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(retain);

        if (qos == 2) message = message.WithExactlyOnceQoS();
        else if (qos == 1) message = message.WithAtLeastOnceQoS();
        else message = message.WithAtMostOnceQoS();

        await _client!.PublishAsync(message.Build(), cancellationToken);
    }

    public async Task SubscribeAsync(string topic, int qos = 1, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var qosLevel = qos switch
        {
            2 => MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce,
            1 => MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
            _ => MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce
        };

        await _client!.SubscribeAsync(topic, qosLevel, cancellationToken: cancellationToken);
    }
}
