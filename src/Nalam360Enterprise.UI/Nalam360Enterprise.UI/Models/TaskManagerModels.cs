using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models
{
    // Task Model
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int Progress { get; set; } // 0-100
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public Guid? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedToAvatar { get; set; }
        public Guid? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public Guid? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public List<TaskAttachment> Attachments { get; set; } = new();
        public List<TaskComment> Comments { get; set; } = new();
        public List<TaskItem> Subtasks { get; set; } = new();
        public List<TaskDependency> Dependencies { get; set; } = new();
        public TaskRecurrence? Recurrence { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsStarred { get; set; }
        public bool IsArchived { get; set; }
        public int SubtaskCount => Subtasks.Count;
        public int CompletedSubtaskCount => Subtasks.Count(st => st.Status == TaskStatus.Completed);
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != TaskStatus.Completed;
        public int DaysUntilDue => DueDate.HasValue ? (DueDate.Value.Date - DateTime.Now.Date).Days : 0;
    }

    // Task Status Enum
    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        OnHold,
        UnderReview,
        Completed,
        Cancelled,
        Blocked,
        Deferred
    }

    // Task Priority Enum
    public enum TaskPriority
    {
        Trivial,
        Low,
        Normal,
        High,
        Critical
    }

    // Task Dependency
    public class TaskDependency
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid DependsOnTaskId { get; set; }
        public string DependsOnTaskTitle { get; set; } = string.Empty;
        public DependencyType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Dependency Type Enum
    public enum DependencyType
    {
        FinishToStart,  // Predecessor must finish before successor starts
        StartToStart,   // Both tasks start at the same time
        FinishToFinish, // Both tasks finish at the same time
        StartToFinish   // Successor finishes when predecessor starts
    }

    // Task Attachment
    public class TaskAttachment
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public Guid UploadedById { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    // Task Comment
    public class TaskComment
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public List<TaskComment> Replies { get; set; } = new();
    }

    // Task Recurrence
    public class TaskRecurrence
    {
        public RecurrencePattern Pattern { get; set; }
        public int Interval { get; set; } = 1;
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public int? DayOfMonth { get; set; }
        public DateTime? EndDate { get; set; }
        public int? OccurrenceCount { get; set; }
    }

    // Recurrence Pattern Enum
    public enum RecurrencePattern
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    // Project Model
    public class TaskProject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#3b82f6";
        public string? Icon { get; set; }
        public Guid? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public ProjectStatus Status { get; set; }
        public int Progress { get; set; } // 0-100
        public List<Guid> MemberIds { get; set; } = new();
        public int TaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsArchived { get; set; }
    }

    // Project Status Enum
    public enum ProjectStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Cancelled
    }

    // Task Filter
    public class TaskFilter
    {
        public string? SearchQuery { get; set; }
        public List<TaskStatus> Statuses { get; set; } = new();
        public List<TaskPriority> Priorities { get; set; } = new();
        public List<Guid> ProjectIds { get; set; } = new();
        public List<Guid> AssignedToIds { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? IsOverdue { get; set; }
        public bool? IsStarred { get; set; }
        public bool? HasSubtasks { get; set; }
        public bool? IsBlocked { get; set; }
        public bool ShowArchived { get; set; }
    }

    // Task View Mode Enum
    public enum TaskViewMode
    {
        List,
        Kanban,
        Calendar,
        Timeline,
        Table
    }

    // Task Sort By Enum
    public enum TaskSortBy
    {
        Title,
        DueDate,
        Priority,
        Status,
        CreatedDate,
        Progress,
        AssignedTo
    }

    // Task Group By Enum
    public enum TaskGroupBy
    {
        None,
        Status,
        Priority,
        Project,
        AssignedTo,
        DueDate
    }

    // Task Statistics
    public class TaskStatistics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int DueTodayTasks { get; set; }
        public int DueThisWeekTasks { get; set; }
        public int BlockedTasks { get; set; }
        public int UnassignedTasks { get; set; }
        public double AverageCompletionRate { get; set; }
        public double TotalEstimatedHours { get; set; }
        public double TotalActualHours { get; set; }
    }

    // Time Entry Model
    public class TimeEntry
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double Hours { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRunning => !EndTime.HasValue;
    }

    // Task Template
    public class TaskTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskPriority DefaultPriority { get; set; }
        public double? DefaultEstimatedHours { get; set; }
        public List<string> DefaultTags { get; set; } = new();
        public List<string> DefaultLabels { get; set; } = new();
        public List<TaskTemplateSubtask> Subtasks { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public bool IsShared { get; set; }
    }

    // Task Template Subtask
    public class TaskTemplateSubtask
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double? EstimatedHours { get; set; }
        public int Order { get; set; }
    }

    // Task Activity Log
    public class TaskActivity
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public TaskActivityType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Changes { get; set; } = new();
    }

    // Task Activity Type Enum
    public enum TaskActivityType
    {
        Created,
        Updated,
        StatusChanged,
        PriorityChanged,
        AssigneeChanged,
        CommentAdded,
        AttachmentAdded,
        SubtaskAdded,
        DependencyAdded,
        Completed,
        Archived,
        Restored
    }

    // Calendar Day Model
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<TaskItem> Tasks { get; set; } = new();
        public int TaskCount => Tasks.Count;
    }

    // Timeline Group Model
    public class TimelineGroup
    {
        public string Name { get; set; } = string.Empty;
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
