namespace Nalam360.Platform.Tenancy;

/// <summary>
/// Tenant context provider interface.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    string? GetCurrentTenantId();

    /// <summary>
    /// Sets the current tenant identifier.
    /// </summary>
    void SetCurrentTenantId(string? tenantId);
}

/// <summary>
/// Tenant context.
/// </summary>
public record TenantContext(string TenantId, string? TenantName = null);
