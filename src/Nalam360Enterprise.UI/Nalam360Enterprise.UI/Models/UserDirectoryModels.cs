using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Represents a user in the organizational directory
/// </summary>
public class DirectoryUser
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string DisplayName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? JobTitle { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? OfficeLocation { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? Extension { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public UserPresence Presence { get; set; } = UserPresence.Offline;
    public string? StatusMessage { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public List<DirectoryRole> Roles { get; set; } = new();
    public List<DirectoryTeam> Teams { get; set; } = new();
    public List<DirectoryUser> DirectReports { get; set; } = new();
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public string? Biography { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? GitHubHandle { get; set; }
    public bool IsStarred { get; set; }
    public int TotalExperienceYears { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// User status enumeration
/// </summary>
public enum UserStatus
{
    Active,
    Inactive,
    OnLeave,
    Terminated,
    Suspended,
    PendingActivation
}

/// <summary>
/// User presence status for real-time availability
/// </summary>
public enum UserPresence
{
    Online,
    Away,
    Busy,
    DoNotDisturb,
    BeRightBack,
    Offline,
    InMeeting,
    OnCall,
    Presenting
}

/// <summary>
/// Represents a department in the organization
/// </summary>
public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
    public string? HeadOfDepartmentName { get; set; }
    public int TotalEmployees { get; set; }
    public string? CostCenter { get; set; }
    public string? Location { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<Department> SubDepartments { get; set; } = new();
    public List<DirectoryUser> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents a team within the organization
/// </summary>
public class DirectoryTeam
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TeamType Type { get; set; } = TeamType.Functional;
    public Guid? LeaderId { get; set; }
    public string? LeaderName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public List<DirectoryUser> Members { get; set; } = new();
    public int MemberCount { get; set; }
    public string? Purpose { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EmailAlias { get; set; }
    public string? SlackChannel { get; set; }
    public string? TeamsChannel { get; set; }
}

/// <summary>
/// Team type enumeration
/// </summary>
public enum TeamType
{
    Functional,
    CrossFunctional,
    Project,
    Task,
    Committee,
    WorkingGroup
}

/// <summary>
/// Represents a role assigned to a user
/// </summary>
public class DirectoryRole
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int Level { get; set; }
    public string? BadgeColor { get; set; }
    public string? IconUrl { get; set; }
}

/// <summary>
/// Organization chart node for hierarchical visualization
/// </summary>
public class OrgChartNode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
    public int DirectReportsCount { get; set; }
    public int Level { get; set; }
    public List<OrgChartNode> Children { get; set; } = new();
    public bool IsExpanded { get; set; } = true;
    public UserStatus Status { get; set; }
    public UserPresence Presence { get; set; }
}

/// <summary>
/// Search filter for directory queries
/// </summary>
public class DirectoryFilter
{
    public string? SearchQuery { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new();
    public List<Guid> TeamIds { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
    public List<UserStatus> Statuses { get; set; } = new();
    public List<UserPresence> PresenceStatuses { get; set; } = new();
    public string? Location { get; set; }
    public List<string> Skills { get; set; } = new();
    public bool? ShowStarredOnly { get; set; }
    public bool? ShowDirectReportsOnly { get; set; }
    public Guid? ManagerId { get; set; }
    public int? MinExperienceYears { get; set; }
    public int? MaxExperienceYears { get; set; }
    public DateTime? HireDateFrom { get; set; }
    public DateTime? HireDateTo { get; set; }
}

/// <summary>
/// View mode for displaying directory users
/// </summary>
public enum DirectoryViewMode
{
    Grid,
    List,
    OrgChart,
    Table
}

/// <summary>
/// Sort options for directory
/// </summary>
public enum DirectorySortBy
{
    Name,
    JobTitle,
    Department,
    HireDate,
    LastActive,
    Status
}

/// <summary>
/// User contact information
/// </summary>
public class ContactInfo
{
    public string? WorkPhone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Extension { get; set; }
    public string? WorkEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? OfficeLocation { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Desk { get; set; }
    public string? MailingAddress { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
}

/// <summary>
/// Directory statistics and metrics
/// </summary>
public class DirectoryStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int OnlineUsers { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalTeams { get; set; }
    public Dictionary<string, int> UsersByDepartment { get; set; } = new();
    public Dictionary<UserStatus, int> UsersByStatus { get; set; } = new();
    public Dictionary<UserPresence, int> UsersByPresence { get; set; } = new();
    public Dictionary<string, int> UsersByLocation { get; set; } = new();
    public int NewHiresThisMonth { get; set; }
    public int NewHiresThisQuarter { get; set; }
    public double AverageTenureYears { get; set; }
}

/// <summary>
/// User profile card display settings
/// </summary>
public class ProfileCardSettings
{
    public bool ShowProfilePicture { get; set; } = true;
    public bool ShowJobTitle { get; set; } = true;
    public bool ShowDepartment { get; set; } = true;
    public bool ShowManager { get; set; } = true;
    public bool ShowContactInfo { get; set; } = true;
    public bool ShowPresenceStatus { get; set; } = true;
    public bool ShowSkills { get; set; } = true;
    public bool ShowTeams { get; set; } = true;
    public bool ShowDirectReports { get; set; } = true;
    public bool ShowCustomFields { get; set; } = false;
    public bool AllowDirectMessage { get; set; } = true;
    public bool AllowEmail { get; set; } = true;
    public bool AllowCall { get; set; } = true;
}

/// <summary>
/// User action for directory operations
/// </summary>
public class UserAction
{
    public string ActionType { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? ActionData { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Export format for directory data
/// </summary>
public enum DirectoryExportFormat
{
    Excel,
    CSV,
    PDF,
    VCard
}
