namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Email/message in inbox
/// </summary>
public class InboxMessage
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public InboxSender From { get; set; } = new();
    public List<InboxRecipient> To { get; set; } = new();
    public List<InboxRecipient> Cc { get; set; } = new();
    public List<InboxRecipient> Bcc { get; set; } = new();
    public DateTime SentDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public bool IsRead { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsImportant { get; set; }
    public bool HasAttachments { get; set; }
    public List<InboxAttachment> Attachments { get; set; } = new();
    public string FolderId { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public string? ParentMessageId { get; set; }
    public List<InboxMessage> Replies { get; set; } = new();
    public int ReplyCount { get; set; }
    public bool IsDraft { get; set; }
    public bool IsDeleted { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Message sender information
/// </summary>
public class InboxSender
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// Message recipient information
/// </summary>
public class InboxRecipient
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// Email attachment
/// </summary>
public class InboxAttachment
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsInline { get; set; }
}

/// <summary>
/// Inbox folder
/// </summary>
public class InboxFolder
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public InboxFolderType Type { get; set; }
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string Icon { get; set; } = "📁";
    public string? Color { get; set; }
    public bool IsSystem { get; set; }
    public List<InboxFolder> SubFolders { get; set; } = new();
}

/// <summary>
/// Folder types
/// </summary>
public enum InboxFolderType
{
    Inbox,
    Sent,
    Drafts,
    Trash,
    Spam,
    Archive,
    Custom
}

/// <summary>
/// Message label/tag
/// </summary>
public class InboxLabel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#999999";
    public int MessageCount { get; set; }
}

/// <summary>
/// Search filter criteria
/// </summary>
public class InboxSearchFilter
{
    public string? SearchText { get; set; }
    public string? FolderId { get; set; }
    public List<string>? LabelIds { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsFlagged { get; set; }
    public bool? HasAttachments { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SenderEmail { get; set; }
    public InboxSearchScope Scope { get; set; } = InboxSearchScope.All;
}

/// <summary>
/// Search scope
/// </summary>
public enum InboxSearchScope
{
    All,
    Subject,
    Body,
    Sender,
    Attachments
}

/// <summary>
/// Compose message data
/// </summary>
public class InboxComposeMessage
{
    public string? MessageId { get; set; }
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<InboxAttachment> Attachments { get; set; } = new();
    public bool IsDraft { get; set; }
    public string? ReplyToMessageId { get; set; }
    public DateTime? ScheduledSendTime { get; set; }
}

/// <summary>
/// Message send result
/// </summary>
public class MessageSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
}

/// <summary>
/// Inbox settings
/// </summary>
public class InboxSettings
{
    public int MessagesPerPage { get; set; } = 50;
    public bool ShowMessagePreview { get; set; } = true;
    public int PreviewLines { get; set; } = 2;
    public bool GroupByDate { get; set; } = true;
    public bool ShowAvatars { get; set; } = true;
    public bool AutoMarkAsRead { get; set; } = true;
    public int AutoMarkAsReadDelay { get; set; } = 3; // seconds
    public bool EnableThreading { get; set; } = true;
    public bool ShowUnreadOnly { get; set; } = false;
    public InboxSortOrder SortOrder { get; set; } = InboxSortOrder.DateDescending;
    public InboxViewMode ViewMode { get; set; } = InboxViewMode.List;
    public bool EnableNotifications { get; set; } = true;
    public bool PlaySoundOnNewMessage { get; set; } = false;
    public int RefreshInterval { get; set; } = 60; // seconds
}

/// <summary>
/// Sort order options
/// </summary>
public enum InboxSortOrder
{
    DateDescending,
    DateAscending,
    SubjectAscending,
    SubjectDescending,
    SenderAscending,
    SenderDescending,
    SizeDescending,
    SizeAscending
}

/// <summary>
/// View mode options
/// </summary>
public enum InboxViewMode
{
    List,
    Compact,
    Detailed
}

/// <summary>
/// Bulk action type
/// </summary>
public enum InboxBulkAction
{
    MarkAsRead,
    MarkAsUnread,
    Flag,
    Unflag,
    Delete,
    Archive,
    MoveToFolder,
    AddLabel,
    RemoveLabel
}

/// <summary>
/// Message statistics
/// </summary>
public class InboxStatistics
{
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public int FlaggedMessages { get; set; }
    public int DraftMessages { get; set; }
    public long TotalSize { get; set; }
    public Dictionary<string, int> MessagesByFolder { get; set; } = new();
    public Dictionary<string, int> MessagesByLabel { get; set; } = new();
}
