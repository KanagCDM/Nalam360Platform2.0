namespace Nalam360.Platform.Messaging.RabbitMq;

/// <summary>
/// Configuration options for RabbitMQ.
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// Gets or sets the RabbitMQ host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the RabbitMQ port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets or sets the RabbitMQ username.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the RabbitMQ password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets or sets the RabbitMQ virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string ExchangeName { get; set; } = "nalam360.events";
}
