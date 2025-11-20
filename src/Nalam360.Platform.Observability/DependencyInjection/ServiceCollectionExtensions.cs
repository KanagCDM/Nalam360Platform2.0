using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Observability.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nalam360.Platform.Observability.DependencyInjection;

/// <summary>
/// Extension methods for registering observability services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenTelemetry observability to the service collection.
    /// </summary>
    public static IServiceCollection AddPlatformObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenTelemetryOptions>(
            configuration.GetSection("Observability:OpenTelemetry"));

        var options = configuration
            .GetSection("Observability:OpenTelemetry")
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        // Add custom services
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<ITracingService, TracingService>();

        // Configure OpenTelemetry
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: options.ServiceName, serviceVersion: options.ServiceVersion);

        if (options.TracingEnabled)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource("Nalam360.Platform");

                    if (options.ExportToConsole)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                    {
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                        });
                    }
                });
        }

        if (options.MetricsEnabled)
        {
            services.AddOpenTelemetry()
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter("Nalam360.Platform");

                    if (options.ExportToConsole)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                    {
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                        });
                    }
                });
        }

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry observability to the service collection with options.
    /// </summary>
    public static IServiceCollection AddPlatformObservability(
        this IServiceCollection services,
        Action<OpenTelemetryOptions> configureOptions)
    {
        services.Configure(configureOptions);

        var options = new OpenTelemetryOptions();
        configureOptions(options);

        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddSingleton<ITracingService, TracingService>();

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: options.ServiceName, serviceVersion: options.ServiceVersion);

        if (options.TracingEnabled)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource("Nalam360.Platform");

                    if (options.ExportToConsole)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                    {
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                        });
                    }
                });
        }

        if (options.MetricsEnabled)
        {
            services.AddOpenTelemetry()
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter("Nalam360.Platform");

                    if (options.ExportToConsole)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                    {
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                        });
                    }
                });
        }

        return services;
    }
}
