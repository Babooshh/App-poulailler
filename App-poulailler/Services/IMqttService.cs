namespace App_poulailler.Services;

public interface IMqttService
{
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task PublishAsync(string topic, string payload, bool retain = false, int qos = 1, CancellationToken cancellationToken = default);
    Task SubscribeAsync(string topic, int qos = 1, CancellationToken cancellationToken = default);
    bool IsConnected { get; }
    event EventHandler<string>? MessageReceived; // raw payload
}
