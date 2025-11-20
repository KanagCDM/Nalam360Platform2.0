namespace Nalam360Enterprise.UI.Core.AI.Models;

/// <summary>
/// Represents the result of an ML model prediction
/// </summary>
/// <typeparam name="T">The type of prediction result</typeparam>
public class PredictionResult<T>
{
    /// <summary>
    /// The predicted value
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Confidence score for the prediction (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Model version used for prediction
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the prediction was made
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Feature importance scores (for explainability)
    /// </summary>
    public Dictionary<string, double> FeatureImportance { get; set; } = new();

    /// <summary>
    /// Additional metadata about the prediction
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Optional error message if prediction failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the prediction was successful
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}

/// <summary>
/// Represents the result of batch predictions
/// </summary>
/// <typeparam name="T">The type of prediction result</typeparam>
public class BatchPredictionResult<T>
{
    /// <summary>
    /// Individual prediction results
    /// </summary>
    public List<PredictionResult<T>> Predictions { get; set; } = new();

    /// <summary>
    /// Model version used for predictions
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the batch was processed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of successful predictions
    /// </summary>
    public int SuccessCount => Predictions.Count(p => p.IsSuccess);

    /// <summary>
    /// Number of failed predictions
    /// </summary>
    public int FailureCount => Predictions.Count(p => !p.IsSuccess);

    /// <summary>
    /// Optional error message if batch processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the batch processing was successful
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}
