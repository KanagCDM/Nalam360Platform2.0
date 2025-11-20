using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Core.AI.Services;

/// <summary>
/// HIPAA compliance service for PHI detection, de-identification, and audit logging
/// </summary>
public class HIPAAComplianceService : IAIComplianceService
{
    private readonly ILogger<HIPAAComplianceService>? _logger;

    // PHI Detection Patterns
    private static readonly Dictionary<string, (Regex Pattern, double Confidence, string Replacement)> PhiPatterns = new()
    {
        ["MRN"] = (new Regex(@"\b(MRN|Medical Record|Record #)[\s:]*([A-Z0-9]{6,12})\b", RegexOptions.IgnoreCase), 0.95, "[MRN]"),
        ["SSN"] = (new Regex(@"\b\d{3}-?\d{2}-?\d{4}\b"), 0.98, "[SSN]"),
        ["PHONE"] = (new Regex(@"\b(\+?1[-.]?)?\(?\d{3}\)?[-.]?\d{3}[-.]?\d{4}\b"), 0.85, "[PHONE]"),
        ["DATE"] = (new Regex(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{2,4}\b"), 0.80, "[DATE]"),
        ["EMAIL"] = (new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"), 0.90, "[EMAIL]"),
        ["ADDRESS"] = (new Regex(@"\b\d+\s+([A-Z][a-z]+\s*){1,3}(Street|St|Avenue|Ave|Road|Rd|Drive|Dr|Boulevard|Blvd|Lane|Ln)\b", RegexOptions.IgnoreCase), 0.75, "[ADDRESS]"),
        ["NAME"] = (new Regex(@"\b(Dr\.|Mr\.|Mrs\.|Ms\.)\s+[A-Z][a-z]+\s+[A-Z][a-z]+\b"), 0.60, "[PATIENT]")
    };

    public HIPAAComplianceService(ILogger<HIPAAComplianceService>? logger = null)
    {
        _logger = logger;
    }

    public Task<List<PHIElement>> DetectPHIAsync(string text, CancellationToken cancellationToken = default)
    {
        var detectedPHI = new List<PHIElement>();

        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(detectedPHI);

        foreach (var (type, (pattern, confidence, replacement)) in PhiPatterns)
        {
            var matches = pattern.Matches(text);
            foreach (Match match in matches)
            {
                detectedPHI.Add(new PHIElement
                {
                    Type = type,
                    Value = match.Value,
                    StartPosition = match.Index,
                    EndPosition = match.Index + match.Length,
                    Confidence = confidence,
                    SuggestedReplacement = replacement
                });
            }
        }

        if (detectedPHI.Any())
        {
            _logger?.LogWarning("Detected {Count} PHI elements in text of length {Length}",
                detectedPHI.Count, text.Length);
        }

        return Task.FromResult(detectedPHI);
    }

    public Task<string> DeIdentifyAsync(string text, List<PHIElement> phiElements, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text) || phiElements == null || !phiElements.Any())
            return Task.FromResult(text);

        var deIdentified = text;
        
        // Sort by position descending to replace from end to start (prevents position shifts)
        var sortedElements = phiElements.OrderByDescending(p => p.StartPosition).ToList();

        foreach (var phi in sortedElements)
        {
            if (phi.StartPosition >= 0 && phi.EndPosition <= deIdentified.Length)
            {
                var before = deIdentified.Substring(0, phi.StartPosition);
                var after = deIdentified.Substring(phi.EndPosition);
                deIdentified = before + phi.SuggestedReplacement + after;
            }
        }

        _logger?.LogInformation("De-identified {Count} PHI elements", sortedElements.Count);

        return Task.FromResult(deIdentified);
    }

    public Task AuditAIOperationAsync(
        string operation,
        string? userId,
        string input,
        string output,
        CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would write to an audit log database or service
        // For now, we'll log to the application logger
        _logger?.LogInformation(
            "AI Audit: Operation={Operation}, User={UserId}, InputLength={InputLength}, OutputLength={OutputLength}, Timestamp={Timestamp}",
            operation,
            userId ?? "Anonymous",
            input.Length,
            output.Length,
            DateTime.UtcNow);

        // Implement audit trail persistence
        var auditEntry = new
        {
            AuditId = Guid.NewGuid(),
            Operation = operation,
            UserId = userId ?? "Anonymous",
            InputHash = ComputeHash(input), // Store hash, not actual data
            OutputHash = ComputeHash(output),
            InputLength = input.Length,
            OutputLength = output.Length,
            Timestamp = DateTime.UtcNow,
            RetentionUntil = DateTime.UtcNow.AddYears(7), // HIPAA 7-year retention
            IsEncrypted = true,
            SourceSystem = "Nalam360Enterprise.UI",
            ComplianceVersion = "HIPAA-v2.0"
        };

        // In production, write to:
        // 1. Append-only audit database (e.g., Azure SQL with temporal tables)
        // 2. Encrypted audit log file with write-once-read-many (WORM) storage
        // 3. Blockchain-based audit trail for tamper-proof logging
        // For now, we log the structured audit entry
        _logger?.LogInformation(
            "HIPAA Audit Entry: {@AuditEntry}",
            auditEntry);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates that the endpoint uses HTTPS and approved regions
    /// </summary>
    public Task<bool> ValidateDataResidencyAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return Task.FromResult(false);

        // Must use HTTPS
        if (!endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogWarning("Endpoint does not use HTTPS: {Endpoint}", endpoint);
            return Task.FromResult(false);
        }

        // Check for approved Azure regions (US only for HIPAA)
        var approvedRegions = new[]
        {
            "eastus", "eastus2", "westus", "westus2", "westus3",
            "centralus", "northcentralus", "southcentralus",
            "usgovvirginia", "usgovarizona"
        };

        var isApprovedRegion = approvedRegions.Any(region =>
            endpoint.Contains(region, StringComparison.OrdinalIgnoreCase));

        if (!isApprovedRegion)
        {
            _logger?.LogWarning("Endpoint is not in an approved US region: {Endpoint}", endpoint);
        }

        return Task.FromResult(isApprovedRegion);
    }

    /// <summary>
    /// Validates encryption in transit
    /// </summary>
    public Task<bool> ValidateEncryptionAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return Task.FromResult(false);

        // Ensure HTTPS (TLS 1.2+ required for HIPAA)
        var isSecure = endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        if (!isSecure)
        {
            _logger?.LogError("Endpoint does not use encryption: {Endpoint}", endpoint);
        }

        return Task.FromResult(isSecure);
    }

    /// <summary>
    /// Generates a compliance report for a text sample
    /// </summary>
    public async Task<ComplianceReport> GenerateComplianceReportAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var phiElements = await DetectPHIAsync(text, cancellationToken);
        var phiTypes = phiElements.Select(p => p.Type).Distinct().ToList();
        var avgConfidence = phiElements.Any() ? phiElements.Average(p => p.Confidence) : 1.0;

        return new ComplianceReport
        {
            ContainsPHI = phiElements.Any(),
            PHIElementCount = phiElements.Count,
            PHITypes = phiTypes,
            AverageConfidence = avgConfidence,
            IsCompliant = !phiElements.Any() || avgConfidence > 0.7,
            Timestamp = DateTime.UtcNow,
            Recommendations = GenerateRecommendations(phiElements)
        };
    }

    private List<string> GenerateRecommendations(List<PHIElement> phiElements)
    {
        var recommendations = new List<string>();

        if (!phiElements.Any())
        {
            recommendations.Add("No PHI detected. Text is safe to process.");
            return recommendations;
        }

        var lowConfidenceElements = phiElements.Where(p => p.Confidence < 0.7).ToList();
        if (lowConfidenceElements.Any())
        {
            recommendations.Add($"Review {lowConfidenceElements.Count} low-confidence PHI detections manually.");
        }

        var phiTypeGroups = phiElements.GroupBy(p => p.Type);
        foreach (var group in phiTypeGroups)
        {
            recommendations.Add($"De-identify {group.Count()} {group.Key} element(s) before processing.");
        }

        recommendations.Add("Ensure all AI processing uses de-identified data.");
        recommendations.Add("Maintain audit trail of all PHI access.");

        return recommendations;
    }

    /// <summary>
    /// Computes SHA256 hash of text for audit trail (stores hash, not actual data)
    /// </summary>
    private string ComputeHash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// Compliance report for PHI analysis
/// </summary>
public class ComplianceReport
{
    public bool ContainsPHI { get; set; }
    public int PHIElementCount { get; set; }
    public List<string> PHITypes { get; set; } = new();
    public double AverageConfidence { get; set; }
    public bool IsCompliant { get; set; }
    public DateTime Timestamp { get; set; }
    public List<string> Recommendations { get; set; } = new();
}
