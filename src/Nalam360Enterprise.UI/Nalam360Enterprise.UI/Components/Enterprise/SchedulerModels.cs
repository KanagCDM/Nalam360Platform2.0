using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Represents a scheduled event/appointment
/// </summary>
public class SchedulerEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public string? ResourceId { get; set; } // Person, room, equipment, etc.
    public string? ResourceName { get; set; }
    public string? Category { get; set; } // Meeting, Appointment, Task, etc.
    public string? Color { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    public EventPriority Priority { get; set; } = EventPriority.Normal;
    public bool IsRecurring { get; set; }
    public RecurrencePattern? RecurrencePattern { get; set; }
    public string? RecurrenceException { get; set; } // Dates to exclude
    public string? RecurrenceId { get; set; } // Link to parent recurring event
    public List<string> Attendees { get; set; } = new();
    public string? OrganizerName { get; set; }
    public string? OrganizerEmail { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public List<SchedulerReminder> Reminders { get; set; } = new();
    public bool IsLocked { get; set; } // Prevent editing/deletion
}

/// <summary>
/// Event status enumeration
/// </summary>
public enum EventStatus
{
    Scheduled,
    Confirmed,
    Tentative,
    Cancelled,
    Completed,
    NoShow
}

/// <summary>
/// Event priority enumeration
/// </summary>
public enum EventPriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// Recurrence pattern for recurring events
/// </summary>
public class RecurrencePattern
{
    public RecurrenceType Type { get; set; } = RecurrenceType.Daily;
    public int Interval { get; set; } = 1; // Every N days/weeks/months/years
    public List<DayOfWeek> DaysOfWeek { get; set; } = new(); // For weekly recurrence
    public int? DayOfMonth { get; set; } // For monthly recurrence
    public int? MonthOfYear { get; set; } // For yearly recurrence
    public DateTime? EndDate { get; set; }
    public int? Occurrences { get; set; } // Number of occurrences
}

/// <summary>
/// Recurrence type enumeration
/// </summary>
public enum RecurrenceType
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

/// <summary>
/// Event reminder configuration
/// </summary>
public class SchedulerReminder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int MinutesBefore { get; set; } = 15;
    public ReminderMethod Method { get; set; } = ReminderMethod.Notification;
}

/// <summary>
/// Reminder delivery method
/// </summary>
public enum ReminderMethod
{
    Notification,
    Email,
    SMS
}

/// <summary>
/// Represents a resource (person, room, equipment) that can be scheduled
/// </summary>
public class SchedulerResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public ResourceType Type { get; set; } = ResourceType.Person;
    public string? Color { get; set; }
    public string? Department { get; set; }
    public string? Title { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<WorkingHours> WorkingHours { get; set; } = new();
    public List<TimeOff> TimeOff { get; set; } = new();
    public int MaxConcurrentEvents { get; set; } = 1;
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

/// <summary>
/// Resource type enumeration
/// </summary>
public enum ResourceType
{
    Person,
    Room,
    Equipment,
    Vehicle,
    Other
}

/// <summary>
/// Working hours for a resource
/// </summary>
public class WorkingHours
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);
    public bool IsWorkingDay { get; set; } = true;
}

/// <summary>
/// Time off/unavailability period for a resource
/// </summary>
public class TimeOff
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public TimeOffType Type { get; set; } = TimeOffType.Vacation;
}

/// <summary>
/// Time off type enumeration
/// </summary>
public enum TimeOffType
{
    Vacation,
    Sick,
    Personal,
    Holiday,
    Training,
    Other
}

/// <summary>
/// Scheduler view mode
/// </summary>
public enum SchedulerViewMode
{
    Day,
    Week,
    WorkWeek,
    Month,
    Agenda,
    Timeline
}

/// <summary>
/// Conflict detection result
/// </summary>
public class ScheduleConflict
{
    public string EventId { get; set; } = string.Empty;
    public string ConflictingEventId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime ConflictStart { get; set; }
    public DateTime ConflictEnd { get; set; }
    public ConflictType Type { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Conflict type enumeration
/// </summary>
public enum ConflictType
{
    DoubleBooking,
    OutsideWorkingHours,
    ResourceUnavailable,
    MaxCapacityExceeded
}

/// <summary>
/// Time slot for availability checking
/// </summary>
public class TimeSlot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ResourceId { get; set; }
    public List<string> ConflictingEventIds { get; set; } = new();
}

/// <summary>
/// Scheduler settings/configuration
/// </summary>
public class SchedulerSettings
{
    public TimeSpan StartHour { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan EndHour { get; set; } = new TimeSpan(18, 0, 0);
    public int TimeSlotDuration { get; set; } = 30; // minutes
    public int WorkDays { get; set; } = 5; // Monday-Friday
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;
    public bool ShowWeekends { get; set; } = false;
    public bool ShowTimeOffPeriods { get; set; } = true;
    public bool EnableConflictDetection { get; set; } = true;
    public bool AllowDoubleBooking { get; set; } = false;
    public bool EnableDragDrop { get; set; } = true;
    public bool EnableResize { get; set; } = true;
    public string DateFormat { get; set; } = "MMM dd, yyyy";
    public string TimeFormat { get; set; } = "h:mm tt";
}

/// <summary>
/// Event drag/drop result
/// </summary>
public class EventDragResult
{
    public SchedulerEvent Event { get; set; } = new();
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
    public string? NewResourceId { get; set; }
    public List<ScheduleConflict> Conflicts { get; set; } = new();
    public bool IsValid { get; set; } = true;
}
