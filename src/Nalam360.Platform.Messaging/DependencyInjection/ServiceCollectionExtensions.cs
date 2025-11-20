using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Messaging.AzureServiceBus;
using Nalam360.Platform.Messaging.Idempotency;
using Nalam360.Platform.Messaging.Kafka;
using Nalam360.Platform.Messaging.Outbox;
using Nalam360.Platform.Messaging.RabbitMq;

namespace Nalam360.Platform.Messaging.DependencyInjection;

/// <summary>
/// Extension methods for registering messaging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ event bus to the service collection.
    /// </summary>
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(options =>
            configuration.GetSection("Messaging:RabbitMQ").Bind(options));

        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        return services;
    }

    /// <summary>
    /// Adds RabbitMQ event bus to the service collection with options.
    /// </summary>
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        Action<RabbitMqOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        return services;
    }

    /// <summary>
    /// Adds Kafka event bus to the service collection.
    /// </summary>
    public static IServiceCollection AddKafkaEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(options =>
            configuration.GetSection("Messaging:Kafka").Bind(options));

        services.AddSingleton<IEventBus, KafkaEventBus>();

        return services;
    }

    /// <summary>
    /// Adds Kafka event bus to the service collection with options.
    /// </summary>
    public static IServiceCollection AddKafkaEventBus(
        this IServiceCollection services,
        Action<KafkaOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IEventBus, KafkaEventBus>();

        return services;
    }

    /// <summary>
    /// Adds Azure Service Bus event bus to the service collection.
    /// </summary>
    public static IServiceCollection AddAzureServiceBusEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureServiceBusOptions>(options =>
            configuration.GetSection("Messaging:AzureServiceBus").Bind(options));

        services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

        return services;
    }

    /// <summary>
    /// Adds Azure Service Bus event bus to the service collection with options.
    /// </summary>
    public static IServiceCollection AddAzureServiceBusEventBus(
        this IServiceCollection services,
        Action<AzureServiceBusOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

        return services;
    }

    /// <summary>
    /// Adds outbox pattern support to the service collection.
    /// </summary>
    public static IServiceCollection AddOutboxPattern(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(options =>
            configuration.GetSection("Messaging:Outbox").Bind(options));

        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    /// <summary>
    /// Adds outbox pattern support to the service collection with options.
    /// </summary>
    public static IServiceCollection AddOutboxPattern(
        this IServiceCollection services,
        Action<OutboxOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

    /// <summary>
    /// Adds idempotency support to the service collection.
    /// </summary>
    public static IServiceCollection AddIdempotency(
        this IServiceCollection services)
    {
        services.AddScoped<IIdempotencyHandler, IdempotencyHandler>();

        return services;
    }
}
