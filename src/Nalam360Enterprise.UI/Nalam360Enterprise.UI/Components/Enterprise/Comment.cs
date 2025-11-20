namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Represents a comment in a threaded discussion.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the comment.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the comment text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID who posted the comment.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user name who posted the comment.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string UserDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's initials for avatar display.
        /// </summary>
        public string UserInitials { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the comment was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp when the comment was last edited.
        /// </summary>
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the comment has been edited.
        /// </summary>
        public bool IsEdited { get; set; }

        /// <summary>
        /// Gets or sets the parent comment ID for threaded replies.
        /// </summary>
        public Guid? ParentCommentId { get; set; }

        /// <summary>
        /// Gets or sets the depth level in the thread (0 = root comment).
        /// </summary>
        public int ThreadDepth { get; set; }

        /// <summary>
        /// Gets or sets the reactions to this comment.
        /// Key: Reaction emoji, Value: Count of reactions.
        /// </summary>
        public Dictionary<string, int> Reactions { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the users who reacted to this comment.
        /// Key: Reaction emoji, Value: List of user IDs.
        /// </summary>
        public Dictionary<string, List<string>> ReactedUsers { get; set; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Gets or sets the list of mentioned user IDs in this comment.
        /// </summary>
        public List<string> MentionedUserIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of mentioned user names in this comment.
        /// </summary>
        public List<string> MentionedUserNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets whether the comment has been deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the comment was deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the number of replies to this comment.
        /// </summary>
        public int ReplyCount { get; set; }

        /// <summary>
        /// Gets or sets the reply comments (for hierarchical loading).
        /// </summary>
        public List<Comment>? Replies { get; set; }

        /// <summary>
        /// Gets or sets the comment's status.
        /// </summary>
        public CommentStatus Status { get; set; } = CommentStatus.Active;

        /// <summary>
        /// Gets or sets additional metadata for the comment.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets whether the comment has been flagged for moderation.
        /// </summary>
        public bool IsFlagged { get; set; }

        /// <summary>
        /// Gets or sets the flag reason if flagged.
        /// </summary>
        public string? FlagReason { get; set; }

        /// <summary>
        /// Gets or sets the list of attachments (URLs or file references).
        /// </summary>
        public List<CommentAttachment> Attachments { get; set; } = new List<CommentAttachment>();

        /// <summary>
        /// Gets or sets the comment's visibility scope.
        /// </summary>
        public CommentVisibility Visibility { get; set; } = CommentVisibility.Public;

        /// <summary>
        /// Gets or sets whether the comment is pinned to the top.
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets the pin order if pinned (lower = higher priority).
        /// </summary>
        public int? PinOrder { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the commenter.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the user agent of the commenter.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Gets the total reaction count across all reaction types.
        /// </summary>
        public int TotalReactions => Reactions.Values.Sum();

        /// <summary>
        /// Adds a reaction to the comment.
        /// </summary>
        /// <param name="reaction">The reaction emoji.</param>
        /// <param name="userId">The user ID adding the reaction.</param>
        public void AddReaction(string reaction, string userId)
        {
            if (!Reactions.ContainsKey(reaction))
            {
                Reactions[reaction] = 0;
                ReactedUsers[reaction] = new List<string>();
            }

            if (!ReactedUsers[reaction].Contains(userId))
            {
                Reactions[reaction]++;
                ReactedUsers[reaction].Add(userId);
            }
        }

        /// <summary>
        /// Removes a reaction from the comment.
        /// </summary>
        /// <param name="reaction">The reaction emoji.</param>
        /// <param name="userId">The user ID removing the reaction.</param>
        public void RemoveReaction(string reaction, string userId)
        {
            if (ReactedUsers.ContainsKey(reaction) && ReactedUsers[reaction].Contains(userId))
            {
                Reactions[reaction]--;
                ReactedUsers[reaction].Remove(userId);

                if (Reactions[reaction] <= 0)
                {
                    Reactions.Remove(reaction);
                    ReactedUsers.Remove(reaction);
                }
            }
        }

        /// <summary>
        /// Checks if a user has reacted with a specific reaction.
        /// </summary>
        /// <param name="reaction">The reaction emoji.</param>
        /// <param name="userId">The user ID to check.</param>
        /// <returns>True if the user has reacted, false otherwise.</returns>
        public bool HasUserReacted(string reaction, string userId)
        {
            return ReactedUsers.ContainsKey(reaction) && ReactedUsers[reaction].Contains(userId);
        }

        /// <summary>
        /// Marks the comment as edited.
        /// </summary>
        public void MarkAsEdited()
        {
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the comment.
        /// </summary>
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            Status = CommentStatus.Deleted;
        }

        /// <summary>
        /// Flags the comment for moderation.
        /// </summary>
        /// <param name="reason">The reason for flagging.</param>
        public void Flag(string reason)
        {
            IsFlagged = true;
            FlagReason = reason;
            Status = CommentStatus.Flagged;
        }

        /// <summary>
        /// Pins the comment.
        /// </summary>
        /// <param name="order">The pin order.</param>
        public void Pin(int order)
        {
            IsPinned = true;
            PinOrder = order;
        }

        /// <summary>
        /// Unpins the comment.
        /// </summary>
        public void Unpin()
        {
            IsPinned = false;
            PinOrder = null;
        }
    }

    /// <summary>
    /// Represents the status of a comment.
    /// </summary>
    public enum CommentStatus
    {
        /// <summary>
        /// Comment is active and visible.
        /// </summary>
        Active,

        /// <summary>
        /// Comment is pending moderation.
        /// </summary>
        PendingModeration,

        /// <summary>
        /// Comment has been flagged for review.
        /// </summary>
        Flagged,

        /// <summary>
        /// Comment has been deleted.
        /// </summary>
        Deleted,

        /// <summary>
        /// Comment has been hidden by moderator.
        /// </summary>
        Hidden,

        /// <summary>
        /// Comment has been approved by moderator.
        /// </summary>
        Approved,

        /// <summary>
        /// Comment has been rejected by moderator.
        /// </summary>
        Rejected
    }

    /// <summary>
    /// Represents the visibility scope of a comment.
    /// </summary>
    public enum CommentVisibility
    {
        /// <summary>
        /// Visible to everyone.
        /// </summary>
        Public,

        /// <summary>
        /// Visible only to authenticated users.
        /// </summary>
        Private,

        /// <summary>
        /// Visible only to specific users or groups.
        /// </summary>
        Restricted,

        /// <summary>
        /// Visible only to the author and moderators.
        /// </summary>
        AuthorOnly
    }

    /// <summary>
    /// Represents an attachment on a comment.
    /// </summary>
    public class CommentAttachment
    {
        /// <summary>
        /// Gets or sets the attachment ID.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file URL or path.
        /// </summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the file.
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the attachment type.
        /// </summary>
        public AttachmentType Type { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail URL for images/videos.
        /// </summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets when the attachment was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents the type of attachment.
    /// </summary>
    public enum AttachmentType
    {
        /// <summary>
        /// Image file.
        /// </summary>
        Image,

        /// <summary>
        /// Video file.
        /// </summary>
        Video,

        /// <summary>
        /// Document file.
        /// </summary>
        Document,

        /// <summary>
        /// Link/URL.
        /// </summary>
        Link,

        /// <summary>
        /// Other file type.
        /// </summary>
        Other
    }
}
