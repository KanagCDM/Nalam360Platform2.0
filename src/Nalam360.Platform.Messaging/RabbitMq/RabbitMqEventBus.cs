using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using RabbitMQ.Client;

namespace Nalam360.Platform.Messaging.RabbitMq;

/// <summary>
/// RabbitMQ implementation of the event bus.
/// </summary>
public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private bool _disposed;

    public RabbitMqEventBus(
        ILogger<RabbitMqEventBus> logger,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation(
                "RabbitMQ connection established to {HostName}:{Port}",
                _options.HostName,
                _options.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        if (_channel is null || _connection is null || !_connection.IsOpen)
        {
            return Result.Failure(Error.Internal(
                "RabbitMQ.ConnectionClosed",
                "RabbitMQ connection is not available"));
        }

        try
        {
            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = eventName;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: eventName,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published event {EventName} to RabbitMQ exchange {Exchange}",
                eventName,
                _options.ExchangeName);

            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to RabbitMQ");
            return Result.Failure(Error.Internal(
                "RabbitMQ.PublishFailed",
                $"Failed to publish event: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        if (_channel is null || _connection is null || !_connection.IsOpen)
        {
            return Result.Failure(Error.Internal(
                "RabbitMQ.ConnectionClosed",
                "RabbitMQ connection is not available"));
        }

        try
        {
            var batch = _channel.CreateBasicPublishBatch();
            var eventName = typeof(TEvent).Name;

            foreach (var @event in events)
            {
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Type = eventName;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                batch.Add(
                    exchange: _options.ExchangeName,
                    routingKey: eventName,
                    mandatory: false,
                    properties: properties,
                    body: body);
            }

            batch.Publish();

            _logger.LogInformation(
                "Published {Count} events of type {EventName} to RabbitMQ",
                events.Count(),
                eventName);

            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch events to RabbitMQ");
            return Result.Failure(Error.Internal(
                "RabbitMQ.BatchPublishFailed",
                $"Failed to publish batch: {ex.Message}"));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
