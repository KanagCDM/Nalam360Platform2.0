using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Represents a role in the system
/// </summary>
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public RoleType Type { get; set; } = RoleType.Custom;
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; }
    public Guid? ParentRoleId { get; set; }
    public string? ParentRoleName { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public List<Guid> UserIds { get; set; } = new();
    public int UserCount { get; set; }
    public string? Color { get; set; }
    public string? IconUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Role type enumeration
/// </summary>
public enum RoleType
{
    System,
    Administrator,
    Manager,
    User,
    Custom,
    Service,
    API
}

/// <summary>
/// Represents a permission in the system
/// </summary>
public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public PermissionScope Scope { get; set; } = PermissionScope.Global;
    public string? Category { get; set; }
    public string? Module { get; set; }
    public bool IsGranted { get; set; }
    public bool IsDenied { get; set; }
    public bool IsInherited { get; set; }
    public Guid? InheritedFromRoleId { get; set; }
    public string? InheritedFromRoleName { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public int Priority { get; set; }
}

/// <summary>
/// Permission scope enumeration
/// </summary>
public enum PermissionScope
{
    Global,
    Organization,
    Department,
    Team,
    Personal
}

/// <summary>
/// Permission category for grouping
/// </summary>
public class PermissionCategory
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public bool IsExpanded { get; set; } = true;
}

/// <summary>
/// Role assignment to a user
/// </summary>
public class RoleAssignment
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Reason { get; set; }
}

/// <summary>
/// Role hierarchy node for tree visualization
/// </summary>
public class RoleHierarchyNode
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleType Type { get; set; }
    public int Level { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public bool IsActive { get; set; }
    public List<RoleHierarchyNode> Children { get; set; } = new();
    public bool IsExpanded { get; set; } = true;
}

/// <summary>
/// Permission matrix cell representing a role-permission relationship
/// </summary>
public class PermissionMatrixCell
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsGranted { get; set; }
    public bool IsDenied { get; set; }
    public bool IsInherited { get; set; }
    public string? InheritedFrom { get; set; }
    public bool CanModify { get; set; } = true;
}

/// <summary>
/// Role template for quick role creation
/// </summary>
public class RoleTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleType Type { get; set; }
    public string? Category { get; set; }
    public List<string> PermissionNames { get; set; } = new();
    public bool IsPublic { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// Role comparison result
/// </summary>
public class RoleComparison
{
    public Role Role1 { get; set; } = new();
    public Role Role2 { get; set; } = new();
    public List<Permission> CommonPermissions { get; set; } = new();
    public List<Permission> UniqueToRole1 { get; set; } = new();
    public List<Permission> UniqueToRole2 { get; set; } = new();
    public double SimilarityPercentage { get; set; }
}

/// <summary>
/// Role filter criteria
/// </summary>
public class RoleFilter
{
    public string? SearchQuery { get; set; }
    public List<RoleType> Types { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public bool? IsActive { get; set; }
    public bool? IsSystem { get; set; }
    public bool? HasUsers { get; set; }
    public int? MinUserCount { get; set; }
    public int? MaxUserCount { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

/// <summary>
/// Role statistics and metrics
/// </summary>
public class RoleStatistics
{
    public int TotalRoles { get; set; }
    public int ActiveRoles { get; set; }
    public int InactiveRoles { get; set; }
    public int SystemRoles { get; set; }
    public int CustomRoles { get; set; }
    public int TotalUsers { get; set; }
    public int UsersWithRoles { get; set; }
    public int UsersWithoutRoles { get; set; }
    public Dictionary<RoleType, int> RolesByType { get; set; } = new();
    public Dictionary<string, int> RolesByCategory { get; set; } = new();
    public Dictionary<string, int> TopRolesByUserCount { get; set; } = new();
    public int TotalPermissions { get; set; }
    public int AveragePermissionsPerRole { get; set; }
}

/// <summary>
/// Permission change audit entry
/// </summary>
public class PermissionChange
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public PermissionChangeType ChangeType { get; set; }
    public bool OldValue { get; set; }
    public bool NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Permission change type enumeration
/// </summary>
public enum PermissionChangeType
{
    Grant,
    Revoke,
    Deny,
    Inherit,
    Override
}

/// <summary>
/// Role validation result
/// </summary>
public class RoleValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// View mode for role manager
/// </summary>
public enum RoleManagerViewMode
{
    List,
    Grid,
    Matrix,
    Hierarchy,
    Comparison
}

/// <summary>
/// Sort options for roles
/// </summary>
public enum RoleSortBy
{
    Name,
    Type,
    UserCount,
    PermissionCount,
    CreatedAt,
    ModifiedAt
}

/// <summary>
/// Bulk role operation
/// </summary>
public class BulkRoleOperation
{
    public BulkOperationType OperationType { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
    public List<Guid> PermissionIds { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string? Reason { get; set; }
}

/// <summary>
/// Bulk operation type enumeration
/// </summary>
public enum BulkOperationType
{
    GrantPermissions,
    RevokePermissions,
    ActivateRoles,
    DeactivateRoles,
    DeleteRoles,
    CloneRoles,
    MergeRoles,
    AssignUsers,
    UnassignUsers
}

/// <summary>
/// Role export format
/// </summary>
public enum RoleExportFormat
{
    Excel,
    CSV,
    JSON,
    XML
}

/// <summary>
/// Permission conflict
/// </summary>
public class PermissionConflict
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid PermissionId { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public ConflictType Type { get; set; }
    public string? Description { get; set; }
    public List<string> AffectedRoles { get; set; } = new();
    public ConflictSeverity Severity { get; set; }
}

/// <summary>
/// Conflict type enumeration
/// </summary>
public enum ConflictType
{
    GrantAndDeny,
    CircularInheritance,
    DuplicatePermission,
    MissingDependency,
    InheritanceOverride
}

/// <summary>
/// Conflict severity enumeration
/// </summary>
public enum ConflictSeverity
{
    Low,
    Medium,
    High,
    Critical
}
