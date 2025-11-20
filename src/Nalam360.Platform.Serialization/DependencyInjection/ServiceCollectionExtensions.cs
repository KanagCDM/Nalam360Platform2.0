using Microsoft.Extensions.DependencyInjection;

namespace Nalam360.Platform.Serialization.DependencyInjection;

/// <summary>
/// Extension methods for registering serialization services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds JSON serializer to the service collection.
    /// </summary>
    public static IServiceCollection AddJsonSerializer(
        this IServiceCollection services)
    {
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        return services;
    }

    /// <summary>
    /// Adds XML serializer to the service collection.
    /// </summary>
    public static IServiceCollection AddXmlSerializer(
        this IServiceCollection services)
    {
        services.AddSingleton<IXmlSerializer, SystemXmlSerializer>();
        return services;
    }

    /// <summary>
    /// Adds Protobuf serializer to the service collection.
    /// </summary>
    public static IServiceCollection AddProtobufSerializer(
        this IServiceCollection services)
    {
        services.AddSingleton<IProtobufSerializer, GoogleProtobufSerializer>();
        return services;
    }

    /// <summary>
    /// Adds all serializers to the service collection.
    /// </summary>
    public static IServiceCollection AddSerializers(
        this IServiceCollection services)
    {
        services.AddJsonSerializer();
        services.AddXmlSerializer();
        services.AddProtobufSerializer();
        return services;
    }
}
