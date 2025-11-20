namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Result from ML model prediction
/// </summary>
public class MLPredictionResult
{
    /// <summary>
    /// The prediction score (e.g., probability, value)
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// The confidence level of the prediction (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// The predicted label or class (if applicable)
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Whether the prediction was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if prediction failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional prediction metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
