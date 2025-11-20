using Microsoft.AspNetCore.Http;
using Nalam360.Platform.Security.Authorization.Models;
using Nalam360.Platform.Security.Authorization.Stores;
using System.Security.Claims;

namespace Nalam360.Platform.Security.Authorization;

/// <summary>
/// Transforms claims before authorization checks.
/// </summary>
public interface IClaimsTransformation
{
    /// <summary>
    /// Transforms claims principal.
    /// </summary>
    Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default claims transformation that adds role and permission claims.
/// </summary>
public class RolePermissionClaimsTransformation : IClaimsTransformation
{
    private readonly IUserPrincipalStore _principalStore;
    private readonly IRoleStore _roleStore;

    public RolePermissionClaimsTransformation(
        IUserPrincipalStore principalStore,
        IRoleStore roleStore)
    {
        _principalStore = principalStore ?? throw new ArgumentNullException(nameof(principalStore));
        _roleStore = roleStore ?? throw new ArgumentNullException(nameof(roleStore));
    }

    public async Task<ClaimsPrincipal> TransformAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        var userPrincipal = await _principalStore.GetUserPrincipalAsync(userId, cancellationToken);
        if (userPrincipal == null)
        {
            return principal;
        }

        // Create new identity with additional claims
        var identity = new ClaimsIdentity(principal.Identity);

        // Add role claims
        foreach (var role in userPrincipal.Roles)
        {
            if (!principal.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        // Add permission claims
        var permissions = await GetAllPermissionsAsync(userPrincipal, cancellationToken);
        foreach (var permission in permissions)
        {
            if (!principal.HasClaim("permission", permission))
            {
                identity.AddClaim(new Claim("permission", permission));
            }
        }

        // Add custom claims from user principal
        foreach (var (key, value) in userPrincipal.Claims)
        {
            if (!principal.HasClaim(key, value.ToString() ?? string.Empty))
            {
                identity.AddClaim(new Claim(key, value.ToString() ?? string.Empty));
            }
        }

        return new ClaimsPrincipal(identity);
    }

    private async Task<IReadOnlyList<string>> GetAllPermissionsAsync(
        UserPrincipal userPrincipal,
        CancellationToken cancellationToken)
    {
        var permissions = new HashSet<string>(userPrincipal.Permissions);

        // Add permissions from roles
        foreach (var roleName in userPrincipal.Roles)
        {
            var rolePermissions = await _roleStore.GetRolePermissionsAsync(roleName, cancellationToken);
            foreach (var permission in rolePermissions)
            {
                permissions.Add(permission);
            }
        }

        // Remove denied permissions
        foreach (var denied in userPrincipal.DeniedPermissions)
        {
            permissions.Remove(denied);
        }

        return permissions.ToList();
    }
}

/// <summary>
/// Middleware for claims transformation.
/// </summary>
public class ClaimsTransformationMiddleware
{
    private readonly RequestDelegate _next;

    public ClaimsTransformationMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(
        HttpContext context,
        IClaimsTransformation claimsTransformation)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var transformedPrincipal = await claimsTransformation.TransformAsync(
                context.User,
                context.RequestAborted);

            context.User = transformedPrincipal;
        }

        await _next(context);
    }
}
