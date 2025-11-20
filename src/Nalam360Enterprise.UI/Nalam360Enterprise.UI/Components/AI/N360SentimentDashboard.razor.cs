using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalam360Enterprise.UI.Components.AI;

public partial class N360SentimentDashboard
{
    // Injected Services
    [Inject] private IAIService? AIService { get; set; }
    [Inject] private IAIComplianceService? ComplianceService { get; set; }
    [Inject] private IMLModelService? MLModelService { get; set; }

    // Parameters
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; }
    [Parameter] public bool EnableAudit { get; set; }
    [Parameter] public string? AuditResource { get; set; }
    [Parameter] public string? AuditAction { get; set; }
    [Parameter] public string SelectedTimeRange { get; set; } = "week";
    [Parameter] public EventCallback<SentimentSummary> OnSentimentChanged { get; set; }
    [Parameter] public EventCallback<FeedbackItem> OnFeedbackSelected { get; set; }
    
    // AI Features
    [Parameter] public bool UseRealAI { get; set; } = false;
    [Parameter] public string? AIModelEndpoint { get; set; }
    [Parameter] public string? AIApiKey { get; set; }
    [Parameter] public bool EnablePHIDetection { get; set; } = true;
    [Parameter] public string? UserId { get; set; }

    // State
    private bool IsLoading { get; set; }
    private SentimentSummary? SummaryData { get; set; }
    private List<TrendDataPoint> TrendData { get; set; } = new();
    private List<ChannelSentiment> ChannelData { get; set; } = new();
    private List<KeywordData> TopKeywords { get; set; } = new();
    private List<FeedbackItem> RecentFeedback { get; set; } = new();
    private List<SentimentAlert> AlertsData { get; set; } = new();

    private bool HasPermission { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(RequiredPermission) && PermissionService != null)
        {
            HasPermission = await PermissionService.HasPermissionAsync(RequiredPermission);
        }
        await LoadSentimentDataAsync();
    }

    private async Task OnTimeRangeChanged()
    {
        await RefreshData();
    }

    private async Task RefreshData()
    {
        IsLoading = true;
        StateHasChanged();

        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "SentimentDashboard",
                Action = AuditAction ?? "Refresh",
                AdditionalData = new Dictionary<string, object>
                {
                    ["TimeRange"] = SelectedTimeRange
                }
            });
        }

        await LoadSentimentDataAsync();

        IsLoading = false;
        StateHasChanged();
    }

    private async Task LoadSentimentDataAsync()
    {
        // Simulate AI sentiment analysis processing
        await Task.Delay(500);

        // Generate summary data
        var (positive, neutral, negative) = GetSentimentCounts(SelectedTimeRange);
        var total = positive + neutral + negative;

        SummaryData = new SentimentSummary
        {
            PositiveCount = positive,
            NeutralCount = neutral,
            NegativeCount = negative,
            PositivePercentage = total > 0 ? (positive * 100.0 / total) : 0,
            NeutralPercentage = total > 0 ? (neutral * 100.0 / total) : 0,
            NegativePercentage = total > 0 ? (negative * 100.0 / total) : 0,
            OverallScore = CalculateOverallScore(positive, neutral, negative)
        };

        // Generate trend data
        TrendData = GenerateTrendData(SelectedTimeRange);

        // Generate channel data
        ChannelData = GenerateChannelData();

        // Generate top keywords
        TopKeywords = GenerateTopKeywords();

        // Generate recent feedback
        RecentFeedback = GenerateRecentFeedback();

        // Generate alerts
        AlertsData = GenerateAlerts(SummaryData);

        await OnSentimentChanged.InvokeAsync(SummaryData);
    }

    private (int positive, int neutral, int negative) GetSentimentCounts(string timeRange)
    {
        // Simulate sentiment counts based on time range
        return timeRange switch
        {
            "today" => (45, 28, 12),
            "week" => (312, 189, 84),
            "month" => (1456, 823, 348),
            "quarter" => (4234, 2456, 1023),
            "year" => (16845, 9234, 4123),
            _ => (312, 189, 84)
        };
    }

    private double CalculateOverallScore(int positive, int neutral, int negative)
    {
        var total = positive + neutral + negative;
        if (total == 0) return 3.0;

        // Weight: Positive=5, Neutral=3, Negative=1
        var weightedSum = (positive * 5.0) + (neutral * 3.0) + (negative * 1.0);
        return weightedSum / total;
    }

    private List<TrendDataPoint> GenerateTrendData(string timeRange)
    {
        var data = new List<TrendDataPoint>();
        var days = timeRange switch
        {
            "today" => 24, // hourly
            "week" => 7,
            "month" => 30,
            "quarter" => 90,
            "year" => 365,
            _ => 7
        };

        var now = DateTime.Now;
        var random = new Random(42);

        for (int i = 0; i < Math.Min(days, 30); i++)
        {
            var date = timeRange == "today" ? now.AddHours(-i) : now.AddDays(-i);
            var baseScore = 3.8 + (random.NextDouble() * 0.8 - 0.4); // 3.4 to 4.2
            data.Add(new TrendDataPoint
            {
                Date = date,
                Score = Math.Round(baseScore, 2)
            });
        }

        return data.OrderBy(d => d.Date).ToList();
    }

    private List<ChannelSentiment> GenerateChannelData()
    {
        return new List<ChannelSentiment>
        {
            new() { Channel = "Surveys", PositivePercentage = 68.5, NeutralPercentage = 22.3, NegativePercentage = 9.2 },
            new() { Channel = "Chat", PositivePercentage = 52.4, NeutralPercentage = 31.8, NegativePercentage = 15.8 },
            new() { Channel = "Phone", PositivePercentage = 48.9, NeutralPercentage = 35.6, NegativePercentage = 15.5 },
            new() { Channel = "Portal", PositivePercentage = 61.2, NeutralPercentage = 28.4, NegativePercentage = 10.4 },
            new() { Channel = "Email", PositivePercentage = 57.8, NeutralPercentage = 30.1, NegativePercentage = 12.1 },
            new() { Channel = "Social Media", PositivePercentage = 44.3, NeutralPercentage = 32.7, NegativePercentage = 23.0 }
        };
    }

    private List<KeywordData> GenerateTopKeywords()
    {
        var positiveKeywords = new[]
        {
            ("excellent", 234), ("caring", 189), ("professional", 176), ("helpful", 165),
            ("clean", 142), ("friendly", 138), ("efficient", 127), ("compassionate", 118),
            ("knowledgeable", 106), ("comfortable", 98), ("thorough", 89), ("respectful", 82)
        };

        var neutralKeywords = new[]
        {
            ("appointment", 456), ("procedure", 312), ("insurance", 289), ("billing", 267),
            ("medication", 245), ("results", 223), ("schedule", 198), ("records", 187),
            ("forms", 165), ("payment", 152), ("referral", 134), ("prescription", 128)
        };

        var negativeKeywords = new[]
        {
            ("waiting", 198), ("delayed", 167), ("confused", 145), ("rude", 123),
            ("expensive", 112), ("uncomfortable", 98), ("rushed", 87), ("unprofessional", 76),
            ("error", 68), ("pain", 62), ("complicated", 58), ("frustrating", 54)
        };

        var keywords = new List<KeywordData>();
        keywords.AddRange(positiveKeywords.Select(k => new KeywordData { Word = k.Item1, Frequency = k.Item2, Sentiment = "Positive" }));
        keywords.AddRange(neutralKeywords.Select(k => new KeywordData { Word = k.Item1, Frequency = k.Item2, Sentiment = "Neutral" }));
        keywords.AddRange(negativeKeywords.Select(k => new KeywordData { Word = k.Item1, Frequency = k.Item2, Sentiment = "Negative" }));

        return keywords;
    }

    private List<FeedbackItem> GenerateRecentFeedback()
    {
        var feedback = new List<FeedbackItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddMinutes(-15),
                Channel = "Survey",
                PatientName = "Sarah Johnson",
                Sentiment = "Positive",
                Score = 4.8,
                Text = "Dr. Smith was excellent! Very thorough and took time to answer all my questions."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddMinutes(-32),
                Channel = "Chat",
                PatientName = "Michael Chen",
                Sentiment = "Negative",
                Score = 2.1,
                Text = "Had to wait 45 minutes past my appointment time. Very frustrating experience."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddMinutes(-48),
                Channel = "Portal",
                PatientName = "Emily Rodriguez",
                Sentiment = "Positive",
                Score = 4.5,
                Text = "The new patient portal is very user-friendly. Easy to schedule appointments and view results."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddHours(-1.2),
                Channel = "Phone",
                PatientName = "David Wilson",
                Sentiment = "Neutral",
                Score = 3.2,
                Text = "The appointment was fine, but the billing process was confusing."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddHours(-1.8),
                Channel = "Email",
                PatientName = "Jennifer Martinez",
                Sentiment = "Positive",
                Score = 4.7,
                Text = "Nurse practitioner was very compassionate and knowledgeable. Great experience overall."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddHours(-2.3),
                Channel = "Survey",
                PatientName = "Robert Taylor",
                Sentiment = "Negative",
                Score = 1.9,
                Text = "Front desk staff was rude and unhelpful. Not impressed with the service."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddHours(-3.1),
                Channel = "Social Media",
                PatientName = "Amanda Brown",
                Sentiment = "Positive",
                Score = 4.6,
                Text = "Love the new telehealth option! So convenient and the video quality was great."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddHours(-3.7),
                Channel = "Chat",
                PatientName = "James Anderson",
                Sentiment = "Neutral",
                Score = 3.4,
                Text = "Appointment was okay. Doctor seemed rushed but answered my main concerns."
            }
        };

        return feedback;
    }

    private List<SentimentAlert> GenerateAlerts(SentimentSummary summary)
    {
        var alerts = new List<SentimentAlert>();

        // Check for high negative sentiment
        if (summary.NegativePercentage > 20)
        {
            alerts.Add(new SentimentAlert
            {
                Id = Guid.NewGuid(),
                Severity = "High",
                Title = "Elevated Negative Sentiment Detected",
                Description = $"Negative feedback has reached {summary.NegativePercentage:F1}%, exceeding the 20% threshold. Immediate attention recommended.",
                Timestamp = DateTime.Now.AddMinutes(-10)
            });
        }

        // Check for declining trend
        if (TrendData.Count >= 7)
        {
            var recentAvg = TrendData.TakeLast(3).Average(t => t.Score);
            var previousAvg = TrendData.Skip(TrendData.Count - 7).Take(3).Average(t => t.Score);

            if (recentAvg < previousAvg - 0.3)
            {
                alerts.Add(new SentimentAlert
                {
                    Id = Guid.NewGuid(),
                    Severity = "Medium",
                    Title = "Declining Sentiment Trend",
                    Description = $"Average sentiment score has declined from {previousAvg:F2} to {recentAvg:F2} over the past week.",
                    Timestamp = DateTime.Now.AddMinutes(-25)
                });
            }
        }

        // Check for channel-specific issues
        var problematicChannel = ChannelData.FirstOrDefault(c => c.NegativePercentage > 20);
        if (problematicChannel != null)
        {
            alerts.Add(new SentimentAlert
            {
                Id = Guid.NewGuid(),
                Severity = "Medium",
                Title = $"High Negative Sentiment in {problematicChannel.Channel}",
                Description = $"{problematicChannel.Channel} channel shows {problematicChannel.NegativePercentage:F1}% negative feedback. Review recent interactions for improvement opportunities.",
                Timestamp = DateTime.Now.AddMinutes(-42)
            });
        }

        return alerts;
    }

    private async Task ViewDetails(FeedbackItem feedback)
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "SentimentDashboard",
                Action = "ViewFeedbackDetails",
                AdditionalData = new Dictionary<string, object>
                {
                    ["FeedbackId"] = feedback.Id,
                    ["PatientName"] = feedback.PatientName,
                    ["Sentiment"] = feedback.Sentiment
                }
            });
        }

        await OnFeedbackSelected.InvokeAsync(feedback);
    }

    private async Task DismissAlert(SentimentAlert alert)
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "SentimentDashboard",
                Action = "DismissAlert",
                AdditionalData = new Dictionary<string, object>
                {
                    ["AlertId"] = alert.Id,
                    ["AlertTitle"] = alert.Title,
                    ["Severity"] = alert.Severity
                }
            });
        }

        AlertsData.Remove(alert);
        StateHasChanged();
    }

    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>();

        // Add accessibility attributes
        attributes["role"] = "region";
        attributes["aria-label"] = "Patient Sentiment Dashboard";

        return attributes;
    }

    // Data classes
    public class SentimentSummary
    {
        public int PositiveCount { get; set; }
        public int NeutralCount { get; set; }
        public int NegativeCount { get; set; }
        public double PositivePercentage { get; set; }
        public double NeutralPercentage { get; set; }
        public double NegativePercentage { get; set; }
        public double OverallScore { get; set; }
    }

    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public double Score { get; set; }
    }

    public class ChannelSentiment
    {
        public string Channel { get; set; } = string.Empty;
        public double PositivePercentage { get; set; }
        public double NeutralPercentage { get; set; }
        public double NegativePercentage { get; set; }
    }

    public class KeywordData
    {
        public string Word { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string Sentiment { get; set; } = string.Empty;
    }

    public class FeedbackItem
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class SentimentAlert
    {
        public Guid Id { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
