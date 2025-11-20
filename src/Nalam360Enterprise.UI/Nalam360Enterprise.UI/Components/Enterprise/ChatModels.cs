using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Represents a chat message
/// </summary>
public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConversationId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? ReplyToMessageId { get; set; }
    public ChatMessage? ReplyToMessage { get; set; }
    public List<ChatAttachment> Attachments { get; set; } = new();
    public List<MessageReaction> Reactions { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Message type enumeration
/// </summary>
public enum MessageType
{
    Text,
    Image,
    File,
    Video,
    Audio,
    Location,
    System,
    Typing
}

/// <summary>
/// Represents a file attachment in a message
/// </summary>
public class ChatAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public ChatAttachmentType Type { get; set; }
}

/// <summary>
/// Chat attachment type enumeration
/// </summary>
public enum ChatAttachmentType
{
    Document,
    Image,
    Video,
    Audio,
    Other
}

/// <summary>
/// Represents an emoji reaction to a message
/// </summary>
public class MessageReaction
{
    public string Emoji { get; set; } = string.Empty;
    public List<string> UserIds { get; set; } = new();
    public int Count => UserIds.Count;
}

/// <summary>
/// Represents a chat conversation
/// </summary>
public class ChatConversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public ConversationType Type { get; set; } = ConversationType.Direct;
    public List<ChatParticipant> Participants { get; set; } = new();
    public ChatMessage? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsMuted { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Conversation type enumeration
/// </summary>
public enum ConversationType
{
    Direct,      // One-on-one
    Group,       // Multiple users
    Channel,     // Broadcast channel
    Support      // Support ticket
}

/// <summary>
/// Represents a participant in a conversation
/// </summary>
public class ChatParticipant
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; } // Admin, Member, Guest
    public UserPresence Presence { get; set; } = UserPresence.Offline;
    public DateTime? LastSeen { get; set; }
    public bool IsTyping { get; set; }
}

/// <summary>
/// User presence status enumeration
/// </summary>
public enum UserPresence
{
    Online,
    Away,
    Busy,
    DoNotDisturb,
    Offline
}

/// <summary>
/// Typing indicator data
/// </summary>
public class TypingIndicator
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Message search result
/// </summary>
public class MessageSearchResult
{
    public ChatMessage Message { get; set; } = new();
    public string ConversationName { get; set; } = string.Empty;
    public List<string> MatchedTerms { get; set; } = new();
    public double Relevance { get; set; }
}

/// <summary>
/// Chat settings and preferences
/// </summary>
public class ChatSettings
{
    public bool ShowTimestamps { get; set; } = true;
    public bool ShowReadReceipts { get; set; } = true;
    public bool ShowTypingIndicators { get; set; } = true;
    public bool EnableNotifications { get; set; } = true;
    public bool EnableSoundNotifications { get; set; } = true;
    public bool GroupMessagesByDate { get; set; } = true;
    public bool ShowAvatars { get; set; } = true;
    public bool EnableEmojis { get; set; } = true;
    public bool EnableFileSharing { get; set; } = true;
    public bool EnableSmartReplies { get; set; } = true; // AI-powered smart replies
    public int MaxFileSize { get; set; } = 10485760; // 10MB default
    public List<string> AllowedFileTypes { get; set; } = new() { ".pdf", ".doc", ".docx", ".jpg", ".png", ".gif" };
    public string DateFormat { get; set; } = "MMM dd, yyyy";
    public string TimeFormat { get; set; } = "h:mm tt";
    public MessageDisplayMode DisplayMode { get; set; } = MessageDisplayMode.Bubbles;
}

/// <summary>
/// AI Smart Reply Suggestion
/// </summary>
public class SmartReplySuggestion
{
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Intent { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Message display mode enumeration
/// </summary>
public enum MessageDisplayMode
{
    Bubbles,     // Chat bubble style
    Compact,     // Compact list style
    Detailed     // Detailed with full metadata
}

/// <summary>
/// Message send result
/// </summary>
public class MessageSendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// File upload progress
/// </summary>
public class FileUploadProgress
{
    public string FileId { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete => TotalBytes > 0 ? (int)((BytesUploaded * 100) / TotalBytes) : 0;
    public bool IsComplete { get; set; }
    public string? ErrorMessage { get; set; }
}
