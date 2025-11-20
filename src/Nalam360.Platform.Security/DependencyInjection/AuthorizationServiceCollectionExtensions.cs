using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Security.Authorization.Policies;
using Nalam360.Platform.Security.Authorization.Stores;

namespace Nalam360.Platform.Security.DependencyInjection;

/// <summary>
/// Extension methods for authorization services registration.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds authorization services with in-memory stores.
    /// </summary>
    public static IServiceCollection AddNalam360Authorization(this IServiceCollection services)
    {
        return services.AddNalam360Authorization(options => { });
    }

    /// <summary>
    /// Adds authorization services with configuration.
    /// </summary>
    public static IServiceCollection AddNalam360Authorization(
        this IServiceCollection services,
        Action<AuthorizationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AuthorizationOptions();
        configure(options);

        // Register stores (use custom implementations if provided)
        if (options.PermissionStoreType != null)
        {
            services.AddSingleton(typeof(IPermissionStore), options.PermissionStoreType);
        }
        else
        {
            services.AddSingleton<IPermissionStore, InMemoryPermissionStore>();
        }

        if (options.RoleStoreType != null)
        {
            services.AddSingleton(typeof(IRoleStore), options.RoleStoreType);
        }
        else
        {
            services.AddSingleton<IRoleStore, InMemoryRoleStore>();
        }

        if (options.UserPrincipalStoreType != null)
        {
            services.AddSingleton(typeof(IUserPrincipalStore), options.UserPrincipalStoreType);
        }
        else
        {
            services.AddSingleton<IUserPrincipalStore, InMemoryUserPrincipalStore>();
        }

        // Register policy registry
        services.AddSingleton<IPolicyRegistry>(sp =>
        {
            var registry = new PolicyRegistry();

            // Register built-in policies
            foreach (var policy in options.Policies)
            {
                registry.RegisterPolicy(policy);
            }

            return registry;
        });

        // Register authorization service
        services.AddHttpContextAccessor();
        services.AddScoped<Authorization.IAuthorizationService, Authorization.DefaultAuthorizationService>();

        // Register claims transformation
        if (options.EnableClaimsTransformation)
        {
            services.AddScoped<Authorization.IClaimsTransformation, Authorization.RolePermissionClaimsTransformation>();
        }

        return services;
    }

    /// <summary>
    /// Adds claims transformation middleware.
    /// </summary>
    public static IApplicationBuilder UseClaimsTransformation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<Authorization.ClaimsTransformationMiddleware>();
    }
}

/// <summary>
/// Options for authorization configuration.
/// </summary>
public class AuthorizationOptions
{
    /// <summary>
    /// Custom permission store type.
    /// </summary>
    public Type? PermissionStoreType { get; set; }

    /// <summary>
    /// Custom role store type.
    /// </summary>
    public Type? RoleStoreType { get; set; }

    /// <summary>
    /// Custom user principal store type.
    /// </summary>
    public Type? UserPrincipalStoreType { get; set; }

    /// <summary>
    /// Policies to register.
    /// </summary>
    public List<IAuthorizationPolicy> Policies { get; set; } = new();

    /// <summary>
    /// Enable automatic claims transformation.
    /// </summary>
    public bool EnableClaimsTransformation { get; set; } = true;

    /// <summary>
    /// Adds a permission policy.
    /// </summary>
    public AuthorizationOptions AddPermissionPolicy(string name, bool requireAll = true, params string[] permissions)
    {
        Policies.Add(new PermissionPolicy(name, requireAll, permissions));
        return this;
    }

    /// <summary>
    /// Adds a role policy.
    /// </summary>
    public AuthorizationOptions AddRolePolicy(string name, bool requireAll = true, params string[] roles)
    {
        Policies.Add(new RolePolicy(name, requireAll, roles));
        return this;
    }

    /// <summary>
    /// Adds a custom policy.
    /// </summary>
    public AuthorizationOptions AddCustomPolicy(IAuthorizationPolicy policy)
    {
        Policies.Add(policy);
        return this;
    }

    /// <summary>
    /// Uses custom permission store.
    /// </summary>
    public AuthorizationOptions UsePermissionStore<TStore>() where TStore : IPermissionStore
    {
        PermissionStoreType = typeof(TStore);
        return this;
    }

    /// <summary>
    /// Uses custom role store.
    /// </summary>
    public AuthorizationOptions UseRoleStore<TStore>() where TStore : IRoleStore
    {
        RoleStoreType = typeof(TStore);
        return this;
    }

    /// <summary>
    /// Uses custom user principal store.
    /// </summary>
    public AuthorizationOptions UseUserPrincipalStore<TStore>() where TStore : IUserPrincipalStore
    {
        UserPrincipalStoreType = typeof(TStore);
        return this;
    }
}
