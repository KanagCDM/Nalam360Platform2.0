using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// HIPAA compliance service for PHI detection and de-identification
/// </summary>
public interface IAIComplianceService
{
    Task<List<PHIElement>> DetectPHIAsync(string text, CancellationToken cancellationToken = default);
    Task<string> DeIdentifyAsync(string text, List<PHIElement> phiElements, CancellationToken cancellationToken = default);
    Task AuditAIOperationAsync(string operation, string? userId, string input, string output, CancellationToken cancellationToken = default);
}
