using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus implementation of the event bus.
/// </summary>
public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
{
    private readonly ILogger<AzureServiceBusEventBus> _logger;
    private readonly AzureServiceBusOptions _options;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private bool _disposed;

    public AzureServiceBusEventBus(
        ILogger<AzureServiceBusEventBus> logger,
        IOptions<AzureServiceBusOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        _client = new ServiceBusClient(_options.ConnectionString);
        _sender = _client.CreateSender(_options.TopicName);

        _logger.LogInformation(
            "Azure Service Bus sender created for topic: {Topic}",
            _options.TopicName);
    }

    /// <inheritdoc/>
    public async Task<Result> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        try
        {
            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event);

            var serviceBusMessage = new ServiceBusMessage(message)
            {
                ContentType = "application/json",
                Subject = eventName,
                MessageId = Guid.NewGuid().ToString(),
                SessionId = _options.UseSessionId ? Guid.NewGuid().ToString() : null
            };

            serviceBusMessage.ApplicationProperties.Add("EventType", eventName);
            serviceBusMessage.ApplicationProperties.Add("Timestamp", DateTime.UtcNow);

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Published event {EventName} to Azure Service Bus topic {Topic}",
                eventName,
                _options.TopicName);

            return Result.Success();
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Azure Service Bus error: {Reason}", ex.Reason);
            return Result.Failure(Error.Internal(
                "ServiceBus.SendFailed",
                $"Failed to publish event to Service Bus: {ex.Reason}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to Azure Service Bus");
            return Result.Failure(Error.Internal(
                "ServiceBus.PublishFailed",
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
            var messages = new List<ServiceBusMessage>();

            foreach (var @event in events)
            {
                var message = JsonSerializer.Serialize(@event);
                var serviceBusMessage = new ServiceBusMessage(message)
                {
                    ContentType = "application/json",
                    Subject = eventName,
                    MessageId = Guid.NewGuid().ToString(),
                    SessionId = _options.UseSessionId ? Guid.NewGuid().ToString() : null
                };

                serviceBusMessage.ApplicationProperties.Add("EventType", eventName);
                serviceBusMessage.ApplicationProperties.Add("Timestamp", DateTime.UtcNow);

                messages.Add(serviceBusMessage);
            }

            await _sender.SendMessagesAsync(messages, cancellationToken);

            _logger.LogInformation(
                "Published {Count} events of type {EventName} to Azure Service Bus topic {Topic}",
                messages.Count,
                eventName,
                _options.TopicName);

            return Result.Success();
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Azure Service Bus batch error: {Reason}", ex.Reason);
            return Result.Failure(Error.Internal(
                "ServiceBus.BatchSendFailed",
                $"Failed to publish batch to Service Bus: {ex.Reason}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch events to Azure Service Bus");
            return Result.Failure(Error.Internal(
                "ServiceBus.BatchPublishFailed",
                $"Failed to publish batch: {ex.Message}"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _sender.CloseAsync();
        await _client.DisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
