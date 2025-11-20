using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Messaging.Outbox;

/// <summary>
/// Background service for processing outbox messages.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IOutboxRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly OutboxOptions _options;

    public OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IOutboxRepository repository,
        IEventBus eventBus,
        IOptions<OutboxOptions> options)
    {
        _logger = logger;
        _repository = repository;
        _eventBus = eventBus;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
                await Task.Delay(_options.ProcessingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        var messages = await _repository.GetUnprocessedAsync(
            _options.BatchSize,
            cancellationToken);

        if (!messages.Any())
            return;

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            if (message.Attempts >= _options.MaxRetries)
            {
                _logger.LogWarning(
                    "Message {MessageId} exceeded max retries, marking as failed",
                    message.Id);

                await _repository.MarkAsFailedAsync(
                    message.Id,
                    "Exceeded maximum retry attempts",
                    cancellationToken);

                continue;
            }

            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    _logger.LogError(
                        "Could not resolve event type {EventType} for message {MessageId}",
                        message.EventType,
                        message.Id);

                    await _repository.MarkAsFailedAsync(
                        message.Id,
                        $"Unknown event type: {message.EventType}",
                        cancellationToken);

                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event is null)
                {
                    _logger.LogError(
                        "Failed to deserialize message {MessageId}",
                        message.Id);

                    await _repository.MarkAsFailedAsync(
                        message.Id,
                        "Deserialization failed",
                        cancellationToken);

                    continue;
                }

                // Use reflection to call PublishAsync<T>
                var publishMethod = _eventBus.GetType()
                    .GetMethod(nameof(IEventBus.PublishAsync))!
                    .MakeGenericMethod(eventType);

                var result = await (Task<Result>)publishMethod.Invoke(
                    _eventBus,
                    new[] { @event, cancellationToken })!;

                if (result.IsSuccess)
                {
                    await _repository.MarkAsProcessedAsync(message.Id, cancellationToken);
                    _logger.LogInformation("Message {MessageId} processed successfully", message.Id);
                }
                else
                {
                    await _repository.MarkAsFailedAsync(
                        message.Id,
                        result.Error!.Message,
                        cancellationToken);

                    _logger.LogError(
                        "Failed to publish message {MessageId}: {Error}",
                        message.Id,
                        result.Error.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.Id);
                await _repository.MarkAsFailedAsync(
                    message.Id,
                    ex.Message,
                    cancellationToken);
            }
        }

        // Cleanup old processed messages
        if (_options.CleanupEnabled)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-_options.CleanupAfterDays);
            await _repository.DeleteOldMessagesAsync(cutoffDate, cancellationToken);
        }
    }
}
