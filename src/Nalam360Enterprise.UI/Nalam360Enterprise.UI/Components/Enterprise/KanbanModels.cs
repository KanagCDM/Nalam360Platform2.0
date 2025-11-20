using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.Enterprise;

/// <summary>
/// Represents a Kanban board card/task
/// </summary>
public class KanbanCard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ColumnId { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToAvatar { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<KanbanAttachment> Attachments { get; set; } = new();
    public List<KanbanComment> Comments { get; set; } = new();
    public List<KanbanChecklist> Checklists { get; set; } = new();
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public string? Color { get; set; }
    public int? EstimatedHours { get; set; }
    public int? ActualHours { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    
    public string GetPriorityColor() => Priority?.ToLower() switch
    {
        "critical" => "#d32f2f",
        "high" => "#f57c00",
        "medium" => "#fbc02d",
        "low" => "#388e3c",
        _ => "#757575"
    };
    
    public string GetPriorityIcon() => Priority?.ToLower() switch
    {
        "critical" => "🔴",
        "high" => "🟠",
        "medium" => "🟡",
        "low" => "🟢",
        _ => "⚪"
    };
}

/// <summary>
/// Represents a Kanban board column
/// </summary>
public class KanbanColumn
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? WipLimit { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsCollapsed { get; set; }
    public bool AllowAddCard { get; set; } = true;
    public Dictionary<string, object> Settings { get; set; } = new();
    
    public int CardCount { get; set; }
    
    public bool IsOverWipLimit => WipLimit.HasValue && CardCount > WipLimit.Value;
}

/// <summary>
/// Represents a Kanban board swimlane
/// </summary>
public class KanbanSwimlane
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsCollapsed { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// Represents a card attachment
/// </summary>
public class KanbanAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = string.Empty;
}

/// <summary>
/// Represents a card comment
/// </summary>
public class KanbanComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
}

/// <summary>
/// Represents a card checklist
/// </summary>
public class KanbanChecklist
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public List<KanbanChecklistItem> Items { get; set; } = new();
    
    public int TotalItems => Items.Count;
    public int CompletedItems => Items.Count(i => i.IsCompleted);
    public double CompletionPercentage => TotalItems > 0 ? (CompletedItems * 100.0 / TotalItems) : 0;
}

/// <summary>
/// Represents a checklist item
/// </summary>
public class KanbanChecklistItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
}

/// <summary>
/// Represents the complete Kanban board configuration
/// </summary>
public class KanbanBoard
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "New Board";
    public string? Description { get; set; }
    public List<KanbanColumn> Columns { get; set; } = new();
    public List<KanbanSwimlane> Swimlanes { get; set; } = new();
    public List<KanbanCard> Cards { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    
    public List<KanbanCard> GetCardsForColumn(string columnId, string? swimlaneId = null)
    {
        var cards = Cards.Where(c => c.ColumnId == columnId);
        
        if (swimlaneId != null)
        {
            // Filter by swimlane if needed (based on custom field or property)
            cards = cards.Where(c => c.CustomFields.ContainsKey("SwimlaneId") && 
                                    c.CustomFields["SwimlaneId"]?.ToString() == swimlaneId);
        }
        
        return cards.OrderBy(c => c.Order).ToList();
    }
}

/// <summary>
/// Represents card move event arguments
/// </summary>
public class CardMovedEventArgs
{
    public KanbanCard Card { get; set; } = null!;
    public string FromColumnId { get; set; } = string.Empty;
    public string ToColumnId { get; set; } = string.Empty;
    public string? FromSwimlaneId { get; set; }
    public string? ToSwimlaneId { get; set; }
    public int NewOrder { get; set; }
}

/// <summary>
/// Represents card filter options
/// </summary>
public class KanbanFilter
{
    public string? SearchText { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Priorities { get; set; } = new();
    public List<string> AssignedUsers { get; set; } = new();
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public bool? IsBlocked { get; set; }
}
