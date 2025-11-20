namespace Nalam360.Platform.Security.Authorization.Stores;

/// <summary>
/// Store for managing permissions.
/// </summary>
public interface IPermissionStore
{
    /// <summary>
    /// Gets a permission by name.
    /// </summary>
    Task<Models.Permission?> GetPermissionAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions.
    /// </summary>
    Task<IReadOnlyList<Models.Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a permission.
    /// </summary>
    Task<Models.Permission> SavePermissionAsync(Models.Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a permission.
    /// </summary>
    Task DeletePermissionAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Store for managing roles.
/// </summary>
public interface IRoleStore
{
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<Models.Role?> GetRoleAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles.
    /// </summary>
    Task<IReadOnlyList<Models.Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a role.
    /// </summary>
    Task<Models.Role> SaveRoleAsync(Models.Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role.
    /// </summary>
    Task DeleteRoleAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a role (including inherited).
    /// </summary>
    Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Store for managing user principals.
/// </summary>
public interface IUserPrincipalStore
{
    /// <summary>
    /// Gets user principal.
    /// </summary>
    Task<Models.UserPrincipal?> GetUserPrincipalAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves user principal.
    /// </summary>
    Task<Models.UserPrincipal> SaveUserPrincipalAsync(Models.UserPrincipal principal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns role to user.
    /// </summary>
    Task AssignRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes role from user.
    /// </summary>
    Task RemoveRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants permission to user.
    /// </summary>
    Task GrantPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes permission from user.
    /// </summary>
    Task RevokePermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly denies permission for user.
    /// </summary>
    Task DenyPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
}
