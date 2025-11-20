namespace Nalam360.Platform.Security.Authorization.Models;

/// <summary>
/// Represents a permission in the system.
/// </summary>
public class Permission
{
    /// <summary>
    /// Unique permission identifier (e.g., "users.read", "orders.create").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Resource this permission applies to (e.g., "Users", "Orders").
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Action being permitted (e.g., "Read", "Create", "Update", "Delete").
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Permission category for organization.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this is a system-level permission.
    /// </summary>
    public bool IsSystem { get; set; }
}

/// <summary>
/// Represents a role in the system.
/// </summary>
public class Role
{
    /// <summary>
    /// Unique role identifier (e.g., "Admin", "User", "Manager").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Permissions assigned to this role.
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Parent roles this role inherits from.
    /// </summary>
    public List<string> InheritsFrom { get; set; } = new();

    /// <summary>
    /// Whether this is a system-level role.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Priority/hierarchy level (higher = more privileged).
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// Represents a user's assigned roles and permissions.
/// </summary>
public class UserPrincipal
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Directly assigned roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Directly assigned permissions (overrides).
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Denied permissions (explicit denies).
    /// </summary>
    public List<string> DeniedPermissions { get; set; } = new();

    /// <summary>
    /// Additional claims for context.
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();
}

/// <summary>
/// Result of an authorization check.
/// </summary>
public class AuthorizationResult
{
    public bool IsAuthorized { get; set; }
    public string? Reason { get; set; }
    public List<string>? MissingPermissions { get; set; }
    public List<string>? RequiredRoles { get; set; }

    public static AuthorizationResult Success() =>
        new() { IsAuthorized = true };

    public static AuthorizationResult Failure(string reason) =>
        new() { IsAuthorized = false, Reason = reason };

    public static AuthorizationResult MissingPermission(params string[] permissions) =>
        new()
        {
            IsAuthorized = false,
            Reason = "Missing required permissions",
            MissingPermissions = new List<string>(permissions)
        };

    public static AuthorizationResult MissingRole(params string[] roles) =>
        new()
        {
            IsAuthorized = false,
            Reason = "Missing required roles",
            RequiredRoles = new List<string>(roles)
        };
}
