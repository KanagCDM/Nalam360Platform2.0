namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Represents a node in a workflow diagram.
    /// </summary>
    public class WorkflowNode
    {
        /// <summary>
        /// Gets or sets the unique identifier for the node.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the node type.
        /// </summary>
        public NodeType Type { get; set; } = NodeType.Task;

        /// <summary>
        /// Gets or sets the node label/title.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the node description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the X position on canvas.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y position on canvas.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the node width.
        /// </summary>
        public double Width { get; set; } = 180;

        /// <summary>
        /// Gets or sets the node height.
        /// </summary>
        public double Height { get; set; } = 80;

        /// <summary>
        /// Gets or sets the node status.
        /// </summary>
        public NodeStatus Status { get; set; } = NodeStatus.Pending;

        /// <summary>
        /// Gets or sets the node configuration data.
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of input port IDs.
        /// </summary>
        public List<string> InputPorts { get; set; } = new() { "input" };

        /// <summary>
        /// Gets or sets the list of output port IDs.
        /// </summary>
        public List<string> OutputPorts { get; set; } = new() { "output" };

        /// <summary>
        /// Gets or sets whether the node is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets whether the node is being dragged.
        /// </summary>
        public bool IsDragging { get; set; }

        /// <summary>
        /// Gets or sets custom CSS class for styling.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets or sets the node icon.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the node color.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets validation errors for the node.
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Gets or sets execution metadata.
        /// </summary>
        public NodeExecutionMetadata? ExecutionMetadata { get; set; }

        /// <summary>
        /// Gets whether the node is valid.
        /// </summary>
        public bool IsValid => !ValidationErrors.Any();

        /// <summary>
        /// Gets the default icon for the node type.
        /// </summary>
        public string GetDefaultIcon()
        {
            return Type switch
            {
                NodeType.Start => "▶️",
                NodeType.End => "⏹️",
                NodeType.Task => "📋",
                NodeType.Decision => "🔀",
                NodeType.Parallel => "⫴",
                NodeType.Subprocess => "🔄",
                NodeType.Wait => "⏸️",
                NodeType.Trigger => "⚡",
                NodeType.Notification => "📢",
                NodeType.Script => "💻",
                NodeType.Api => "🌐",
                NodeType.Database => "🗄️",
                NodeType.Email => "📧",
                NodeType.Approval => "✓",
                _ => "📋"
            };
        }

        /// <summary>
        /// Gets the default color for the node type.
        /// </summary>
        public string GetDefaultColor()
        {
            return Type switch
            {
                NodeType.Start => "#22c55e",
                NodeType.End => "#ef4444",
                NodeType.Task => "#3b82f6",
                NodeType.Decision => "#f59e0b",
                NodeType.Parallel => "#8b5cf6",
                NodeType.Subprocess => "#06b6d4",
                NodeType.Wait => "#64748b",
                NodeType.Trigger => "#eab308",
                NodeType.Notification => "#ec4899",
                NodeType.Script => "#6366f1",
                NodeType.Api => "#10b981",
                NodeType.Database => "#0ea5e9",
                NodeType.Email => "#f97316",
                NodeType.Approval => "#84cc16",
                _ => "#3b82f6"
            };
        }
    }

    /// <summary>
    /// Represents an edge/connection between workflow nodes.
    /// </summary>
    public class WorkflowEdge
    {
        /// <summary>
        /// Gets or sets the unique identifier for the edge.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the source node ID.
        /// </summary>
        public string SourceNodeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source port ID.
        /// </summary>
        public string SourcePort { get; set; } = "output";

        /// <summary>
        /// Gets or sets the target node ID.
        /// </summary>
        public string TargetNodeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target port ID.
        /// </summary>
        public string TargetPort { get; set; } = "input";

        /// <summary>
        /// Gets or sets the edge label.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the edge type.
        /// </summary>
        public EdgeType Type { get; set; } = EdgeType.Default;

        /// <summary>
        /// Gets or sets whether the edge is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the edge condition (for decision nodes).
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Gets or sets custom CSS class for styling.
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Gets or sets the edge color.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the edge style (solid, dashed, dotted).
        /// </summary>
        public string Style { get; set; } = "solid";

        /// <summary>
        /// Gets or sets whether the edge is animated.
        /// </summary>
        public bool IsAnimated { get; set; }
    }

    /// <summary>
    /// Node execution metadata.
    /// </summary>
    public class NodeExecutionMetadata
    {
        /// <summary>
        /// Gets or sets when execution started.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Gets or sets when execution completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets execution duration in milliseconds.
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Gets or sets execution error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets execution result data.
        /// </summary>
        public Dictionary<string, object>? ResultData { get; set; }

        /// <summary>
        /// Gets or sets retry count.
        /// </summary>
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// Workflow definition containing nodes and edges.
    /// </summary>
    public class WorkflowDefinition
    {
        /// <summary>
        /// Gets or sets the workflow ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the workflow name.
        /// </summary>
        public string Name { get; set; } = "Untitled Workflow";

        /// <summary>
        /// Gets or sets the workflow description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the workflow version.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the workflow nodes.
        /// </summary>
        public List<WorkflowNode> Nodes { get; set; } = new();

        /// <summary>
        /// Gets or sets the workflow edges.
        /// </summary>
        public List<WorkflowEdge> Edges { get; set; } = new();

        /// <summary>
        /// Gets or sets workflow metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Gets or sets when the workflow was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the workflow was last modified.
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the workflow status.
        /// </summary>
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

        /// <summary>
        /// Validates the workflow definition.
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // Check for start node
            if (!Nodes.Any(n => n.Type == NodeType.Start))
            {
                errors.Add("Workflow must have at least one Start node");
            }

            // Check for end node
            if (!Nodes.Any(n => n.Type == NodeType.End))
            {
                errors.Add("Workflow must have at least one End node");
            }

            // Check for disconnected nodes
            var connectedNodes = new HashSet<string>();
            foreach (var edge in Edges)
            {
                connectedNodes.Add(edge.SourceNodeId);
                connectedNodes.Add(edge.TargetNodeId);
            }

            var disconnectedNodes = Nodes
                .Where(n => n.Type != NodeType.Start && n.Type != NodeType.End && !connectedNodes.Contains(n.Id))
                .ToList();

            if (disconnectedNodes.Any())
            {
                errors.Add($"{disconnectedNodes.Count} disconnected node(s) found");
            }

            // Check for cycles (simple check)
            // In production, use a proper cycle detection algorithm

            return errors;
        }
    }

    /// <summary>
    /// Node type enumeration.
    /// </summary>
    public enum NodeType
    {
        Start,
        End,
        Task,
        Decision,
        Parallel,
        Subprocess,
        Wait,
        Trigger,
        Notification,
        Script,
        Api,
        Database,
        Email,
        Approval
    }

    /// <summary>
    /// Node status enumeration.
    /// </summary>
    public enum NodeStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped,
        Cancelled
    }

    /// <summary>
    /// Edge type enumeration.
    /// </summary>
    public enum EdgeType
    {
        Default,
        Success,
        Error,
        Conditional,
        Parallel
    }

    /// <summary>
    /// Workflow status enumeration.
    /// </summary>
    public enum WorkflowStatus
    {
        Draft,
        Published,
        Active,
        Paused,
        Archived
    }
}
