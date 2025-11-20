namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Metadata about an ML model
/// </summary>
public class ModelMetadata
{
    /// <summary>
    /// Unique identifier for the model
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Model version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Model name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Model description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Model type (Classification, Regression, Clustering, etc.)
    /// </summary>
    public string ModelType { get; set; } = string.Empty;

    /// <summary>
    /// Input features the model expects
    /// </summary>
    public List<string> InputFeatures { get; set; } = new();

    /// <summary>
    /// Output labels/predictions the model produces
    /// </summary>
    public List<string> OutputLabels { get; set; } = new();

    /// <summary>
    /// Model accuracy metrics
    /// </summary>
    public Dictionary<string, double> Metrics { get; set; } = new();

    /// <summary>
    /// When the model was trained
    /// </summary>
    public DateTime TrainedAt { get; set; }

    /// <summary>
    /// When the model was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Whether the model is currently loaded in memory
    /// </summary>
    public bool IsLoaded { get; set; }

    /// <summary>
    /// File path to the model
    /// </summary>
    public string? ModelPath { get; set; }
}
