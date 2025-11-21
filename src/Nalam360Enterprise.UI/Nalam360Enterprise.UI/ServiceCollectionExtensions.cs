using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.Browser;
using Nalam360Enterprise.UI.Core.Http;
using Nalam360Enterprise.UI.Core.Notifications;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.State;
using Nalam360Enterprise.UI.Core.Storage;
using Nalam360Enterprise.UI.Core.Theming;
using Syncfusion.Blazor;

namespace Nalam360Enterprise.UI;

/// <summary>
/// Extension methods for registering Nalam360 Enterprise UI services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Nalam360 Enterprise UI services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureTheme">Optional theme configuration</param>
    /// <param name="configureRbac">Optional RBAC configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNalam360EnterpriseUI(
        this IServiceCollection services,
        Action<ThemeConfiguration>? configureTheme = null,
        Action<RbacConfiguration>? configureRbac = null)
    {
        // Register Syncfusion license
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtfdXZWRmRcUkxyX0RZYUA=");
        
        // Register Syncfusion services
        services.AddSyncfusionBlazor();

        // Configure and register theme configuration
        var themeConfig = new ThemeConfiguration();
        configureTheme?.Invoke(themeConfig);
        services.AddSingleton(themeConfig);
        services.AddScoped<ThemeService>();

        // Configure and register RBAC configuration
        var rbacConfig = new RbacConfiguration();
        configureRbac?.Invoke(rbacConfig);
        services.AddSingleton(rbacConfig);

        // Register security services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPermissionService, DefaultPermissionService>();

        // Register AI services
        services.AddSingleton<IMLModelService, MLNetModelService>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MLNetModelService>>();
            return new MLNetModelService(logger, "ML/Models");
        });

        // Register HTTP client and API client
        services.AddHttpClient<IApiClient, ApiClient>();
        services.AddScoped<IApiClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(IApiClient));
            var logger = sp.GetRequiredService<ILogger<ApiClient>>();
            return new ApiClient(httpClient, logger);
        });

        // Register state management
        services.AddScoped(typeof(IStateManager<>), typeof(StateManager<>));

        // Register notification queue
        services.AddSingleton<INotificationQueue, NotificationQueue>();

        // Register storage service
        services.AddScoped<IStorageService, StorageService>();

        // Register browser service
        services.AddScoped<IBrowserService, BrowserService>();

        return services;
    }

    /// <summary>
    /// Adds Nalam360 Enterprise UI services with a custom permission service
    /// </summary>
    public static IServiceCollection AddNalam360EnterpriseUI<TPermissionService>(
        this IServiceCollection services,
        Action<ThemeConfiguration>? configureTheme = null,
        Action<RbacConfiguration>? configureRbac = null)
        where TPermissionService : class, IPermissionService
    {
        // Register base services
        AddNalam360EnterpriseUI(services, configureTheme, configureRbac);

        // Replace default permission service with custom implementation
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPermissionService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
        services.AddScoped<IPermissionService, TPermissionService>();

        return services;
    }

    /// <summary>
    /// Adds Nalam360 Enterprise UI services with a custom audit service
    /// </summary>
    public static IServiceCollection AddNalam360EnterpriseUIWithCustomAudit<TAuditService>(
        this IServiceCollection services,
        Action<ThemeConfiguration>? configureTheme = null,
        Action<RbacConfiguration>? configureRbac = null)
        where TAuditService : class, IAuditService
    {
        // Register base services
        AddNalam360EnterpriseUI(services, configureTheme, configureRbac);

        // Replace default audit service with custom implementation
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAuditService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
        services.AddScoped<IAuditService, TAuditService>();

        return services;
    }
}
