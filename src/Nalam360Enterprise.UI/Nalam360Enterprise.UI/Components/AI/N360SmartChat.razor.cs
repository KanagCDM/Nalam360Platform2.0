using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;
using System.Collections.Generic;

namespace Nalam360Enterprise.UI.Components.AI;

public partial class N360SmartChat
{
    private string _currentMessage = "";
    private bool _isProcessing = false;

    #region Injected Services
    
    [Inject] private IAIService? AIService { get; set; }
    [Inject] private IAIComplianceService? ComplianceService { get; set; }
    
    #endregion

    #region Parameters

    [Parameter] public string? Title { get; set; }
    [Parameter] public SmartChatUser? CurrentUser { get; set; }
    [Parameter] public List<SmartChatMessage>? Messages { get; set; } = new();
    [Parameter] public List<SmartChatUser>? TypingUsers { get; set; } = new();
    [Parameter] public string Placeholder { get; set; } = "Type your message...";
    
    // AI Features
    [Parameter] public bool EnableAIResponse { get; set; } = true;
    [Parameter] public bool EnableIntentRecognition { get; set; } = true;
    [Parameter] public bool EnableSentimentAnalysis { get; set; } = true;
    [Parameter] public bool ShowSmartSuggestions { get; set; } = true;
    [Parameter] public bool ShowSentimentIndicator { get; set; }
    [Parameter] public bool ShowAIIndicator { get; set; } = true;
    [Parameter] public string? AIModelEndpoint { get; set; }
    [Parameter] public string? AIApiKey { get; set; }
    [Parameter] public bool UseRealAI { get; set; } = false; // Toggle between real AI and mock
    [Parameter] public bool EnablePHIDetection { get; set; } = true;
    [Parameter] public bool EnableStreaming { get; set; } = false;

    // RBAC & Audit (following N360 pattern)
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; }
    [Parameter] public bool EnableAudit { get; set; }
    [Parameter] public string AuditResource { get; set; } = "SmartChat";
    [Parameter] public string AuditAction { get; set; } = "MessageSent";
    [Parameter] public string? UserId { get; set; }

    // Styling
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public string Style { get; set; } = string.Empty;
    [Parameter] public bool IsRtl { get; set; }

    // Callbacks
    [Parameter] public EventCallback<SmartChatMessage> OnMessageSent { get; set; }
    [Parameter] public EventCallback<AIAnalysisResult> OnAIAnalysisComplete { get; set; }
    [Parameter] public EventCallback<string> OnIntentDetected { get; set; }

    #endregion

    #region State

    private List<SmartChatMessage> InternalMessages { get; set; } = new();
    private List<string> SmartSuggestions { get; set; } = new();
    private string? CurrentSentiment { get; set; }
    private string? DetectedIntent { get; set; }
    private List<SmartChatMessage> ConversationHistory { get; set; } = new();
    private double? IntentConfidence { get; set; }
    private double? SentimentConfidence { get; set; }
    private string? MessageSentiment { get; set; }
    private double? SentimentScore { get; set; }

    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Permission check (N360 pattern)
        if (!string.IsNullOrEmpty(RequiredPermission))
        {
            var hasPermission = await PermissionService.HasPermissionAsync(RequiredPermission);
            if (!hasPermission && HideIfNoPermission)
            {
                return;
            }
        }

        if (Messages != null)
        {
            InternalMessages = new List<SmartChatMessage>(Messages);
        }
        
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "SmartChatInitialized",
                Resource = AuditResource,
                AdditionalData = new Dictionary<string, object>
                {
                    ["EnableAI"] = EnableAIResponse,
                    ["EnableSentiment"] = EnableSentimentAnalysis
                }
            });
        }
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentMessage)) return;

        var message = new SmartChatMessage
        {
            Text = _currentMessage,
            Author = CurrentUser,
            TimeStamp = DateTime.UtcNow
        };
        
        _currentMessage = "";
        ConversationHistory.Add(message);
        InternalMessages.Add(message);
        Messages?.Add(message);

        // AI Processing
        if (EnableAIResponse)
        {
            await ProcessMessageWithAI(message);
        }

        await OnMessageSent.InvokeAsync(message);

        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Action = "MessageSent",
                Resource = AuditResource,
                AdditionalData = new Dictionary<string, object>
                {
                    ["Sender"] = CurrentUser?.Name ?? "Unknown",
                    ["Intent"] = DetectedIntent,
                    ["MessageLength"] = message.Text?.Length ?? 0
                }
            });
        }

        StateHasChanged();
    }

    private async Task ProcessMessageWithAI(SmartChatMessage message)
    {
        if (_isProcessing) return;
        _isProcessing = true;

        try
        {
            var analysisResult = new AIAnalysisResult();
            var messageText = message.Text ?? "";

            // Check if AI services are available and configured
            var useRealAI = UseRealAI && 
                           AIService != null && 
                           !string.IsNullOrWhiteSpace(AIModelEndpoint) && 
                           !string.IsNullOrWhiteSpace(AIApiKey);

            // PHI Detection and De-identification (if enabled)
            var processedText = messageText;
            if (EnablePHIDetection && ComplianceService != null && useRealAI)
            {
                var phiElements = await ComplianceService.DetectPHIAsync(messageText);
                if (phiElements.Any())
                {
                    processedText = await ComplianceService.DeIdentifyAsync(messageText, phiElements);
                    analysisResult.PHIDetected = phiElements.Count > 0;
                }
            }

            if (useRealAI)
            {
                // Use Real AI Services
                await ProcessWithRealAI(processedText, analysisResult);
            }
            else
            {
                // Fallback to Mock AI (existing logic)
                await ProcessWithMockAI(messageText, analysisResult);
            }

            // Audit the AI operation
            if (EnableAudit && ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
            {
                await ComplianceService.AuditAIOperationAsync(
                    "ChatInteraction",
                    UserId,
                    processedText,
                    analysisResult.AIResponse ?? "No response");
            }

            await OnAIAnalysisComplete.InvokeAsync(analysisResult);
        }
        catch (Exception ex)
        {
            // Log error and fallback to mock AI
            Console.WriteLine($"AI Processing Error: {ex.Message}");
            
            var fallbackResult = new AIAnalysisResult();
            await ProcessWithMockAI(message.Text ?? "", fallbackResult);
            await OnAIAnalysisComplete.InvokeAsync(fallbackResult);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ProcessWithRealAI(string messageText, AIAnalysisResult analysisResult)
    {
        if (AIService == null || string.IsNullOrWhiteSpace(AIModelEndpoint) || string.IsNullOrWhiteSpace(AIApiKey))
            return;

        var tasks = new List<Task>();

        // Intent Recognition
        if (EnableIntentRecognition)
        {
            var intentTask = Task.Run(async () =>
            {
                var intentResult = await AIService.AnalyzeIntentAsync(messageText);
                DetectedIntent = intentResult.Intent;
                IntentConfidence = intentResult.Confidence;
                analysisResult.Intent = DetectedIntent;
                analysisResult.IntentConfidence = IntentConfidence;
                await OnIntentDetected.InvokeAsync(DetectedIntent);
            });
            tasks.Add(intentTask);
        }

        // Sentiment Analysis
        if (EnableSentimentAnalysis)
        {
            var sentimentTask = Task.Run(async () =>
            {
                var sentimentResult = await AIService.AnalyzeSentimentAsync(messageText);
                MessageSentiment = sentimentResult.Sentiment;
                SentimentScore = sentimentResult.Confidence;
                CurrentSentiment = sentimentResult.Sentiment;
                SentimentConfidence = sentimentResult.Confidence;
                analysisResult.Sentiment = CurrentSentiment;
                analysisResult.SentimentConfidence = SentimentConfidence;
                analysisResult.SentimentScores = sentimentResult.SentimentScores;
            });
            tasks.Add(sentimentTask);
        }

        // Wait for intent and sentiment analysis
        await Task.WhenAll(tasks);

        // Generate Smart Suggestions based on detected intent
        if (ShowSmartSuggestions && !string.IsNullOrWhiteSpace(DetectedIntent))
        {
            var suggestions = await AIService.GenerateSuggestionsAsync(
                GetConversationContext(),
                DetectedIntent);
            
            if (suggestions.Any())
            {
                SmartSuggestions = suggestions;
                analysisResult.Suggestions = SmartSuggestions;
            }
        }

        // Generate AI Response
        if (EnableAIResponse)
        {
            if (EnableStreaming)
            {
                await GenerateStreamingResponse(analysisResult);
            }
            else
            {
                var response = await AIService.GenerateResponseAsync(
                    GetConversationContext(),
                    messageText);

                if (!string.IsNullOrEmpty(response))
                {
                    AddAIMessage(response);
                    analysisResult.AIResponse = response;
                }
            }
        }
    }

    private async Task GenerateStreamingResponse(AIAnalysisResult analysisResult)
    {
        if (AIService == null)
            return;

        var aiMessage = new SmartChatMessage
        {
            Text = "",
            Author = new SmartChatUser { Id = "ai", Name = "AI Assistant", AvatarUrl = "🤖" },
            TimeStamp = DateTime.UtcNow
        };
        
        InternalMessages.Add(aiMessage);
        Messages?.Add(aiMessage);
        StateHasChanged();

        try
        {
            await foreach (var token in AIService.StreamResponseAsync(
                GetConversationContext(),
                _currentMessage))
            {
                aiMessage.Text += token;
                StateHasChanged();
                await Task.Delay(10); // Smooth streaming effect
            }

            ConversationHistory.Add(aiMessage);
            analysisResult.AIResponse = aiMessage.Text;
        }
        catch (Exception ex)
        {
            aiMessage.Text = "I apologize, but I encountered an error while generating a response.";
            Console.WriteLine($"Streaming Error: {ex.Message}");
        }
    }

    private async Task ProcessWithMockAI(string messageText, AIAnalysisResult analysisResult)
    {
        // Intent Recognition (Mock)
        if (EnableIntentRecognition)
        {
            DetectedIntent = RecognizeIntentMock(messageText);
            IntentConfidence = 0.7; // Simulated confidence
            analysisResult.Intent = DetectedIntent;
            analysisResult.IntentConfidence = IntentConfidence;
            await OnIntentDetected.InvokeAsync(DetectedIntent);
        }

        // Sentiment Analysis (Mock)
        if (EnableSentimentAnalysis)
        {
            CurrentSentiment = AnalyzeSentimentMock(messageText);
            SentimentConfidence = 0.65; // Simulated confidence
            analysisResult.Sentiment = CurrentSentiment;
            analysisResult.SentimentConfidence = SentimentConfidence;
        }

        // Generate Suggestions (Mock)
        if (ShowSmartSuggestions)
        {
            SmartSuggestions = GenerateSmartSuggestionsMock(DetectedIntent);
            analysisResult.Suggestions = SmartSuggestions;
        }

        // Generate AI Response (Mock)
        if (EnableAIResponse)
        {
            var aiResponse = GenerateAIResponseMock(DetectedIntent);
            if (!string.IsNullOrEmpty(aiResponse))
            {
                AddAIMessage(aiResponse);
                analysisResult.AIResponse = aiResponse;
            }
        }
    }

    private void AddAIMessage(string text)
    {
        var aiMessage = new SmartChatMessage
        {
            Text = text,
            Author = new SmartChatUser { Id = "ai", Name = "AI Assistant", AvatarUrl = "🤖" },
            TimeStamp = DateTime.UtcNow
        };
        InternalMessages.Add(aiMessage);
        Messages?.Add(aiMessage);
        ConversationHistory.Add(aiMessage);
    }

    private string GetConversationContext()
    {
        if (!ConversationHistory.Any())
            return "";

        var recentMessages = ConversationHistory.TakeLast(5);
        return string.Join("\n", recentMessages.Select(m => $"{m.Author?.Name}: {m.Text}"));
    }

    #region Mock AI Methods (Fallback)

    private string RecognizeIntentMock(string message)
    {
        var lower = message.ToLower();
        if (lower.Contains("appointment") || lower.Contains("schedule"))
            return "AppointmentScheduling";
        if (lower.Contains("prescription") || lower.Contains("medication"))
            return "PrescriptionInquiry";
        if (lower.Contains("symptom") || lower.Contains("pain"))
            return "SymptomCheck";
        if (lower.Contains("lab") || lower.Contains("test"))
            return "LabResults";
        if (lower.Contains("billing") || lower.Contains("insurance"))
            return "BillingInquiry";
        if (lower.Contains("emergency") || lower.Contains("urgent"))
            return "EmergencyTriage";
        return "GeneralInquiry";
    }

    private string AnalyzeSentimentMock(string message)
    {
        var lower = message.ToLower();
        var negative = new[] { "bad", "pain", "worse", "terrible", "emergency", "urgent" };
        var positive = new[] { "good", "better", "great", "thank", "appreciate" };
        
        var negCount = negative.Count(w => lower.Contains(w));
        var posCount = positive.Count(w => lower.Contains(w));
        
        if (negCount > posCount) return "Negative";
        if (posCount > negCount) return "Positive";
        return "Neutral";
    }

    private List<string> GenerateSmartSuggestionsMock(string? intent)
    {
        return intent switch
        {
            "AppointmentScheduling" => new() { "View available times", "Reschedule appointment", "Find clinic" },
            "PrescriptionInquiry" => new() { "Refill prescription", "Check interactions", "View dosage" },
            "SymptomCheck" => new() { "Describe symptoms", "Schedule urgent care", "Self-care advice" },
            "LabResults" => new() { "View latest results", "Schedule follow-up", "Request interpretation" },
            "EmergencyTriage" => new() { "Call 911", "Find nearest ER", "Connect with physician" },
            _ => new() { "Talk to representative", "Browse FAQs", "Schedule callback" }
        };
    }

    private string GenerateAIResponseMock(string? intent)
    {
        return intent switch
        {
            "AppointmentScheduling" => "I can help schedule an appointment. What type do you need?",
            "PrescriptionInquiry" => "I can assist with prescriptions. Refill or new inquiry?",
            "SymptomCheck" => "I'm here to help. For emergencies call 911. Describe your symptoms?",
            "LabResults" => "I can help access lab results. Would you like me to check?",
            "BillingInquiry" => "I can help with billing. Specific bill, insurance, or payment?",
            "EmergencyTriage" => "⚠️ For life-threatening emergencies, call 911 immediately!",
            _ => "How can I assist you today? I help with appointments, prescriptions, labs, and billing."
        };
    }

    #endregion

    private async Task ApplySuggestion(string suggestion)
    {
        _currentMessage = suggestion;
        await SendMessageAsync();
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SendMessageAsync();
        }
    }

    private string GetSentimentIcon() => CurrentSentiment switch
    {
        "Positive" => "😊",
        "Negative" => "😟",
        _ => "😐"
    };

    public async Task ClearConversationAsync()
    {
        Messages?.Clear();
        InternalMessages.Clear();
        ConversationHistory.Clear();
        SmartSuggestions.Clear();
        CurrentSentiment = null;
        DetectedIntent = null;
        StateHasChanged();
    }

    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>();
        if (IsRtl)
        {
            attributes["dir"] = "rtl";
        }
        return attributes;
    }
}

public class AIAnalysisResult
{
    public string? Intent { get; set; }
    public double? IntentConfidence { get; set; }
    public string? Sentiment { get; set; }
    public double? SentimentConfidence { get; set; }
    public Dictionary<string, double>? SentimentScores { get; set; }
    public List<string>? Suggestions { get; set; }
    public string? AIResponse { get; set; }
    public bool PHIDetected { get; set; }
    public List<string>? DetectedPHITypes { get; set; }
}

// Internal models specific to N360SmartChat
public class SmartChatMessage
{
    public string? Text { get; set; }
    public SmartChatUser? Author { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}

public class SmartChatUser
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
}
