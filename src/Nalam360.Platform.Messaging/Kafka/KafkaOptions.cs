namespace Nalam360.Platform.Messaging.Kafka;

/// <summary>
/// Configuration options for Kafka.
/// </summary>
public class KafkaOptions
{
    /// <summary>
    /// Gets or sets the Kafka bootstrap servers (comma-separated list).
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Gets or sets the topic prefix for events.
    /// </summary>
    public string TopicPrefix { get; set; } = "nalam360.events.";

    /// <summary>
    /// Gets or sets the consumer group ID.
    /// </summary>
    public string ConsumerGroupId { get; set; } = "nalam360-consumer-group";
}
