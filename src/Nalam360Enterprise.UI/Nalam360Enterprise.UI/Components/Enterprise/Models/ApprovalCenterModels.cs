namespace Nalam360Enterprise.UI.Components.Enterprise.Models;

/// <summary>
/// Represents an approval request in the approval center
/// </summary>
public class ApprovalRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; }
    public ApprovalPriority Priority { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string Requestor { get; set; } = string.Empty;
    public string? RequestorEmail { get; set; }
    public string? RequestorDepartment { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ApprovalAttachment> Attachments { get; set; } = new();
    public List<ApprovalComment> Comments { get; set; } = new();
    public List<ApprovalHistory> History { get; set; } = new();
    public List<ApprovalStep> Steps { get; set; } = new();
    public int CurrentStepIndex { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public bool IsUrgent { get; set; }
    public bool IsEscalated { get; set; }
    public int? RemindersSent { get; set; }
}

/// <summary>
/// Status of an approval request
/// </summary>
public enum ApprovalStatus
{
    Pending,
    InReview,
    Approved,
    Rejected,
    Cancelled,
    Expired,
    OnHold,
    RequiresMoreInfo
}

/// <summary>
/// Priority level of an approval request
/// </summary>
public enum ApprovalPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// Represents an attachment in an approval request
/// </summary>
public class ApprovalAttachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

/// <summary>
/// Represents a comment on an approval request
/// </summary>
public class ApprovalComment
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsInternal { get; set; }
    public List<ApprovalAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// Represents a history entry for an approval request
/// </summary>
public class ApprovalHistory
{
    public Guid Id { get; set; }
    public ApprovalAction Action { get; set; }
    public string? ActionDetails { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? PerformedByRole { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? FromValue { get; set; }
    public string? ToValue { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Types of actions in approval history
/// </summary>
public enum ApprovalAction
{
    Created,
    Submitted,
    Assigned,
    Approved,
    Rejected,
    Cancelled,
    Escalated,
    Delegated,
    Recalled,
    Commented,
    AttachmentAdded,
    AttachmentRemoved,
    StatusChanged,
    PriorityChanged,
    DueDateChanged,
    MoreInfoRequested
}

/// <summary>
/// Represents a step in the approval workflow
/// </summary>
public class ApprovalStep
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ApprovalStepStatus Status { get; set; }
    public List<string> Approvers { get; set; } = new();
    public ApprovalStepType Type { get; set; }
    public int? RequiredApprovals { get; set; }
    public int? ReceivedApprovals { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public List<ApprovalStepDecision> Decisions { get; set; } = new();
    public bool IsSkippable { get; set; }
    public bool IsAutoApproved { get; set; }
}

/// <summary>
/// Status of an approval step
/// </summary>
public enum ApprovalStepStatus
{
    Pending,
    InProgress,
    Completed,
    Rejected,
    Skipped
}

/// <summary>
/// Type of approval step
/// </summary>
public enum ApprovalStepType
{
    Sequential,
    Parallel,
    AnyOne,
    Majority,
    Unanimous
}

/// <summary>
/// Represents a decision made in an approval step
/// </summary>
public class ApprovalStepDecision
{
    public string Approver { get; set; } = string.Empty;
    public ApprovalDecision Decision { get; set; }
    public string? Comments { get; set; }
    public DateTime DecidedAt { get; set; }
}

/// <summary>
/// Type of decision made by an approver
/// </summary>
public enum ApprovalDecision
{
    Approved,
    Rejected,
    Abstained,
    Delegated,
    MoreInfoRequested
}

/// <summary>
/// Configuration for the approval center
/// </summary>
public class ApprovalCenterConfiguration
{
    public List<ApprovalFilter> Filters { get; set; } = new();
    public ApprovalSortField SortField { get; set; } = ApprovalSortField.RequestedAt;
    public bool SortDescending { get; set; } = true;
    public ApprovalViewMode ViewMode { get; set; } = ApprovalViewMode.List;
    public List<string> VisibleColumns { get; set; } = new();
    public bool ShowOnlyMyApprovals { get; set; } = true;
    public bool ShowCompletedApprovals { get; set; } = false;
    public bool GroupByRequestType { get; set; } = false;
    public bool EnableAutoRefresh { get; set; } = true;
    public int AutoRefreshInterval { get; set; } = 60;
}

/// <summary>
/// Filter for approval requests
/// </summary>
public class ApprovalFilter
{
    public string Field { get; set; } = string.Empty;
    public ApprovalFilterOperator Operator { get; set; }
    public object? Value { get; set; }
}

/// <summary>
/// Filter operators for approval requests
/// </summary>
public enum ApprovalFilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    Between,
    In
}

/// <summary>
/// Fields for sorting approval requests
/// </summary>
public enum ApprovalSortField
{
    RequestedAt,
    DueDate,
    Priority,
    Status,
    Requestor,
    Amount,
    Title,
    RequestType
}

/// <summary>
/// View mode for approval center
/// </summary>
public enum ApprovalViewMode
{
    List,
    Cards,
    Timeline
}

/// <summary>
/// Statistics for the approval center
/// </summary>
public class ApprovalStatistics
{
    public int TotalPending { get; set; }
    public int TotalApproved { get; set; }
    public int TotalRejected { get; set; }
    public int OverduePending { get; set; }
    public int DueToday { get; set; }
    public int HighPriority { get; set; }
    public decimal? TotalAmount { get; set; }
    public Dictionary<string, int> ByRequestType { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public double? AverageApprovalTime { get; set; }
}

/// <summary>
/// Bulk action to perform on approval requests
/// </summary>
public class ApprovalBulkAction
{
    public ApprovalBulkActionType ActionType { get; set; }
    public List<Guid> RequestIds { get; set; } = new();
    public string? Comments { get; set; }
    public string? DelegateTo { get; set; }
}

/// <summary>
/// Types of bulk actions
/// </summary>
public enum ApprovalBulkActionType
{
    Approve,
    Reject,
    Delegate,
    AddComment,
    ChangePriority,
    SendReminder
}

/// <summary>
/// Delegation settings for an approval
/// </summary>
public class ApprovalDelegation
{
    public string DelegateTo { get; set; } = string.Empty;
    public string? DelegateToEmail { get; set; }
    public string? Reason { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public bool NotifyDelegator { get; set; } = true;
    public bool NotifyDelegatee { get; set; } = true;
}

/// <summary>
/// Quick actions available in the approval center
/// </summary>
public class ApprovalQuickAction
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public ApprovalDecision Decision { get; set; }
    public string? PredefinedComment { get; set; }
    public bool RequiresComment { get; set; }
}
