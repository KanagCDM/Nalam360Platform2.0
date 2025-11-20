using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Services;

namespace Nalam360Enterprise.UI.Core.AI;

/// <summary>
/// Extension methods for registering AI services
/// </summary>
public static class AIServiceCollectionExtensions
{
    /// <summary>
    /// Registers AI services with the dependency injection container
    /// </summary>
    public static IServiceCollection AddNalam360AIServices(
        this IServiceCollection services,
        string azureOpenAIEndpoint,
        string azureOpenAIApiKey,
        string deploymentName = "gpt-4")
    {
        if (string.IsNullOrWhiteSpace(azureOpenAIEndpoint))
            throw new ArgumentException("Azure OpenAI endpoint is required", nameof(azureOpenAIEndpoint));

        if (string.IsNullOrWhiteSpace(azureOpenAIApiKey))
            throw new ArgumentException("Azure OpenAI API key is required", nameof(azureOpenAIApiKey));

        // Register HttpClient for Azure OpenAI
        services.AddHttpClient<IAIService, AzureOpenAIService>((serviceProvider, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("api-key", azureOpenAIApiKey);
        });

        // Register AI services
        services.AddScoped<IAIService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(AzureOpenAIService));
            var logger = sp.GetService<ILogger<AzureOpenAIService>>();
            
            return new AzureOpenAIService(
                httpClient,
                azureOpenAIEndpoint,
                azureOpenAIApiKey,
                deploymentName,
                logger);
        });

        services.AddScoped<IAIComplianceService>(sp =>
        {
            var logger = sp.GetService<ILogger<HIPAAComplianceService>>();
            return new HIPAAComplianceService(logger);
        });

        return services;
    }

    /// <summary>
    /// Registers AI services with configuration options
    /// </summary>
    public static IServiceCollection AddNalam360AIServices(
        this IServiceCollection services,
        Action<AIServiceOptions> configureOptions)
    {
        var options = new AIServiceOptions();
        configureOptions(options);

        return AddNalam360AIServices(
            services,
            options.AzureOpenAIEndpoint,
            options.AzureOpenAIApiKey,
            options.DeploymentName);
    }
}

/// <summary>
/// Configuration options for AI services
/// </summary>
public class AIServiceOptions
{
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;
    public string AzureOpenAIApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "gpt-4";
    public int TimeoutSeconds { get; set; } = 60;
    public bool EnablePHIDetection { get; set; } = true;
    public bool EnableAuditLogging { get; set; } = true;
}
