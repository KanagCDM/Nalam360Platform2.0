namespace Nalam360Enterprise.UI.Core.Security;

/// <summary>
/// Configuration for role-based access control
/// </summary>
public class RbacConfiguration
{
    /// <summary>
    /// Gets or sets whether RBAC is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default behavior when user has no permissions (true = show disabled, false = hide)
    /// </summary>
    public bool ShowDisabledWhenNoPermission { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to audit permission checks
    /// </summary>
    public bool AuditPermissionChecks { get; set; } = false;
}

/// <summary>
/// Service for checking user permissions
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Checks if the current user has the specified permission
    /// </summary>
    Task<bool> HasPermissionAsync(string permission);

    /// <summary>
    /// Checks if the current user has any of the specified permissions
    /// </summary>
    Task<bool> HasAnyPermissionAsync(params string[] permissions);

    /// <summary>
    /// Checks if the current user has all of the specified permissions
    /// </summary>
    Task<bool> HasAllPermissionsAsync(params string[] permissions);

    /// <summary>
    /// Checks if the current user is in the specified role
    /// </summary>
    Task<bool> IsInRoleAsync(string role);

    /// <summary>
    /// Gets all permissions for the current user
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync();
}

/// <summary>
/// Default implementation of IPermissionService
/// Consumers should provide their own implementation based on their auth system
/// </summary>
public class DefaultPermissionService : IPermissionService
{
    private readonly HashSet<string> _userPermissions = new();
    private readonly HashSet<string> _userRoles = new();

    public Task<bool> HasPermissionAsync(string permission)
    {
        return Task.FromResult(_userPermissions.Contains(permission));
    }

    public Task<bool> HasAnyPermissionAsync(params string[] permissions)
    {
        return Task.FromResult(permissions.Any(p => _userPermissions.Contains(p)));
    }

    public Task<bool> HasAllPermissionsAsync(params string[] permissions)
    {
        return Task.FromResult(permissions.All(p => _userPermissions.Contains(p)));
    }

    public Task<bool> IsInRoleAsync(string role)
    {
        return Task.FromResult(_userRoles.Contains(role));
    }

    public Task<IEnumerable<string>> GetUserPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<string>>(_userPermissions);
    }

    /// <summary>
    /// Adds a permission for testing/development purposes
    /// </summary>
    public void AddPermission(string permission)
    {
        _userPermissions.Add(permission);
    }

    /// <summary>
    /// Adds a role for testing/development purposes
    /// </summary>
    public void AddRole(string role)
    {
        _userRoles.Add(role);
    }

    /// <summary>
    /// Removes a permission
    /// </summary>
    public void RemovePermission(string permission)
    {
        _userPermissions.Remove(permission);
    }

    /// <summary>
    /// Clears all permissions and roles
    /// </summary>
    public void Clear()
    {
        _userPermissions.Clear();
        _userRoles.Clear();
    }
}
