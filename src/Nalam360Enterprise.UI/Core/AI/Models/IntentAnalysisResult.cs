namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Represents the result of intent analysis for a user message
/// </summary>
public class IntentAnalysisResult
{
    /// <summary>
    /// The detected intent category
    /// </summary>
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score for the detected intent (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Additional entities extracted from the message
    /// </summary>
    public Dictionary<string, string> Entities { get; set; } = new();

    /// <summary>
    /// Timestamp when the analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the analysis was successful
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}
