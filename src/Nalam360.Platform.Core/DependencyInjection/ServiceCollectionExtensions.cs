using Microsoft.Extensions.DependencyInjection;
using Nalam360.Platform.Core.Identity;
using Nalam360.Platform.Core.Security;
using Nalam360.Platform.Core.Time;

namespace Nalam360.Platform.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering core services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core platform services
    /// </summary>
    public static IServiceCollection AddPlatformCore(this IServiceCollection services)
    {
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<IGuidProvider, GuidProvider>();
        services.AddSingleton<IRandomNumberGenerator, CryptoRandomNumberGenerator>();

        return services;
    }
}
