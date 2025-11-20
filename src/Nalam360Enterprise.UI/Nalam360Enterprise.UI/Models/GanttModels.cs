using System;
using System.Collections.Generic;
using System.Linq;

namespace Nalam360Enterprise.UI.Models;

/// <summary>
/// Represents a task in a Gantt chart
/// </summary>
public class GanttTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Progress { get; set; } // 0-100
    public string? ParentId { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToAvatar { get; set; }
    public GanttTaskPriority Priority { get; set; } = GanttTaskPriority.Medium;
    public GanttTaskStatus Status { get; set; } = GanttTaskStatus.NotStarted;
    public string? Color { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public List<GanttTaskResource> Resources { get; set; } = new();
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public int Order { get; set; }
    public bool IsCollapsed { get; set; }
    public bool IsMilestone { get; set; }
    public bool IsExpanded { get; set; } = true;

    public int Duration => (EndDate - StartDate).Days + 1;
    public bool IsOverdue => EndDate < DateTime.Now && Progress < 100;
    public bool HasChildren(List<GanttTask> allTasks) => allTasks.Any(t => t.ParentId == Id);
    public List<GanttTask> GetChildren(List<GanttTask> allTasks) => allTasks.Where(t => t.ParentId == Id).OrderBy(t => t.Order).ToList();

    public string GetPriorityColor() => Priority switch
    {
        GanttTaskPriority.Critical => "#d32f2f",
        GanttTaskPriority.High => "#f57c00",
        GanttTaskPriority.Medium => "#fbc02d",
        GanttTaskPriority.Low => "#388e3c",
        _ => "#757575"
    };

    public string GetStatusColor() => Status switch
    {
        GanttTaskStatus.NotStarted => "#9e9e9e",
        GanttTaskStatus.InProgress => "#2196f3",
        GanttTaskStatus.OnHold => "#ff9800",
        GanttTaskStatus.Completed => "#4caf50",
        GanttTaskStatus.Cancelled => "#f44336",
        _ => "#757575"
    };
}

/// <summary>
/// Task priority levels
/// </summary>
public enum GanttTaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Task status options
/// </summary>
public enum GanttTaskStatus
{
    NotStarted,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}

/// <summary>
/// Resource assigned to a task
/// </summary>
public class GanttTaskResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? Role { get; set; }
    public double AllocationPercentage { get; set; } = 100;
}

/// <summary>
/// Dependency between tasks
/// </summary>
public class GanttDependency
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FromTaskId { get; set; } = string.Empty;
    public string ToTaskId { get; set; } = string.Empty;
    public GanttDependencyType Type { get; set; } = GanttDependencyType.FinishToStart;
    public int LagDays { get; set; } = 0;
}

/// <summary>
/// Dependency types
/// </summary>
public enum GanttDependencyType
{
    FinishToStart,
    StartToStart,
    FinishToFinish,
    StartToFinish
}

/// <summary>
/// Main Gantt chart container
/// </summary>
public class GanttChart
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }
    public List<GanttTask> Tasks { get; set; } = new();
    public List<GanttDependency> Dependencies { get; set; } = new();
    public GanttViewMode ViewMode { get; set; } = GanttViewMode.Days;
    public Dictionary<string, object> Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public List<GanttTask> GetRootTasks() => Tasks.Where(t => string.IsNullOrEmpty(t.ParentId)).OrderBy(t => t.Order).ToList();
    
    public GanttTask? GetTask(string taskId) => Tasks.FirstOrDefault(t => t.Id == taskId);
    
    public double GetOverallProgress()
    {
        if (!Tasks.Any()) return 0;
        return Tasks.Average(t => t.Progress);
    }

    public int GetTotalDuration() => (ProjectEndDate - ProjectStartDate).Days + 1;
    
    public List<GanttTask> GetCriticalPath()
    {
        // Simplified critical path - tasks with Critical priority or dependencies
        return Tasks.Where(t => t.Priority == GanttTaskPriority.Critical || t.Dependencies.Any()).ToList();
    }
}

/// <summary>
/// View mode for Gantt chart timeline
/// </summary>
public enum GanttViewMode
{
    Hours,
    Days,
    Weeks,
    Months,
    Quarters,
    Years
}

/// <summary>
/// Filter options for Gantt chart
/// </summary>
public class GanttFilter
{
    public string? SearchText { get; set; }
    public List<GanttTaskPriority> Priorities { get; set; } = new();
    public List<GanttTaskStatus> Statuses { get; set; } = new();
    public List<string> AssignedUsers { get; set; } = new();
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public bool? ShowOnlyOverdue { get; set; }
    public bool? ShowOnlyMilestones { get; set; }
}

/// <summary>
/// Event args for task updates
/// </summary>
public class GanttTaskUpdatedEventArgs
{
    public GanttTask Task { get; set; } = null!;
    public string PropertyName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}

/// <summary>
/// Event args for task drag events
/// </summary>
public class GanttTaskDraggedEventArgs
{
    public GanttTask Task { get; set; } = null!;
    public DateTime OldStartDate { get; set; }
    public DateTime OldEndDate { get; set; }
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }
}
