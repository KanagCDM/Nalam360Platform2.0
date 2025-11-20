using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Models;
using Nalam360Enterprise.UI.Core.Security;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// HIPAA compliance service for PHI protection in AI operations
/// </summary>
public class HIPAAComplianceService : IAIComplianceService
{
    private readonly ILogger<HIPAAComplianceService> _logger;
    private readonly IAuditService? _auditService;
    
    // Regex patterns for PHI detection
    private static readonly Regex MrnPattern = new(@"\b(MRN|Medical Record Number)[\s:#]*([A-Z0-9]{6,12})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SsnPattern = new(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(@"\b(?:\+?1[-.]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex DatePattern = new(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{2,4}\b", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex AddressPattern = new(@"\b\d+\s+[\w\s]+(?:street|st|avenue|ave|road|rd|boulevard|blvd|lane|ln|drive|dr|court|ct|circle|cir)[\w\s]*,?\s*(?:apt|apartment|unit|suite|ste)?\.?\s*\#?\d*\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    // Common name patterns (simplified - in production, use NER model)
    private static readonly string[] CommonFirstNames = { "james", "john", "robert", "michael", "william", "david", "richard", "joseph", "thomas", "mary", "patricia", "jennifer", "linda", "elizabeth", "barbara", "susan", "jessica", "sarah", "karen" };
    
    // Allowed geographic regions for data residency (configurable)
    private readonly HashSet<string> _allowedRegions = new(StringComparer.OrdinalIgnoreCase)
    {
        "us", "usa", "united-states", "us-east", "us-west", "us-central", "us-north", "us-south"
    };

    public HIPAAComplianceService(
        ILogger<HIPAAComplianceService> logger,
        IAuditService? auditService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService;
    }

    /// <inheritdoc/>
    public async Task<string> DeIdentifyAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        _logger.LogInformation("De-identifying text of length {Length}", text.Length);

        var phiElements = await DetectPHIAsync(text, cancellationToken);
        var deidentified = text;

        // Replace PHI elements with their suggested replacements (in reverse order to maintain positions)
        foreach (var phi in phiElements.OrderByDescending(p => p.StartPosition))
        {
            deidentified = deidentified.Remove(phi.StartPosition, phi.EndPosition - phi.StartPosition);
            deidentified = deidentified.Insert(phi.StartPosition, phi.SuggestedReplacement);
        }

        _logger.LogInformation("De-identification complete. Removed {Count} PHI elements", phiElements.Count);

        return deidentified;
    }

    /// <inheritdoc/>
    public async Task<List<PHIElement>> DetectPHIAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<PHIElement>();

        _logger.LogDebug("Detecting PHI in text of length {Length}", text.Length);

        var phiElements = new List<PHIElement>();

        // Detect MRNs
        foreach (Match match in MrnPattern.Matches(text))
        {
            phiElements.Add(new PHIElement
            {
                Type = "MRN",
                Value = match.Groups[2].Value,
                StartPosition = match.Groups[2].Index,
                EndPosition = match.Groups[2].Index + match.Groups[2].Length,
                Confidence = 0.95,
                SuggestedReplacement = "[MRN]"
            });
        }

        // Detect SSNs
        foreach (Match match in SsnPattern.Matches(text))
        {
            phiElements.Add(new PHIElement
            {
                Type = "SSN",
                Value = match.Value,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                Confidence = 0.98,
                SuggestedReplacement = "[SSN]"
            });
        }

        // Detect phone numbers
        foreach (Match match in PhonePattern.Matches(text))
        {
            phiElements.Add(new PHIElement
            {
                Type = "PHONE",
                Value = match.Value,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                Confidence = 0.85,
                SuggestedReplacement = "[PHONE]"
            });
        }

        // Detect dates
        foreach (Match match in DatePattern.Matches(text))
        {
            phiElements.Add(new PHIElement
            {
                Type = "DATE",
                Value = match.Value,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                Confidence = 0.80,
                SuggestedReplacement = "[DATE]"
            });
        }

        // Detect emails
        foreach (Match match in EmailPattern.Matches(text))
        {
            phiElements.Add(new PHIElement
            {
                Type = "EMAIL",
                Value = match.Value,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                Confidence = 0.90,
                SuggestedReplacement = "[EMAIL]"
            });
        }

        // Detect addresses
        foreach (Match match in AddressPattern.Matches(text))
        {
            phiElements.Add(new PHIElement
            {
                Type = "ADDRESS",
                Value = match.Value,
                StartPosition = match.Index,
                EndPosition = match.Index + match.Length,
                Confidence = 0.75,
                SuggestedReplacement = "[ADDRESS]"
            });
        }

        // Detect common names (simplified - in production, use NER)
        var words = text.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (CommonFirstNames.Contains(word.ToLower()) && char.IsUpper(word[0]))
            {
                var index = text.IndexOf(word, StringComparison.Ordinal);
                if (index >= 0 && !phiElements.Any(p => p.StartPosition <= index && p.EndPosition >= index + word.Length))
                {
                    phiElements.Add(new PHIElement
                    {
                        Type = "NAME",
                        Value = word,
                        StartPosition = index,
                        EndPosition = index + word.Length,
                        Confidence = 0.60,
                        SuggestedReplacement = "[PATIENT]"
                    });
                }
            }
        }

        _logger.LogDebug("Detected {Count} PHI elements", phiElements.Count);

        return await Task.FromResult(phiElements.OrderBy(p => p.StartPosition).ToList());
    }

    /// <inheritdoc/>
    public Task<bool> ValidateDataResidencyAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _logger.LogWarning("Empty endpoint provided for data residency validation");
            return Task.FromResult(false);
        }

        try
        {
            var uri = new Uri(endpoint);
            var host = uri.Host.ToLower();

            // Check if host contains allowed region identifiers
            var isAllowed = _allowedRegions.Any(region => host.Contains(region));

            _logger.LogInformation(
                "Data residency validation for {Endpoint}: {Result}",
                endpoint,
                isAllowed ? "PASSED" : "FAILED");

            return Task.FromResult(isAllowed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data residency for endpoint: {Endpoint}", endpoint);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public Task<bool> HasConsentForAIProcessingAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Empty userId provided for consent check");
            return Task.FromResult(false);
        }

        // TODO: Implement actual consent storage/retrieval
        // For now, return true (assumes consent is managed elsewhere)
        _logger.LogDebug("Checking AI processing consent for user: {UserId}", userId);
        
        // In production, query a consent management system:
        // return await _consentRepository.HasConsentAsync(userId, "AI_PROCESSING", cancellationToken);
        
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public async Task AuditAIOperationAsync(
        string operation,
        string userId,
        string dataProcessed,
        string result,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Auditing AI operation: {Operation} by user: {UserId}",
                operation,
                userId);

            // De-identify the audit data
            var deidentifiedData = await DeIdentifyAsync(dataProcessed, cancellationToken);
            var deidentifiedResult = await DeIdentifyAsync(result, cancellationToken);

            // Use the audit service if available
            if (_auditService != null)
            {
                await _auditService.LogAsync(
                    userId,
                    $"AI_{operation}",
                    "AIService",
                    new
                    {
                        Operation = operation,
                        DataLength = dataProcessed.Length,
                        ResultLength = result.Length,
                        DeidentifiedData = deidentifiedData.Length > 200 
                            ? deidentifiedData.Substring(0, 200) + "..." 
                            : deidentifiedData,
                        DeidentifiedResult = deidentifiedResult.Length > 200 
                            ? deidentifiedResult.Substring(0, 200) + "..." 
                            : deidentifiedResult,
                        Timestamp = DateTime.UtcNow
                    });
            }
            else
            {
                // Fallback logging
                _logger.LogInformation(
                    "AI Operation Audit - Operation: {Operation}, User: {UserId}, DataLength: {DataLength}, ResultLength: {ResultLength}",
                    operation,
                    userId,
                    dataProcessed.Length,
                    result.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auditing AI operation: {Operation}", operation);
        }
    }

    /// <inheritdoc/>
    public Task<bool> ValidateEncryptionAsync(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _logger.LogWarning("Empty endpoint provided for encryption validation");
            return Task.FromResult(false);
        }

        try
        {
            var uri = new Uri(endpoint);
            
            // Must use HTTPS (TLS)
            var isSecure = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Encryption validation for {Endpoint}: {Result}",
                endpoint,
                isSecure ? "PASSED" : "FAILED - Must use HTTPS");

            return Task.FromResult(isSecure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating encryption for endpoint: {Endpoint}", endpoint);
            return Task.FromResult(false);
        }
    }
}
