using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;
using Nalam360Enterprise.UI.Core.AI.Services;

namespace Nalam360Enterprise.UI.Core.AI;

/// <summary>
/// Extension methods for registering AI services
/// </summary>
public static class AIServiceCollectionExtensions
{
    /// <summary>
    /// Adds AI services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNalam360AIServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register AI Service with HttpClient
        services.AddHttpClient<IAIService, AzureOpenAIService>(client =>
        {
            var endpoint = configuration["AI:AzureOpenAI:Endpoint"];
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                client.BaseAddress = new Uri(endpoint);
            }
            
            var apiKey = configuration["AI:AzureOpenAI:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("api-key", apiKey);
            }
            
            client.Timeout = TimeSpan.FromSeconds(120);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Register ML Model Service
        services.AddSingleton<IMLModelService, MLNetModelService>();
        
        // Register HIPAA Compliance Service
        services.AddScoped<IAIComplianceService, HIPAAComplianceService>();
        
        // Register Memory Cache for ML models
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Adds AI services with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureAI">Action to configure AI options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNalam360AIServices(
        this IServiceCollection services,
        Action<AIServiceOptions> configureAI)
    {
        var options = new AIServiceOptions();
        configureAI(options);

        // Register AI Service with HttpClient
        services.AddHttpClient<IAIService, AzureOpenAIService>(client =>
        {
            if (!string.IsNullOrWhiteSpace(options.AzureOpenAIEndpoint))
            {
                client.BaseAddress = new Uri(options.AzureOpenAIEndpoint);
            }
            
            if (!string.IsNullOrWhiteSpace(options.AzureOpenAIApiKey))
            {
                client.DefaultRequestHeaders.Add("api-key", options.AzureOpenAIApiKey);
            }
            
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy(options.MaxRetries))
        .AddPolicyHandler(GetCircuitBreakerPolicy(options.CircuitBreakerThreshold));

        // Register ML Model Service
        services.AddSingleton<IMLModelService, MLNetModelService>();
        
        // Register HIPAA Compliance Service
        services.AddScoped<IAIComplianceService, HIPAAComplianceService>();
        
        // Register Memory Cache
        services.AddMemoryCache();

        // Register options
        services.AddSingleton(options);

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetries = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"AI Service retry {retryCount} after {timespan.TotalSeconds}s delay");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int threshold = 5)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: threshold,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"AI Service circuit breaker opened for {timespan.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("AI Service circuit breaker reset");
                });
    }
}

/// <summary>
/// Configuration options for AI services
/// </summary>
public class AIServiceOptions
{
    /// <summary>
    /// Azure OpenAI endpoint URL
    /// </summary>
    public string? AzureOpenAIEndpoint { get; set; }

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string? AzureOpenAIApiKey { get; set; }

    /// <summary>
    /// Azure OpenAI deployment name
    /// </summary>
    public string? DeploymentName { get; set; } = "gpt-4";

    /// <summary>
    /// Timeout for AI requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Circuit breaker threshold
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// Enable caching of AI responses
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Enable HIPAA compliance checks
    /// </summary>
    public bool EnableHIPAACompliance { get; set; } = true;

    /// <summary>
    /// Allowed geographic regions for data residency
    /// </summary>
    public List<string> AllowedRegions { get; set; } = new() { "us", "usa", "united-states" };

    /// <summary>
    /// Enable automatic PHI de-identification
    /// </summary>
    public bool AutoDeIdentifyPHI { get; set; } = true;
}
