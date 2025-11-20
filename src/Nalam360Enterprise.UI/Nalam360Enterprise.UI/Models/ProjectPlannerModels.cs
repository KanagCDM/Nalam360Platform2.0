using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nalam360Enterprise.UI.Models
{
    /// <summary>
    /// Represents a project with tasks, resources, milestones, and budget tracking
    /// </summary>
    public class Project
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public ProjectStatus Status { get; set; }
        
        public ProjectPriority Priority { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public DateTime? ActualEndDate { get; set; }
        
        public Guid? ManagerId { get; set; }
        
        public string ManagerName { get; set; } = string.Empty;
        
        public decimal Budget { get; set; }
        
        public decimal ActualCost { get; set; }
        
        public decimal PlannedCost { get; set; }
        
        public string Currency { get; set; } = "USD";
        
        public int ProgressPercentage { get; set; }
        
        public List<ProjectTask> Tasks { get; set; } = new();
        
        public List<ProjectResource> Resources { get; set; } = new();
        
        public List<ProjectMilestone> Milestones { get; set; } = new();
        
        public List<ProjectRisk> Risks { get; set; } = new();
        
        public List<ProjectDependency> Dependencies { get; set; } = new();
        
        public List<string> Tags { get; set; } = new();
        
        public string Color { get; set; } = "#3b82f6";
        
        public string Category { get; set; } = string.Empty;
        
        public Guid? ParentProjectId { get; set; }
        
        public List<Project> SubProjects { get; set; } = new();
        
        public Dictionary<string, object> CustomFields { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        
        public Guid CreatedBy { get; set; }
        
        public DateTime ModifiedAt { get; set; }
        
        public Guid ModifiedBy { get; set; }
        
        // Calculated properties
        public int TotalTasks => Tasks.Count;
        public int CompletedTasks => Tasks.Count(t => t.Status == ProjectTaskStatus.Completed);
        public int OverdueTasks => Tasks.Count(t => t.IsOverdue);
        public decimal BudgetVariance => Budget - ActualCost;
        public decimal BudgetVariancePercentage => Budget > 0 ? (BudgetVariance / Budget) * 100 : 0;
        public bool IsOverBudget => ActualCost > Budget;
        public bool IsDelayed => EndDate.HasValue && DateTime.Now > EndDate.Value && Status != ProjectStatus.Completed;
        public TimeSpan? Duration => EndDate.HasValue ? EndDate.Value - StartDate : null;
    }
    
    /// <summary>
    /// Represents a task within a project
    /// </summary>
    public class ProjectTask
    {
        public Guid Id { get; set; }
        
        public Guid ProjectId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public ProjectTaskStatus Status { get; set; }
        
        public ProjectTaskPriority Priority { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public DateTime? ActualStartDate { get; set; }
        
        public DateTime? ActualEndDate { get; set; }
        
        public int Duration { get; set; } // in days
        
        public int ProgressPercentage { get; set; }
        
        public List<Guid> AssignedResourceIds { get; set; } = new();
        
        public List<string> AssignedResourceNames { get; set; } = new();
        
        public decimal EstimatedHours { get; set; }
        
        public decimal ActualHours { get; set; }
        
        public decimal Cost { get; set; }
        
        public Guid? ParentTaskId { get; set; }
        
        public List<ProjectTask> Subtasks { get; set; } = new();
        
        public List<ProjectTaskDependency> Dependencies { get; set; } = new();
        
        public bool IsMilestone { get; set; }
        
        public bool IsCriticalPath { get; set; }
        
        public string Color { get; set; } = "#3b82f6";
        
        public List<string> Tags { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime ModifiedAt { get; set; }
        
        // Calculated properties
        public bool IsOverdue => DateTime.Now > EndDate && Status != ProjectTaskStatus.Completed;
        public bool IsCompleted => Status == ProjectTaskStatus.Completed;
        public int DelayDays => IsOverdue ? (DateTime.Now - EndDate).Days : 0;
        public decimal HoursVariance => EstimatedHours - ActualHours;
    }
    
    /// <summary>
    /// Represents a resource (person, equipment, material) assigned to projects
    /// </summary>
    public class ProjectResource
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public ResourceType Type { get; set; }
        
        public string Role { get; set; } = string.Empty;
        
        public string Department { get; set; } = string.Empty;
        
        public decimal CostPerHour { get; set; }
        
        public decimal Availability { get; set; } = 100; // percentage
        
        public decimal AllocatedHours { get; set; }
        
        public decimal ActualHours { get; set; }
        
        public List<ResourceAllocation> Allocations { get; set; } = new();
        
        public string Skills { get; set; } = string.Empty;
        
        public string AvatarUrl { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? AvailableFrom { get; set; }
        
        public DateTime? AvailableTo { get; set; }
        
        public Dictionary<string, object> CustomFields { get; set; } = new();
        
        // Calculated properties
        public decimal UtilizationPercentage => Availability > 0 ? (AllocatedHours / Availability) * 100 : 0;
        public bool IsOverallocated => AllocatedHours > Availability;
        public decimal RemainingCapacity => Availability - AllocatedHours;
    }
    
    /// <summary>
    /// Represents resource allocation to a task or project
    /// </summary>
    public class ResourceAllocation
    {
        public Guid Id { get; set; }
        
        public Guid ResourceId { get; set; }
        
        public Guid? TaskId { get; set; }
        
        public Guid? ProjectId { get; set; }
        
        public string TaskName { get; set; } = string.Empty;
        
        public string ProjectName { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public decimal AllocatedHours { get; set; }
        
        public decimal AllocationPercentage { get; set; }
        
        public string Notes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Represents a project milestone
    /// </summary>
    public class ProjectMilestone
    {
        public Guid Id { get; set; }
        
        public Guid ProjectId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime DueDate { get; set; }
        
        public DateTime? ActualDate { get; set; }
        
        public MilestoneStatus Status { get; set; }
        
        public int ProgressPercentage { get; set; }
        
        public List<Guid> DependentTaskIds { get; set; } = new();
        
        public string Color { get; set; } = "#10b981";
        
        public bool IsCompleted => Status == MilestoneStatus.Achieved;
        public bool IsOverdue => DateTime.Now > DueDate && !IsCompleted;
    }
    
    /// <summary>
    /// Represents a project risk
    /// </summary>
    public class ProjectRisk
    {
        public Guid Id { get; set; }
        
        public Guid ProjectId { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public RiskCategory Category { get; set; }
        
        public RiskProbability Probability { get; set; }
        
        public RiskImpact Impact { get; set; }
        
        public RiskStatus Status { get; set; }
        
        public string MitigationPlan { get; set; } = string.Empty;
        
        public Guid? OwnerId { get; set; }
        
        public string OwnerName { get; set; } = string.Empty;
        
        public DateTime IdentifiedDate { get; set; }
        
        public DateTime? MitigatedDate { get; set; }
        
        public DateTime? ReviewDate { get; set; }
        
        // Calculated properties
        public int RiskScore => (int)Probability * (int)Impact;
        public RiskLevel RiskLevel
        {
            get
            {
                var score = RiskScore;
                if (score >= 15) return RiskLevel.Critical;
                if (score >= 10) return RiskLevel.High;
                if (score >= 5) return RiskLevel.Medium;
                return RiskLevel.Low;
            }
        }
    }
    
    /// <summary>
    /// Represents a dependency between projects
    /// </summary>
    public class ProjectDependency
    {
        public Guid Id { get; set; }
        
        public Guid ProjectId { get; set; }
        
        public Guid DependentProjectId { get; set; }
        
        public string DependentProjectName { get; set; } = string.Empty;
        
        public DependencyType Type { get; set; }
        
        public int LagDays { get; set; }
        
        public string Notes { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Represents a dependency between tasks
    /// </summary>
    public class ProjectTaskDependency
    {
        public Guid Id { get; set; }
        
        public Guid TaskId { get; set; }
        
        public Guid DependentTaskId { get; set; }
        
        public string DependentTaskName { get; set; } = string.Empty;
        
        public DependencyType Type { get; set; }
        
        public int LagDays { get; set; }
    }
    
    /// <summary>
    /// Project filtering options
    /// </summary>
    public class ProjectFilter
    {
        public string SearchQuery { get; set; } = string.Empty;
        
        public List<ProjectStatus> Statuses { get; set; } = new();
        
        public List<ProjectPriority> Priorities { get; set; } = new();
        
        public List<Guid> ManagerIds { get; set; } = new();
        
        public List<string> Categories { get; set; } = new();
        
        public List<string> Tags { get; set; } = new();
        
        public DateTime? StartDateFrom { get; set; }
        
        public DateTime? StartDateTo { get; set; }
        
        public DateTime? EndDateFrom { get; set; }
        
        public DateTime? EndDateTo { get; set; }
        
        public decimal? BudgetMin { get; set; }
        
        public decimal? BudgetMax { get; set; }
        
        public int? ProgressMin { get; set; }
        
        public int? ProgressMax { get; set; }
        
        public bool? ShowOverBudget { get; set; }
        
        public bool? ShowDelayed { get; set; }
        
        public bool? ShowCompleted { get; set; }
    }
    
    /// <summary>
    /// Project statistics
    /// </summary>
    public class ProjectStatistics
    {
        public int TotalProjects { get; set; }
        
        public int ActiveProjects { get; set; }
        
        public int CompletedProjects { get; set; }
        
        public int DelayedProjects { get; set; }
        
        public int OverBudgetProjects { get; set; }
        
        public decimal TotalBudget { get; set; }
        
        public decimal TotalActualCost { get; set; }
        
        public decimal AverageProgress { get; set; }
        
        public int TotalTasks { get; set; }
        
        public int CompletedTasks { get; set; }
        
        public int OverdueTasks { get; set; }
        
        public int TotalResources { get; set; }
        
        public int OverallocatedResources { get; set; }
    }
    
    // Note: ProjectStatus enum is defined in TaskManagerModels.cs
    
    public enum ProjectPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    public enum ProjectTaskStatus
    {
        NotStarted,
        InProgress,
        OnHold,
        Completed,
        Cancelled,
        Blocked
    }
    
    public enum ProjectTaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    public enum ResourceType
    {
        Human,
        Equipment,
        Material,
        Software,
        Facility
    }
    
    public enum MilestoneStatus
    {
        Pending,
        InProgress,
        Achieved,
        Missed,
        Cancelled
    }
    
    public enum RiskCategory
    {
        Technical,
        Financial,
        Schedule,
        Resource,
        External,
        Quality,
        Scope,
        Communication
    }
    
    public enum RiskProbability
    {
        VeryLow = 1,
        Low = 2,
        Medium = 3,
        High = 4,
        VeryHigh = 5
    }
    
    public enum RiskImpact
    {
        Negligible = 1,
        Minor = 2,
        Moderate = 3,
        Major = 4,
        Severe = 5
    }
    
    public enum RiskStatus
    {
        Identified,
        Analyzing,
        Mitigating,
        Monitoring,
        Closed
    }
    
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    // Note: DependencyType enum is defined in TaskManagerModels.cs
    
    public enum ProjectViewMode
    {
        Gantt,
        Timeline,
        Board,
        List,
        Resource,
        Calendar
    }
    
    public enum ProjectSortBy
    {
        Name,
        StartDate,
        EndDate,
        Priority,
        Status,
        Progress,
        Budget
    }
    
    public enum ProjectGroupBy
    {
        None,
        Status,
        Priority,
        Manager,
        Category
    }
}
