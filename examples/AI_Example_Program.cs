using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nalam360Enterprise.UI.Core.AI;
using Nalam360Enterprise.UI.Core.Security;

namespace Nalam360.AI.Example;

/// <summary>
/// Example Program.cs showing how to configure AI services
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Configure base HTTP client
        builder.Services.AddScoped(sp => new HttpClient 
        { 
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
        });

        // ============================================
        // AI Services Configuration (Method 1: Direct)
        // ============================================
        builder.Services.AddNalam360AIServices(
            endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
            apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!,
            deploymentName: builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4"
        );

        // ============================================
        // AI Services Configuration (Method 2: Options)
        // ============================================
        // builder.Services.AddNalam360AIServices(options =>
        // {
        //     options.AzureOpenAIEndpoint = builder.Configuration["AzureOpenAI:Endpoint"]!;
        //     options.AzureOpenAIApiKey = builder.Configuration["AzureOpenAI:ApiKey"]!;
        //     options.DeploymentName = "gpt-4";
        //     options.TimeoutSeconds = 60;
        //     options.EnablePHIDetection = true;
        //     options.EnableAuditLogging = true;
        // });

        // Register other required services
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddScoped<IAuditService, AuditService>();

        await builder.Build().RunAsync();
    }
}

/// <summary>
/// Example appsettings.json configuration
/// </summary>
public class ExampleConfiguration
{
    public const string AppSettingsExample = @"
{
  ""AzureOpenAI"": {
    ""Endpoint"": ""https://your-resource.openai.azure.com"",
    ""ApiKey"": ""your-api-key-here"",
    ""DeploymentName"": ""gpt-4"",
    ""TimeoutSeconds"": 60
  },
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning"",
      ""Nalam360Enterprise.UI.Core.AI"": ""Debug""
    }
  }
}
";
}

// Stub implementations for example
public interface IPermissionService { }
public class PermissionService : IPermissionService { }
public interface IAuditService { }
public class AuditService : IAuditService { }
public class App : Microsoft.AspNetCore.Components.ComponentBase { }
