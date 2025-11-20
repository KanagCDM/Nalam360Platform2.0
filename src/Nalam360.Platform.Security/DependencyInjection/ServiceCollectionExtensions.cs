using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Security.Cryptography;
using Nalam360.Platform.Security.KeyVault;

namespace Nalam360.Platform.Security.DependencyInjection;

/// <summary>
/// Extension methods for registering security services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AES encryption service to the service collection.
    /// </summary>
    public static IServiceCollection AddAesEncryption(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AesEncryptionOptions>(options =>
            configuration.GetSection("Security:Encryption").Bind(options));

        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        return services;
    }

    /// <summary>
    /// Adds AES encryption service to the service collection with options.
    /// </summary>
    public static IServiceCollection AddAesEncryption(
        this IServiceCollection services,
        Action<AesEncryptionOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        return services;
    }

    /// <summary>
    /// Adds Azure Key Vault service to the service collection.
    /// </summary>
    public static IServiceCollection AddAzureKeyVault(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureKeyVaultOptions>(options =>
            configuration.GetSection("Security:KeyVault").Bind(options));

        services.AddSingleton<IKeyVaultService, AzureKeyVaultService>();

        return services;
    }

    /// <summary>
    /// Adds Azure Key Vault service to the service collection with options.
    /// </summary>
    public static IServiceCollection AddAzureKeyVault(
        this IServiceCollection services,
        Action<AzureKeyVaultOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IKeyVaultService, AzureKeyVaultService>();

        return services;
    }
}
