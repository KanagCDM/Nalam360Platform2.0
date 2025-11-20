namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Represents a detected Protected Health Information (PHI) element
/// </summary>
public class PHIElement
{
    /// <summary>
    /// The type of PHI (e.g., NAME, MRN, DATE, ADDRESS, PHONE, SSN, DEVICE_ID)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The actual PHI value detected
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Start position in the original text
    /// </summary>
    public int StartPosition { get; set; }

    /// <summary>
    /// End position in the original text
    /// </summary>
    public int EndPosition { get; set; }

    /// <summary>
    /// Confidence score for the detection (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Suggested replacement text for de-identification
    /// </summary>
    public string SuggestedReplacement { get; set; } = string.Empty;
}
