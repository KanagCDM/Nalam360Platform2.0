namespace Nalam360.Platform.Messaging.Outbox;

/// <summary>
/// Repository for managing outbox messages.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a new outbox message.
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unprocessed messages up to the specified limit.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed with error details.
    /// </summary>
    Task MarkAsFailedAsync(
        Guid messageId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes messages older than the specified date.
    /// </summary>
    Task DeleteOldMessagesAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
