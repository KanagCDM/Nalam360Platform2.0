namespace Nalam360.Platform.Security.Authorization;

/// <summary>
/// Service for checking permissions and authorization.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if the current user has the specified permission.
    /// </summary>
    Task<bool> HasPermissionAsync(
        string permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user has the specified role.
    /// </summary>
    Task<bool> HasRoleAsync(
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current user satisfies the specified policy.
    /// </summary>
    Task<bool> SatisfiesPolicyAsync(
        string policyName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for the current user.
    /// </summary>
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles for the current user.
    /// </summary>
    Task<IReadOnlyList<string>> GetUserRolesAsync(
        CancellationToken cancellationToken = default);
}
