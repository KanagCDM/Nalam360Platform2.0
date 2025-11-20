using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nalam360.Platform.Integration.Grpc;

/// <summary>
/// Base class for gRPC client wrappers.
/// </summary>
/// <typeparam name="TClient">The gRPC client type.</typeparam>
public abstract class BaseGrpcClient<TClient> where TClient : class
{
    protected readonly ILogger Logger;
    protected readonly GrpcChannel Channel;
    protected readonly TClient Client;

    protected BaseGrpcClient(
        ILogger logger,
        IOptions<GrpcClientOptions> options,
        Func<GrpcChannel, TClient> clientFactory)
    {
        Logger = logger;
        var opts = options.Value;

        Channel = GrpcChannel.ForAddress(opts.ServerAddress, new GrpcChannelOptions
        {
            MaxReceiveMessageSize = opts.MaxReceiveMessageSize,
            MaxSendMessageSize = opts.MaxSendMessageSize,
            HttpHandler = new SocketsHttpHandler
            {
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            }
        });

        Client = clientFactory(Channel);

        Logger.LogInformation(
            "gRPC client initialized for {ClientType} at {Address}",
            typeof(TClient).Name,
            opts.ServerAddress);
    }

    /// <summary>
    /// Disposes the gRPC channel.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        await Channel.ShutdownAsync();
        Channel.Dispose();
        GC.SuppressFinalize(this);
    }
}
