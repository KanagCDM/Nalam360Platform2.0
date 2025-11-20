namespace Nalam360.Platform.Integration.Grpc;

/// <summary>
/// Configuration options for gRPC clients.
/// </summary>
public class GrpcClientOptions
{
    /// <summary>
    /// Gets or sets the gRPC server address.
    /// </summary>
    public string ServerAddress { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Gets or sets the maximum receive message size in bytes.
    /// </summary>
    public int? MaxReceiveMessageSize { get; set; } = 4 * 1024 * 1024; // 4 MB

    /// <summary>
    /// Gets or sets the maximum send message size in bytes.
    /// </summary>
    public int? MaxSendMessageSize { get; set; } = 4 * 1024 * 1024; // 4 MB
}
