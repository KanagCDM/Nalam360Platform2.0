using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// Service for HIPAA compliance and PHI protection in AI operations
/// </summary>
public interface IAIComplianceService
{
    /// <summary>
    /// De-identifies text by removing or masking PHI elements
    /// </summary>
    /// <param name="text">The text to de-identify</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>De-identified text with PHI removed or masked</returns>
    Task<string> DeIdentifyAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects PHI elements in text
    /// </summary>
    /// <param name="text">The text to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of detected PHI elements with positions and confidence scores</returns>
    Task<List<PHIElement>> DetectPHIAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that an AI service endpoint meets data residency requirements
    /// </summary>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the endpoint meets residency requirements, false otherwise</returns>
    Task<bool> ValidateDataResidencyAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has given consent for AI processing of their data
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if consent is granted, false otherwise</returns>
    Task<bool> HasConsentForAIProcessingAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Audits an AI operation for compliance tracking
    /// </summary>
    /// <param name="operation">The operation performed (e.g., "IntentAnalysis", "Prediction")</param>
    /// <param name="userId">The user who initiated the operation</param>
    /// <param name="dataProcessed">Summary of data processed (should be de-identified)</param>
    /// <param name="result">Summary of the result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AuditAIOperationAsync(
        string operation,
        string userId,
        string dataProcessed,
        string result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that data is encrypted in transit and at rest
    /// </summary>
    /// <param name="endpoint">The AI service endpoint URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if encryption requirements are met, false otherwise</returns>
    Task<bool> ValidateEncryptionAsync(
        string endpoint,
        CancellationToken cancellationToken = default);
}
