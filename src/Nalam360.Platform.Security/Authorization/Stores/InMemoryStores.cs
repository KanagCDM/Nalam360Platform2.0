using System.Collections.Concurrent;
using Nalam360.Platform.Security.Authorization.Models;

namespace Nalam360.Platform.Security.Authorization.Stores;

/// <summary>
/// In-memory permission store for development and testing.
/// </summary>
public class InMemoryPermissionStore : IPermissionStore
{
    private readonly ConcurrentDictionary<string, Permission> _permissions = new();

    public Task<Permission?> GetPermissionAsync(string name, CancellationToken cancellationToken = default)
    {
        _permissions.TryGetValue(name, out var permission);
        return Task.FromResult(permission);
    }

    public Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Permission>>(_permissions.Values.ToList());
    }

    public Task<Permission> SavePermissionAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(permission);
        _permissions[permission.Name] = permission;
        return Task.FromResult(permission);
    }

    public Task DeletePermissionAsync(string name, CancellationToken cancellationToken = default)
    {
        _permissions.TryRemove(name, out _);
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory role store for development and testing.
/// </summary>
public class InMemoryRoleStore : IRoleStore
{
    private readonly ConcurrentDictionary<string, Role> _roles = new();
    private readonly IPermissionStore _permissionStore;

    public InMemoryRoleStore(IPermissionStore permissionStore)
    {
        _permissionStore = permissionStore;
    }

    public Task<Role?> GetRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        _roles.TryGetValue(name, out var role);
        return Task.FromResult(role);
    }

    public Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Role>>(_roles.Values.ToList());
    }

    public Task<Role> SaveRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        _roles[role.Name] = role;
        return Task.FromResult(role);
    }

    public Task DeleteRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        _roles.TryRemove(name, out _);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleAsync(roleName, cancellationToken);
        if (role == null)
        {
            return Array.Empty<string>();
        }

        var permissions = new HashSet<string>(role.Permissions);

        // Recursively get inherited permissions
        foreach (var parentRoleName in role.InheritsFrom)
        {
            var parentPermissions = await GetRolePermissionsAsync(parentRoleName, cancellationToken);
            foreach (var perm in parentPermissions)
            {
                permissions.Add(perm);
            }
        }

        return permissions.ToList();
    }
}

/// <summary>
/// In-memory user principal store for development and testing.
/// </summary>
public class InMemoryUserPrincipalStore : IUserPrincipalStore
{
    private readonly ConcurrentDictionary<string, UserPrincipal> _principals = new();

    public Task<UserPrincipal?> GetUserPrincipalAsync(string userId, CancellationToken cancellationToken = default)
    {
        _principals.TryGetValue(userId, out var principal);
        return Task.FromResult(principal);
    }

    public Task<UserPrincipal> SaveUserPrincipalAsync(UserPrincipal principal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        _principals[principal.UserId] = principal;
        return Task.FromResult(principal);
    }

    public async Task AssignRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var principal = await GetOrCreatePrincipalAsync(userId);
        if (!principal.Roles.Contains(roleName))
        {
            principal.Roles.Add(roleName);
        }
        await SaveUserPrincipalAsync(principal, cancellationToken);
    }

    public async Task RemoveRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var principal = await GetUserPrincipalAsync(userId, cancellationToken);
        if (principal != null)
        {
            principal.Roles.Remove(roleName);
            await SaveUserPrincipalAsync(principal, cancellationToken);
        }
    }

    public async Task GrantPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        var principal = await GetOrCreatePrincipalAsync(userId);
        if (!principal.Permissions.Contains(permission))
        {
            principal.Permissions.Add(permission);
        }
        // Remove from denied if present
        principal.DeniedPermissions.Remove(permission);
        await SaveUserPrincipalAsync(principal, cancellationToken);
    }

    public async Task RevokePermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        var principal = await GetUserPrincipalAsync(userId, cancellationToken);
        if (principal != null)
        {
            principal.Permissions.Remove(permission);
            await SaveUserPrincipalAsync(principal, cancellationToken);
        }
    }

    public async Task DenyPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        var principal = await GetOrCreatePrincipalAsync(userId);
        if (!principal.DeniedPermissions.Contains(permission))
        {
            principal.DeniedPermissions.Add(permission);
        }
        // Remove from granted if present
        principal.Permissions.Remove(permission);
        await SaveUserPrincipalAsync(principal, cancellationToken);
    }

    private async Task<UserPrincipal> GetOrCreatePrincipalAsync(string userId)
    {
        var principal = await GetUserPrincipalAsync(userId);
        if (principal == null)
        {
            principal = new UserPrincipal { UserId = userId };
            await SaveUserPrincipalAsync(principal);
        }
        return principal;
    }
}
