using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Integration.Grpc;
using Nalam360.Platform.Integration.Storage;

namespace Nalam360.Platform.Integration.DependencyInjection;

/// <summary>
/// Extension methods for registering integration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Blob Storage service to the service collection.
    /// </summary>
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageOptions>(options =>
            configuration.GetSection("Storage:AzureBlob").Bind(options));

        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }

    /// <summary>
    /// Adds Azure Blob Storage service to the service collection with options.
    /// </summary>
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        Action<AzureBlobStorageOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }

    /// <summary>
    /// Adds AWS S3 storage service to the service collection.
    /// </summary>
    public static IServiceCollection AddAwsS3Storage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AwsS3Options>(options =>
            configuration.GetSection("Storage:AwsS3").Bind(options));

        services.AddSingleton<IBlobStorageService, AwsS3StorageService>();

        return services;
    }

    /// <summary>
    /// Adds AWS S3 storage service to the service collection with options.
    /// </summary>
    public static IServiceCollection AddAwsS3Storage(
        this IServiceCollection services,
        Action<AwsS3Options> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IBlobStorageService, AwsS3StorageService>();

        return services;
    }

    /// <summary>
    /// Adds Google Cloud Storage service to the service collection.
    /// </summary>
    public static IServiceCollection AddGcpStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GcpStorageOptions>(options =>
            configuration.GetSection("Storage:Gcp").Bind(options));

        services.AddSingleton<IBlobStorageService, GcpStorageService>();

        return services;
    }

    /// <summary>
    /// Adds Google Cloud Storage service to the service collection with options.
    /// </summary>
    public static IServiceCollection AddGcpStorage(
        this IServiceCollection services,
        Action<GcpStorageOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IBlobStorageService, GcpStorageService>();

        return services;
    }

    /// <summary>
    /// Adds gRPC client configuration to the service collection.
    /// </summary>
    public static IServiceCollection AddGrpcClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GrpcClientOptions>(options =>
            configuration.GetSection("Grpc").Bind(options));

        return services;
    }
}
