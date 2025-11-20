namespace Nalam360.Platform.Messaging.AzureServiceBus;

/// <summary>
/// Configuration options for Azure Service Bus.
/// </summary>
public class AzureServiceBusOptions
{
    /// <summary>
    /// Gets or sets the Azure Service Bus connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the topic name for publishing events.
    /// </summary>
    public string TopicName { get; set; } = "nalam360-events";

    /// <summary>
    /// Gets or sets whether to use session IDs for messages.
    /// </summary>
    public bool UseSessionId { get; set; } = false;
}
