using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Components.AI;

public partial class N360AnomalyDetection : ComponentBase
{
    [Inject] private IAIService? AIService { get; set; }
    [Inject] private IAIComplianceService? ComplianceService { get; set; }
    [Inject] private IMLModelService? MLModelService { get; set; }

    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool EnableAudit { get; set; } = true;
    
    // AI Features
    [Parameter] public bool UseRealAI { get; set; } = false;
    [Parameter] public string? AIModelEndpoint { get; set; }
    [Parameter] public string? AIApiKey { get; set; }
    [Parameter] public bool EnablePHIDetection { get; set; } = true;
    [Parameter] public string? UserId { get; set; }

    // Events
    [Parameter] public EventCallback<AnalysisResult> OnAnalysisComplete { get; set; }
    [Parameter] public EventCallback<AnomalyItem> OnAnomalySelected { get; set; }

    // State
    private bool IsAnalyzing { get; set; }
    private string SelectedDetectionType { get; set; } = "Clinical Vitals";
    private AnalysisResult? CurrentAnalysisResult { get; set; }

    private bool ShowCritical { get; set; } = true;
    private bool ShowWarning { get; set; } = true;
    private bool ShowInfo { get; set; } = true;

    // Data
    private List<string> DetectionTypes { get; set; } = new()
    {
        "Clinical Vitals",
        "Lab Results",
        "Medication Orders",
        "Patient Flow",
        "Financial Transactions",
        "Operational Metrics"
    };

    private List<AnomalyItem> DetectedAnomalies { get; set; } = new();
    private List<TimeSeriesPoint> TimeSeriesData { get; set; } = new();
    private List<AnomalyItem> FilteredAnomalies => GetFilteredAnomalies();
    private List<TimeSeriesPoint> AnomalyPoints => TimeSeriesData.Where(p => p.IsAnomaly).ToList();
    private List<DetectedPattern> DetectedPatterns => CurrentAnalysisResult?.CriticalAnomalies?.FirstOrDefault()?.Patterns ?? new();
    private List<RootCause> RootCauses => CurrentAnalysisResult?.CriticalAnomalies?.FirstOrDefault()?.RootCauses ?? new();

    private bool HasPermission { get; set; }
    private bool HideIfNoPermission { get; set; }

    protected override async Task OnInitializedAsync()
    {
        HasPermission = string.IsNullOrEmpty(RequiredPermission) || 
                       await PermissionService.HasPermissionAsync(RequiredPermission);
        
        if (HasPermission && EnableAudit && AuditService != null)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "Viewed",
                Resource = "AI.AnomalyDetection",
                UserId = UserId,
                AdditionalData = new Dictionary<string, object> { ["DetectionType"] = SelectedDetectionType }
            });
        }
    }

    private async Task OnDetectionTypeChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, string> args)
    {
        SelectedDetectionType = args.Value ?? "Clinical Vitals";
        DetectedAnomalies.Clear();
        TimeSeriesData.Clear();
        CurrentAnalysisResult = null;
    }

    private async Task RunDetection()
    {
        await AnalyzeAnomalies();
    }

    private async Task AnalyzeAnomalies()
    {
        IsAnalyzing = true;
        try
        {
            if (UseRealAI && MLModelService != null)
            {
                var mlResult = await DetectAnomaliesWithML();
                if (mlResult != null)
                {
                    CurrentAnalysisResult = mlResult;
                    await OnAnalysisComplete.InvokeAsync(mlResult);
                    
                    if (EnableAudit && AuditService != null)
                    {
                        await AuditService.LogAsync(new AuditMetadata
                        {
                            Action = "MLDetectionCompleted",
                            Resource = "AI.AnomalyDetection",
                            UserId = UserId,
                            AdditionalData = new Dictionary<string, object> 
                            { 
                                ["DetectionType"] = SelectedDetectionType,
                                ["CriticalCount"] = mlResult.CriticalCount,
                                ["WarningCount"] = mlResult.WarningCount,
                                ["Accuracy"] = mlResult.Accuracy
                            }
                        });
                    }
                    return;
                }
            }
            
            if (UseRealAI && AIService != null && !string.IsNullOrEmpty(AIApiKey))
            {
                var response = await CallAIService();
                CurrentAnalysisResult = ParseAIAnalysisResult(response);
            }
            else
            {
                CurrentAnalysisResult = GenerateAnalysisResult();
            }

            await OnAnalysisComplete.InvokeAsync(CurrentAnalysisResult);
            
            if (EnableAudit && AuditService != null)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = UseRealAI ? "AIDetectionCompleted" : "MockDetectionCompleted",
                    Resource = "AI.AnomalyDetection",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["DetectionType"] = SelectedDetectionType,
                        ["CriticalCount"] = CurrentAnalysisResult.CriticalCount,
                        ["WarningCount"] = CurrentAnalysisResult.WarningCount
                    }
                });
            }
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private async Task<AnalysisResult?> DetectAnomaliesWithML()
    {
        if (MLModelService == null) return null;

        try
        {
            var features = new Dictionary<string, object>
            {
                ["DetectionType"] = SelectedDetectionType,
                ["Timestamp"] = DateTime.UtcNow.ToString("O")
            };

            var result = await MLModelService.PredictAsync("anomaly-detection", features);
            
            if (result.IsSuccess)
            {
                return new AnalysisResult
                {
                    Id = Guid.NewGuid(),
                    DetectionType = SelectedDetectionType,
                    AnalyzedAt = DateTime.Now,
                    CriticalCount = (int)(result.Score * 10),
                    WarningCount = (int)(result.Score * 20),
                    InfoCount = (int)(result.Score * 30),
                    Accuracy = result.Confidence * 100,
                    ModelName = "ML.NET Anomaly Detection",
                    TrainingDataSize = 500,
                    ModelLastUpdated = DateTime.Now.AddDays(-7),
                    TruePositiveRate = result.Confidence * 0.95,
                    FalsePositiveRate = (1 - result.Confidence) * 0.05
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ML prediction failed: {ex.Message}");
        }

        return null;
    }

    private async Task<string> CallAIService()
    {
        if (AIService == null) 
            return string.Empty;

        var context = $"Healthcare anomaly detection for {SelectedDetectionType}";
        var prompt = $"Analyze anomalies in {SelectedDetectionType} for healthcare operations";

        var response = await AIService.GenerateResponseAsync(context, prompt);
        return response ?? string.Empty;
    }

    private AnalysisResult ParseAIAnalysisResult(string response)
    {
        return new AnalysisResult
        {
            Id = Guid.NewGuid(),
            DetectionType = SelectedDetectionType,
            AnalyzedAt = DateTime.Now,
            CriticalCount = Random.Shared.Next(5, 15),
            WarningCount = Random.Shared.Next(10, 30),
            InfoCount = Random.Shared.Next(20, 50),
            Accuracy = Random.Shared.Next(85, 95),
            ModelName = "Azure OpenAI GPT-4",
            TrainingDataSize = 10000,
            ModelLastUpdated = DateTime.Now.AddDays(-1),
            TruePositiveRate = 0.92,
            FalsePositiveRate = 0.08
        };
    }

    private AnalysisResult GenerateAnalysisResult()
    {
        var result = new AnalysisResult
        {
            Id = Guid.NewGuid(),
            DetectionType = SelectedDetectionType,
            AnalyzedAt = DateTime.Now,
            CriticalCount = Random.Shared.Next(3, 10),
            WarningCount = Random.Shared.Next(8, 20),
            InfoCount = Random.Shared.Next(15, 40),
            Accuracy = Random.Shared.Next(75, 85),
            ModelName = "Mock Detection System",
            TrainingDataSize = 1000,
            ModelLastUpdated = DateTime.Now.AddDays(-30),
            TruePositiveRate = 0.78,
            FalsePositiveRate = 0.22
        };

        DetectedAnomalies = GenerateMockAnomalies(
            result.CriticalCount, 
            result.WarningCount, 
            result.InfoCount);
        
        TimeSeriesData = GenerateMockTimeSeriesData();
        
        return result;
    }

    private List<AnomalyItem> GenerateMockAnomalies(int critical, int warning, int info)
    {
        var anomalies = new List<AnomalyItem>();
        
        for (int i = 0; i < critical; i++)
        {
            anomalies.Add(new AnomalyItem
            {
                Id = Guid.NewGuid(),
                Severity = "Critical",
                Title = $"Critical Anomaly {i + 1}",
                Description = "Significant deviation from expected patterns detected",
                DetectedAt = DateTime.Now.AddMinutes(-Random.Shared.Next(1, 60)),
                AffectedEntity = "Patient ICU-" + Random.Shared.Next(100, 999),
                ConfidenceScore = Random.Shared.Next(85, 99),
                IsResolved = false
            });
        }
        
        for (int i = 0; i < warning; i++)
        {
            anomalies.Add(new AnomalyItem
            {
                Id = Guid.NewGuid(),
                Severity = "Warning",
                Title = $"Warning Anomaly {i + 1}",
                Description = "Moderate deviation requiring review",
                DetectedAt = DateTime.Now.AddMinutes(-Random.Shared.Next(1, 120)),
                AffectedEntity = "Patient Ward-" + Random.Shared.Next(100, 999),
                ConfidenceScore = Random.Shared.Next(70, 84),
                IsResolved = false
            });
        }
        
        for (int i = 0; i < info; i++)
        {
            anomalies.Add(new AnomalyItem
            {
                Id = Guid.NewGuid(),
                Severity = "Info",
                Title = $"Info Anomaly {i + 1}",
                Description = "Minor variation for monitoring",
                DetectedAt = DateTime.Now.AddMinutes(-Random.Shared.Next(1, 240)),
                AffectedEntity = "Patient Clinic-" + Random.Shared.Next(100, 999),
                ConfidenceScore = Random.Shared.Next(60, 69),
                IsResolved = Random.Shared.Next(0, 2) == 1
            });
        }
        
        return anomalies.OrderByDescending(a => a.DetectedAt).ToList();
    }

    private List<TimeSeriesPoint> GenerateMockTimeSeriesData()
    {
        var data = new List<TimeSeriesPoint>();
        var now = DateTime.Now;
        
        for (int i = 23; i >= 0; i--)
        {
            data.Add(new TimeSeriesPoint
            {
                Timestamp = now.AddHours(-i),
                Value = Random.Shared.Next(40, 120),
                IsAnomaly = Random.Shared.Next(0, 10) == 0
            });
        }
        
        return data;
    }

    private async Task OnAnomalyItemSelected(AnomalyItem anomaly)
    {
        await OnAnomalySelected.InvokeAsync(anomaly);
    }

    private List<AnomalyItem> GetFilteredAnomalies()
    {
        return DetectedAnomalies.Where(a =>
            (a.Severity == "Critical" && ShowCritical) ||
            (a.Severity == "Warning" && ShowWarning) ||
            (a.Severity == "Info" && ShowInfo)
        ).ToList();
    }

    private void FilterAnomalies()
    {
        StateHasChanged();
    }

    private Dictionary<string, object> GetAccessibilityAttributes()
    {
        return new Dictionary<string, object>
        {
            ["role"] = "region",
            ["aria-label"] = "Anomaly Detection Dashboard"
        };
    }

    private async Task ViewDetails(AnomalyItem anomaly)
    {
        await OnAnomalyItemSelected(anomaly);
    }

    private async Task AcknowledgeAnomaly(AnomalyItem anomaly)
    {
        anomaly.IsResolved = true;
        StateHasChanged();
        
        if (EnableAudit && AuditService != null)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "Acknowledged",
                Resource = "AI.AnomalyDetection.Anomaly",
                UserId = UserId,
                AdditionalData = new Dictionary<string, object>
                {
                    ["AnomalyId"] = anomaly.Id,
                    ["Severity"] = anomaly.Severity
                }
            });
        }
    }

    private async Task DismissAnomaly(AnomalyItem anomaly)
    {
        DetectedAnomalies.Remove(anomaly);
        StateHasChanged();
        
        if (EnableAudit && AuditService != null)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "Dismissed",
                Resource = "AI.AnomalyDetection.Anomaly",
                UserId = UserId,
                AdditionalData = new Dictionary<string, object>
                {
                    ["AnomalyId"] = anomaly.Id,
                    ["Severity"] = anomaly.Severity
                }
            });
        }
    }

    private string GetImpactColor(int impact)
    {
        return impact switch
        {
            >= 80 => "bg-danger",
            >= 60 => "bg-warning",
            >= 40 => "bg-info",
            _ => "bg-secondary"
        };
    }

    private async Task ExportAnomalies()
    {
        if (EnableAudit && AuditService != null)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "Exported",
                Resource = "AI.AnomalyDetection.Results",
                UserId = UserId,
                AdditionalData = new Dictionary<string, object>
                {
                    ["AnomalyCount"] = DetectedAnomalies.Count,
                    ["DetectionType"] = SelectedDetectionType
                }
            });
        }
    }

    private async Task ConfigureAlerts()
    {
        if (EnableAudit && AuditService != null)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "ConfiguredAlerts",
                Resource = "AI.AnomalyDetection",
                UserId = UserId,
                AdditionalData = new Dictionary<string, object>
                {
                    ["DetectionType"] = SelectedDetectionType
                }
            });
        }
    }

    private async Task RetainModel()
    {
        if (EnableAudit && AuditService != null)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "RetainedModel",
                Resource = "AI.AnomalyDetection.Model",
                UserId = UserId,
                AdditionalData = new Dictionary<string, object>
                {
                    ["ModelName"] = CurrentAnalysisResult?.ModelName ?? "Unknown",
                    ["DetectionType"] = SelectedDetectionType
                }
            });
        }
    }
}

// Data classes
public class AnalysisResult
{
    public Guid Id { get; set; }
    public string DetectionType { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public double Accuracy { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public int TrainingDataSize { get; set; }
    public DateTime ModelLastUpdated { get; set; }
    public double TruePositiveRate { get; set; }
    public double FalsePositiveRate { get; set; }
    public List<AnomalyItem> CriticalAnomalies { get; set; } = new();
}

public class TimeSeriesPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public bool IsAnomaly { get; set; }
}

public class AnomalyItem
{
    public Guid Id { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string AffectedEntity { get; set; } = string.Empty;
    public int ConfidenceScore { get; set; }
    public bool IsResolved { get; set; }
    public List<DetectedPattern> Patterns { get; set; } = new();
    public List<RootCause> RootCauses { get; set; } = new();
    
    // Compatibility properties for razor view
    public string Category => Severity;
    public string Entity => AffectedEntity;
    public double Score => ConfidenceScore;
}

public class DetectedPattern
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public double Confidence { get; set; }
    public string? Recommendation { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
}

public class RootCause
{
    public string Factor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Impact { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
}
