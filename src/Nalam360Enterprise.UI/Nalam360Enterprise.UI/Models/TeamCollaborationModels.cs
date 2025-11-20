using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models;

// Team Collaboration Domain Models

public class Channel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChannelType Type { get; set; }
    public ChannelVisibility Visibility { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
    public DateTime LastActivityDate { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public ChannelSettings Settings { get; set; } = new();
    public List<ChannelMember> Members { get; set; } = new();
}

public class ChannelSettings
{
    public bool AllowGuestPosting { get; set; }
    public bool AllowThreading { get; set; }
    public bool AllowReactions { get; set; }
    public bool AllowFileUploads { get; set; }
    public bool RequireModeration { get; set; }
    public int MessageRetentionDays { get; set; }
    public NotificationLevel DefaultNotificationLevel { get; set; }
}

public class ChannelMember
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public ChannelRole Role { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime LastSeenDate { get; set; }
    public PresenceStatus Status { get; set; }
    public NotificationLevel NotificationLevel { get; set; }
}

public class CollaborationMessage
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? ThreadId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public DateTime SentDate { get; set; }
    public DateTime? EditedDate { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderAvatar { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPinned { get; set; }
    public int ReplyCount { get; set; }
    public int ReactionCount { get; set; }
    public List<MessageReaction> Reactions { get; set; } = new();
    public List<MessageAttachment> Attachments { get; set; } = new();
    public List<string> MentionedUserIds { get; set; } = new();
    public List<CollaborationMessage> Replies { get; set; } = new();
}

public class MessageReaction
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> UserIds { get; set; } = new();
    public bool HasCurrentUser { get; set; }
}

public class MessageAttachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}

public class Meeting
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MeetingType Type { get; set; }
    public MeetingStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public string OrganizerName { get; set; } = string.Empty;
    public string MeetingUrl { get; set; } = string.Empty;
    public string MeetingCode { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public bool AllowRecording { get; set; }
    public bool IsRecorded { get; set; }
    public string RecordingUrl { get; set; } = string.Empty;
    public List<MeetingParticipant> Participants { get; set; } = new();
    public List<string> Agenda { get; set; } = new();
    public List<MeetingNote> Notes { get; set; } = new();
}

public class MeetingParticipant
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public MeetingRole Role { get; set; }
    public ParticipantStatus Status { get; set; }
    public DateTime? JoinedTime { get; set; }
    public DateTime? LeftTime { get; set; }
    public bool IsMuted { get; set; }
    public bool IsVideoOn { get; set; }
    public bool IsScreenSharing { get; set; }
}

public class MeetingNote
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public NoteType Type { get; set; }
}

public class ThreadSummary
{
    public Guid MessageId { get; set; }
    public string ThreadStarter { get; set; } = string.Empty;
    public string ThreadStarterAvatar { get; set; } = string.Empty;
    public string PreviewText { get; set; } = string.Empty;
    public int ReplyCount { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime LastReplyDate { get; set; }
    public List<string> ParticipantAvatars { get; set; } = new();
}

public class ActivityFeed
{
    public Guid Id { get; set; }
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string ActorAvatar { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid? RelatedChannelId { get; set; }
    public Guid? RelatedMessageId { get; set; }
    public Guid? RelatedMeetingId { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
}

public class TeamCollaborationStatistics
{
    public int TotalChannels { get; set; }
    public int ActiveChannels { get; set; }
    public int TotalMessages { get; set; }
    public int TodayMessages { get; set; }
    public int TotalMeetings { get; set; }
    public int UpcomingMeetings { get; set; }
    public int ActiveMembers { get; set; }
    public int OnlineMembers { get; set; }
    public double AverageResponseTime { get; set; }
    public double EngagementRate { get; set; }
}

public class ChannelFilter
{
    public List<ChannelType> Types { get; set; } = new();
    public List<ChannelVisibility> Visibilities { get; set; } = new();
    public bool? IsArchived { get; set; }
    public bool? HasUnread { get; set; }
    public bool? IsPinned { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int? MinMembers { get; set; }
    public int? MaxMembers { get; set; }
    public List<string> Tags { get; set; } = new();
}

// Enums

public enum ChannelType
{
    Public,
    Private,
    Direct,
    Group,
    Announcement,
    Project,
    Department,
    Topic
}

public enum ChannelVisibility
{
    Public,
    Private,
    Hidden,
    Secret
}

public enum ChannelRole
{
    Owner,
    Admin,
    Moderator,
    Member,
    Guest
}

public enum MessageType
{
    Text,
    File,
    Image,
    Video,
    Audio,
    Link,
    Code,
    Poll,
    System
}

public enum MeetingType
{
    Video,
    Audio,
    ScreenShare,
    Webinar,
    Training,
    Review,
    Standup,
    Retrospective
}

public enum MeetingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed
}

public enum MeetingRole
{
    Organizer,
    Presenter,
    Participant,
    Observer
}

public enum ParticipantStatus
{
    Invited,
    Accepted,
    Declined,
    Tentative,
    Joined,
    Left
}

public enum NotificationLevel
{
    All,
    Mentions,
    DirectOnly,
    Muted
}

public enum PresenceStatus
{
    Online,
    Away,
    Busy,
    DoNotDisturb,
    Offline,
    InMeeting,
    OnCall,
    Presenting
}

public enum NoteType
{
    General,
    ActionItem,
    Decision,
    Question,
    Followup
}

public enum ActivityType
{
    ChannelCreated,
    ChannelArchived,
    MemberJoined,
    MemberLeft,
    MessagePinned,
    MeetingScheduled,
    MeetingStarted,
    MeetingEnded,
    FileUploaded,
    ChannelRenamed
}

public enum CollaborationViewMode
{
    Channels,
    Threads,
    Meetings,
    Files,
    Activity
}

public enum ChannelSortBy
{
    Name,
    Activity,
    Members,
    Created,
    Unread
}
