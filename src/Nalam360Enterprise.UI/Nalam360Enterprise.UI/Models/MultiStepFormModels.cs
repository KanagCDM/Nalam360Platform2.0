using System;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Models
{
    /// <summary>
    /// Represents a step in a multi-step form
    /// </summary>
    public class FormStep
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsValid { get; set; } = true;
        public bool IsCompleted { get; set; }
        public bool IsOptional { get; set; }
        public bool IsSkipped { get; set; }
        public Dictionary<string, object?> Data { get; set; } = new();
        public List<string> ValidationErrors { get; set; } = new();
        public int Order { get; set; }
    }

    /// <summary>
    /// Multi-step form configuration
    /// </summary>
    public class MultiStepFormConfig
    {
        public List<FormStep> Steps { get; set; } = new();
        public StepperOrientation Orientation { get; set; } = StepperOrientation.Horizontal;
        public bool ShowStepNumbers { get; set; } = true;
        public bool ShowProgressBar { get; set; } = true;
        public bool AllowStepSkipping { get; set; }
        public bool ValidateOnStepChange { get; set; } = true;
        public bool SaveDraft { get; set; }
        public int AutoSaveDraftInterval { get; set; } = 30000; // 30 seconds
    }

    /// <summary>
    /// Stepper orientation
    /// </summary>
    public enum StepperOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Form navigation direction
    /// </summary>
    public enum NavigationDirection
    {
        Next,
        Previous,
        Jump
    }

    /// <summary>
    /// Step change event data
    /// </summary>
    public class StepChangeEvent
    {
        public int FromStep { get; set; }
        public int ToStep { get; set; }
        public NavigationDirection Direction { get; set; }
        public bool IsCancelled { get; set; }
        public string? CancelReason { get; set; }
    }

    /// <summary>
    /// Form submission data
    /// </summary>
    public class FormSubmissionData
    {
        public Dictionary<string, Dictionary<string, object?>> StepData { get; set; } = new();
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public List<string> CompletedSteps { get; set; } = new();
        public List<string> SkippedSteps { get; set; } = new();
    }
}
