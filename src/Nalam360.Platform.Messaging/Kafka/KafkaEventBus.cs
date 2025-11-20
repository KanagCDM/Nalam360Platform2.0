using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using CoreError = Nalam360.Platform.Core.Results.Error;

namespace Nalam360.Platform.Messaging.Kafka;

/// <summary>
/// Kafka implementation of the event bus.
/// </summary>
public class KafkaEventBus : IEventBus, IDisposable
{
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly KafkaOptions _options;
    private readonly IProducer<string, string> _producer;
    private bool _disposed;

    public KafkaEventBus(
        ILogger<KafkaEventBus> logger,
        IOptions<KafkaOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MaxInFlight = 5,
            MessageTimeoutMs = 10000,
            RequestTimeoutMs = 30000,
            LingerMs = 10,
            CompressionType = CompressionType.Snappy
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError("Kafka error: {Reason}", error.Reason);
            })
            .Build();

        _logger.LogInformation(
            "Kafka producer initialized with servers: {Servers}",
            _options.BootstrapServers);
    }

    /// <inheritdoc/>
    public async Task<Result> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        try
        {
            var eventName = @event.GetType().Name;
            var topic = _options.TopicPrefix + eventName;
            var message = JsonSerializer.Serialize(@event);

            var kafkaMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message,
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(eventName) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                }
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

            _logger.LogInformation(
                "Published event {EventName} to Kafka topic {Topic}, partition {Partition}, offset {Offset}",
                eventName,
                topic,
                result.Partition.Value,
                result.Offset.Value);

            return Result.Success();
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Kafka produce error: {Error}", ex.Error.Reason);
            return Result.Failure(CoreError.Internal(
                "Kafka.ProduceFailed",
                $"Failed to publish event to Kafka: {ex.Error.Reason}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to Kafka");
            return Result.Failure(CoreError.Internal(
                "Kafka.PublishFailed",
                $"Failed to publish event: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        try
        {
            var eventName = typeof(TEvent).Name;
            var topic = _options.TopicPrefix + eventName;
            var tasks = new List<Task<DeliveryResult<string, string>>>();

            foreach (var @event in events)
            {
                var message = JsonSerializer.Serialize(@event);
                var kafkaMessage = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = message,
                    Headers = new Headers
                    {
                        { "event-type", System.Text.Encoding.UTF8.GetBytes(eventName) },
                        { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                    }
                };

                tasks.Add(_producer.ProduceAsync(topic, kafkaMessage, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);

            _logger.LogInformation(
                "Published {Count} events of type {EventName} to Kafka topic {Topic}",
                results.Length,
                eventName,
                topic);

            return Result.Success();
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Kafka batch produce error: {Error}", ex.Error.Reason);
            return Result.Failure(CoreError.Internal(
                "Kafka.BatchProduceFailed",
                $"Failed to publish batch to Kafka: {ex.Error.Reason}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch events to Kafka");
            return Result.Failure(CoreError.Internal(
                "Kafka.BatchPublishFailed",
                $"Failed to publish batch: {ex.Message}"));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
