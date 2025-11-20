using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// Service for ML model predictions and management
/// </summary>
public interface IMLModelService
{
    /// <summary>
    /// Makes a prediction using the specified model
    /// </summary>
    /// <param name="modelId">The unique identifier of the model</param>
    /// <param name="features">The input features for prediction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prediction result with confidence score</returns>
    Task<MLPredictionResult> PredictAsync(
        string modelId,
        Dictionary<string, object> features,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes batch predictions using the specified model
    /// </summary>
    /// <param name="modelId">The unique identifier of the model</param>
    /// <param name="featuresList">The list of input features for batch prediction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch prediction results</returns>
    Task<IEnumerable<MLPredictionResult>> PredictBatchAsync(
        string modelId,
        IEnumerable<Dictionary<string, object>> featuresList,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata about a specific model
    /// </summary>
    /// <param name="modelId">The unique identifier of the model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Model metadata including version, metrics, and features</returns>
    Task<ModelMetadata> GetModelMetadataAsync(
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a model is currently loaded in memory
    /// </summary>
    /// <param name="modelId">The unique identifier of the model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the model is loaded, false otherwise</returns>
    Task<bool> IsModelLoadedAsync(
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Preloads a model into memory for faster predictions
    /// </summary>
    /// <param name="modelId">The unique identifier of the model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PreloadModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unloads a model from memory to free resources
    /// </summary>
    /// <param name="modelId">The unique identifier of the model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UnloadModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);
}
