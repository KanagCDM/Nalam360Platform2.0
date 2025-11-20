using Microsoft.AspNetCore.Http;
using Nalam360.Platform.Security.Authorization.Models;
using Nalam360.Platform.Security.Authorization.Policies;
using Nalam360.Platform.Security.Authorization.Stores;
using System.Security.Claims;

namespace Nalam360.Platform.Security.Authorization;

/// <summary>
/// Default implementation of authorization service.
/// </summary>
public class DefaultAuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserPrincipalStore _principalStore;
    private readonly IRoleStore _roleStore;
    private readonly IPolicyRegistry _policyRegistry;

    public DefaultAuthorizationService(
        IHttpContextAccessor httpContextAccessor,
        IUserPrincipalStore principalStore,
        IRoleStore roleStore,
        IPolicyRegistry policyRegistry)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _principalStore = principalStore ?? throw new ArgumentNullException(nameof(principalStore));
        _roleStore = roleStore ?? throw new ArgumentNullException(nameof(roleStore));
        _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
    }

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var permissions = await GetUserPermissionsInternalAsync(userId, cancellationToken);
        return permissions.Contains(permission);
    }

    public async Task<bool> HasRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var roles = await GetUserRolesInternalAsync(userId, cancellationToken);
        return roles.Contains(role);
    }

    public async Task<bool> SatisfiesPolicyAsync(string policyName, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var policy = _policyRegistry.GetPolicy(policyName);
        if (policy == null)
        {
            return false;
        }

        var principal = await _principalStore.GetUserPrincipalAsync(userId, cancellationToken);
        if (principal == null)
        {
            return false;
        }

        var permissions = await GetUserPermissionsInternalAsync(userId, cancellationToken);
        var roles = await GetUserRolesInternalAsync(userId, cancellationToken);

        var result = await policy.EvaluateAsync(principal, permissions, roles, cancellationToken);
        return result.IsAuthorized;
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Array.Empty<string>();
        }

        return await GetUserPermissionsInternalAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Array.Empty<string>();
        }

        return await GetUserRolesInternalAsync(userId, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetUserPermissionsInternalAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var principal = await _principalStore.GetUserPrincipalAsync(userId, cancellationToken);
        if (principal == null)
        {
            return Array.Empty<string>();
        }

        var permissions = new HashSet<string>();

        // Add directly assigned permissions
        foreach (var permission in principal.Permissions)
        {
            permissions.Add(permission);
        }

        // Add permissions from roles
        foreach (var roleName in principal.Roles)
        {
            var rolePermissions = await _roleStore.GetRolePermissionsAsync(roleName, cancellationToken);
            foreach (var permission in rolePermissions)
            {
                permissions.Add(permission);
            }
        }

        // Remove explicitly denied permissions
        foreach (var denied in principal.DeniedPermissions)
        {
            permissions.Remove(denied);
        }

        return permissions.ToList();
    }

    private async Task<IReadOnlyList<string>> GetUserRolesInternalAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var principal = await _principalStore.GetUserPrincipalAsync(userId, cancellationToken);
        if (principal == null)
        {
            return Array.Empty<string>();
        }

        var roles = new HashSet<string>(principal.Roles);

        // Add inherited roles
        foreach (var roleName in principal.Roles.ToList())
        {
            await AddInheritedRolesAsync(roleName, roles, cancellationToken);
        }

        return roles.ToList();
    }

    private async Task AddInheritedRolesAsync(
        string roleName,
        HashSet<string> roles,
        CancellationToken cancellationToken)
    {
        var role = await _roleStore.GetRoleAsync(roleName, cancellationToken);
        if (role == null)
        {
            return;
        }

        foreach (var parentRoleName in role.InheritsFrom)
        {
            if (roles.Add(parentRoleName))
            {
                // Recursively add parent's parents
                await AddInheritedRolesAsync(parentRoleName, roles, cancellationToken);
            }
        }
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User
            ?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User
            ?.FindFirst("sub")?.Value;
    }
}
