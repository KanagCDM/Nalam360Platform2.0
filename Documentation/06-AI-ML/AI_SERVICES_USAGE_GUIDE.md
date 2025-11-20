# AI Services Usage Guide

## Quick Start

### 1. Configure Azure OpenAI

Add to your `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4"
  }
}
```

### 2. Register Services

In your `Program.cs`:

```csharp
using Nalam360Enterprise.UI.Core.AI;

var builder = WebApplication.CreateBuilder(args);

// Register AI services
builder.Services.AddNalam360AIServices(
    builder.Configuration["AzureOpenAI:Endpoint"]!,
    builder.Configuration["AzureOpenAI:ApiKey"]!,
    builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4"
);

var app = builder.Build();
app.Run();
```

### 3. Use in Components

```csharp
@page "/chat"
@inject IAIService AIService
@inject IAIComplianceService ComplianceService

<N360SmartChat
    UseRealAI="true"
    EnablePHIDetection="true"
    EnableStreaming="true"
    UserId="@CurrentUserId"
    AIModelEndpoint="@Configuration["AzureOpenAI:Endpoint"]"
    AIApiKey="@Configuration["AzureOpenAI:ApiKey"]"
    OnIntentDetected="@OnIntentDetected"
    OnSentimentChanged="@OnSentimentChanged"
/>

@code {
    private string CurrentUserId => "user-123";

    private void OnIntentDetected(string intent)
    {
        Console.WriteLine($"Detected intent: {intent}");
    }

    private void OnSentimentChanged(string sentiment)
    {
        Console.WriteLine($"Sentiment changed: {sentiment}");
    }
}
```

## Service APIs

### IAIService

#### Analyze Intent

```csharp
var result = await AIService.AnalyzeIntentAsync(
    "I need to schedule an appointment for next Tuesday",
    cancellationToken
);

Console.WriteLine($"Intent: {result.Intent}"); // "AppointmentScheduling"
Console.WriteLine($"Confidence: {result.Confidence:P}"); // "95.3%"
```

**Supported Intents:**
- `AppointmentScheduling` - Scheduling, rescheduling appointments
- `PrescriptionInquiry` - Medication questions, refills
- `SymptomCheck` - Health concerns, symptoms
- `LabResults` - Test result inquiries
- `BillingInquiry` - Bills, insurance, payments
- `EmergencyTriage` - Urgent situations
- `GeneralInquiry` - Other topics

#### Analyze Sentiment

```csharp
var result = await AIService.AnalyzeSentimentAsync(
    "I'm feeling much better after taking the medication",
    cancellationToken
);

Console.WriteLine($"Sentiment: {result.Sentiment}"); // "Positive"
Console.WriteLine($"Confidence: {result.Confidence:P}"); // "92.1%"
Console.WriteLine($"Positive Score: {result.SentimentScores["positive"]:P}");
Console.WriteLine($"Negative Score: {result.SentimentScores["negative"]:P}");
```

#### Generate Response

```csharp
var context = "User: I have a headache\nAI: I understand. When did it start?";
var message = "It started this morning";

var response = await AIService.GenerateResponseAsync(
    context,
    message,
    cancellationToken
);

Console.WriteLine(response); 
// "I see. Is the pain constant or does it come and go? 
//  Have you taken any medication for it yet?"
```

#### Stream Response (Real-Time)

```csharp
var context = "User: Tell me about diabetes management";
var message = "What should I eat?";

await foreach (var token in AIService.StreamResponseAsync(context, message))
{
    Console.Write(token); // Prints tokens as they arrive
    await InvokeAsync(StateHasChanged); // Update UI
    await Task.Delay(10); // Smooth visual effect
}
```

#### Generate Suggestions

```csharp
var context = "User: I need to see a doctor\nAI: I can help with that.";
var intent = "AppointmentScheduling";

var suggestions = await AIService.GenerateSuggestionsAsync(
    context,
    intent,
    cancellationToken
);

foreach (var suggestion in suggestions)
{
    Console.WriteLine($"- {suggestion}");
}
// - View available appointment times
// - Find a specialist near you
// - Schedule a video consultation
```

### IAIComplianceService

#### Detect PHI

```csharp
var text = "Patient John Doe (MRN: MED123456) called from 555-123-4567";

var phiElements = await ComplianceService.DetectPHIAsync(text, cancellationToken);

foreach (var phi in phiElements)
{
    Console.WriteLine($"Type: {phi.Type}");
    Console.WriteLine($"Value: {phi.Value}");
    Console.WriteLine($"Confidence: {phi.Confidence:P}");
    Console.WriteLine($"Position: {phi.StartPosition}-{phi.EndPosition}");
    Console.WriteLine($"Replacement: {phi.SuggestedReplacement}");
    Console.WriteLine();
}

// Output:
// Type: NAME
// Value: John Doe
// Confidence: 60.0%
// Position: 8-16
// Replacement: [PATIENT]
//
// Type: MRN
// Value: MED123456
// Confidence: 95.0%
// Position: 23-32
// Replacement: [MRN]
//
// Type: PHONE
// Value: 555-123-4567
// Confidence: 85.0%
// Position: 47-59
// Replacement: [PHONE]
```

**PHI Types Detected:**
- `MRN` - Medical Record Numbers (95% confidence)
- `SSN` - Social Security Numbers (98% confidence)
- `PHONE` - Phone Numbers (85% confidence)
- `DATE` - Dates (80% confidence)
- `EMAIL` - Email Addresses (90% confidence)
- `ADDRESS` - Street Addresses (75% confidence)
- `NAME` - Patient Names (60% confidence)

#### De-Identify Data

```csharp
var text = "Patient John Doe (MRN: MED123456) called from 555-123-4567";
var phiElements = await ComplianceService.DetectPHIAsync(text);

var deIdentified = await ComplianceService.DeIdentifyAsync(
    text,
    phiElements,
    cancellationToken
);

Console.WriteLine(deIdentified);
// "Patient [PATIENT] (MRN: [MRN]) called from [PHONE]"
```

#### Audit AI Operations

```csharp
await ComplianceService.AuditAIOperationAsync(
    operation: "ChatInteraction",
    userId: "user-123",
    input: deIdentifiedInput,
    output: aiResponse,
    cancellationToken
);

// Logs to audit trail:
// AI Audit: Operation=ChatInteraction, User=user-123, 
//           InputLength=150, OutputLength=200, 
//           Timestamp=2025-11-19T10:30:00Z
```

## Complete Example: HIPAA-Compliant Chat

```csharp
@page "/secure-chat"
@inject IAIService AIService
@inject IAIComplianceService ComplianceService
@inject ILogger<SecureChat> Logger

<div class="chat-container">
    @foreach (var message in Messages)
    {
        <div class="message @message.Role">
            <strong>@message.Author:</strong> @message.Text
        </div>
    }
</div>

<div class="input-area">
    <input @bind="CurrentMessage" @onkeydown="OnKeyDown" />
    <button @onclick="SendMessage" disabled="@IsProcessing">Send</button>
</div>

@if (DetectedIntent != null)
{
    <div class="intent-indicator">
        Intent: @DetectedIntent (@IntentConfidence.ToString("P"))
    </div>
}

@if (CurrentSentiment != null)
{
    <div class="sentiment-indicator">
        Sentiment: @CurrentSentiment (@SentimentConfidence.ToString("P"))
    </div>
}

@code {
    private List<ChatMessage> Messages { get; set; } = new();
    private string CurrentMessage { get; set; } = "";
    private bool IsProcessing { get; set; }
    private string? DetectedIntent { get; set; }
    private double? IntentConfidence { get; set; }
    private string? CurrentSentiment { get; set; }
    private double? SentimentConfidence { get; set; }
    private string UserId => "current-user-id";

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage) || IsProcessing)
            return;

        IsProcessing = true;
        var userMessage = CurrentMessage;
        CurrentMessage = "";

        try
        {
            // 1. Add user message
            Messages.Add(new ChatMessage
            {
                Role = "user",
                Author = "You",
                Text = userMessage
            });

            // 2. Detect PHI
            var phiElements = await ComplianceService.DetectPHIAsync(userMessage);
            
            if (phiElements.Any())
            {
                Logger.LogWarning("PHI detected in message - {Count} elements", phiElements.Count);
            }

            // 3. De-identify before AI processing
            var deIdentifiedMessage = await ComplianceService.DeIdentifyAsync(
                userMessage,
                phiElements
            );

            // 4. Analyze intent and sentiment in parallel
            var intentTask = AIService.AnalyzeIntentAsync(deIdentifiedMessage);
            var sentimentTask = AIService.AnalyzeSentimentAsync(deIdentifiedMessage);

            await Task.WhenAll(intentTask, sentimentTask);

            var intentResult = await intentTask;
            var sentimentResult = await sentimentTask;

            DetectedIntent = intentResult.Intent;
            IntentConfidence = intentResult.Confidence;
            CurrentSentiment = sentimentResult.Sentiment;
            SentimentConfidence = sentimentResult.Confidence;

            // 5. Generate AI response with context
            var context = GetConversationContext();
            var aiResponseText = "";

            // 6. Stream response for real-time UX
            var aiMessage = new ChatMessage
            {
                Role = "assistant",
                Author = "AI Assistant",
                Text = ""
            };
            Messages.Add(aiMessage);

            await foreach (var token in AIService.StreamResponseAsync(context, deIdentifiedMessage))
            {
                aiMessage.Text += token;
                aiResponseText += token;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(10); // Smooth visual effect
            }

            // 7. Audit the interaction
            await ComplianceService.AuditAIOperationAsync(
                "ChatInteraction",
                UserId,
                deIdentifiedMessage,
                aiResponseText
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            Messages.Add(new ChatMessage
            {
                Role = "assistant",
                Author = "System",
                Text = "I apologize, but I encountered an error. Please try again."
            });
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    private string GetConversationContext()
    {
        var recent = Messages.TakeLast(5);
        return string.Join("\n", recent.Select(m => $"{m.Author}: {m.Text}"));
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Author { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
```

## Advanced Scenarios

### 1. Custom Intent Types

If you need custom intent types, extend the service:

```csharp
public class CustomAIService : AzureOpenAIService
{
    public CustomAIService(HttpClient httpClient, string endpoint, string apiKey, string deploymentName)
        : base(httpClient, endpoint, apiKey, deploymentName)
    {
    }

    public async Task<IntentAnalysisResult> AnalyzeCustomIntentAsync(string message)
    {
        // Add your custom intent logic
        // Call base.AnalyzeIntentAsync() or implement custom logic
    }
}
```

### 2. Batch Processing

Process multiple messages efficiently:

```csharp
var messages = new[] { "Message 1", "Message 2", "Message 3" };

var intentTasks = messages.Select(m => AIService.AnalyzeIntentAsync(m));
var results = await Task.WhenAll(intentTasks);

foreach (var result in results)
{
    Console.WriteLine($"Intent: {result.Intent}, Confidence: {result.Confidence:P}");
}
```

### 3. Error Handling with Retry

```csharp
using Polly;
using Polly.Retry;

var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var result = await retryPolicy.ExecuteAsync(async () =>
{
    return await AIService.AnalyzeIntentAsync(message);
});
```

### 4. Caching Results

```csharp
@inject IMemoryCache Cache

private async Task<IntentAnalysisResult> GetIntentWithCacheAsync(string message)
{
    var cacheKey = $"intent:{message.GetHashCode()}";
    
    if (Cache.TryGetValue<IntentAnalysisResult>(cacheKey, out var cached))
    {
        return cached;
    }

    var result = await AIService.AnalyzeIntentAsync(message);
    
    Cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
    
    return result;
}
```

### 5. Monitoring and Metrics

```csharp
@inject ILogger<MyComponent> Logger

private async Task<string> GenerateResponseWithMetricsAsync(string context, string message)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    try
    {
        var response = await AIService.GenerateResponseAsync(context, message);
        
        stopwatch.Stop();
        Logger.LogInformation(
            "AI response generated in {Elapsed}ms, Length: {Length}",
            stopwatch.ElapsedMilliseconds,
            response.Length
        );
        
        return response;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        Logger.LogError(
            ex,
            "AI response failed after {Elapsed}ms",
            stopwatch.ElapsedMilliseconds
        );
        throw;
    }
}
```

## Testing

### Unit Testing AI Services

```csharp
using Xunit;
using Moq;
using Moq.Protected;

public class AzureOpenAIServiceTests
{
    [Fact]
    public async Task AnalyzeIntentAsync_ReturnsCorrectIntent()
    {
        // Arrange
        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""choices"": [{
                        ""message"": {
                            ""content"": ""{\""intent\"": \""AppointmentScheduling\"", \""confidence\"": 0.95}""
                        }
                    }]
                }")
            });

        var httpClient = new HttpClient(mockHttp.Object);
        var service = new AzureOpenAIService(
            httpClient,
            "https://test.openai.azure.com",
            "test-key",
            "gpt-4"
        );

        // Act
        var result = await service.AnalyzeIntentAsync("I need to book an appointment");

        // Assert
        Assert.Equal("AppointmentScheduling", result.Intent);
        Assert.True(result.Confidence > 0.9);
    }
}
```

### Testing HIPAA Compliance

```csharp
public class HIPAAComplianceServiceTests
{
    [Theory]
    [InlineData("MRN: MED123456", "MRN")]
    [InlineData("SSN: 123-45-6789", "SSN")]
    [InlineData("Call 555-123-4567", "PHONE")]
    public async Task DetectPHIAsync_DetectsExpectedTypes(string text, string expectedType)
    {
        // Arrange
        var service = new HIPAAComplianceService();

        // Act
        var results = await service.DetectPHIAsync(text);

        // Assert
        Assert.Contains(results, r => r.Type == expectedType);
    }

    [Fact]
    public async Task DeIdentifyAsync_RemovesPHI()
    {
        // Arrange
        var service = new HIPAAComplianceService();
        var text = "Patient John Doe (MRN: MED123456)";
        var phi = await service.DetectPHIAsync(text);

        // Act
        var deIdentified = await service.DeIdentifyAsync(text, phi);

        // Assert
        Assert.DoesNotContain("John Doe", deIdentified);
        Assert.DoesNotContain("MED123456", deIdentified);
        Assert.Contains("[PATIENT]", deIdentified);
        Assert.Contains("[MRN]", deIdentified);
    }
}
```

## Performance Considerations

### Response Times
- **Intent Analysis**: ~200-500ms
- **Sentiment Analysis**: ~200-500ms
- **Response Generation**: ~500-2000ms (depends on length)
- **Streaming**: ~50ms for first token, then ~10ms per token

### Cost Optimization
1. **Cache Results**: Cache intent/sentiment for identical messages (15-min TTL)
2. **Batch Requests**: Combine multiple analyses when possible
3. **Use Appropriate Models**: GPT-3.5 for simpler tasks, GPT-4 for complex
4. **Set Token Limits**: Use `max_tokens` parameter to control costs

### Scaling
- Services are registered as **Scoped** - new instance per request
- HttpClient is registered via `AddHttpClient` - connection pooling enabled
- Consider adding rate limiting for production

## Troubleshooting

### Common Issues

**Issue**: "The type or namespace name 'IAIService' could not be found"
- **Solution**: Ensure you've called `AddNalam360AIServices()` in Program.cs

**Issue**: "Request failed with status code 401"
- **Solution**: Check your API key in appsettings.json is correct

**Issue**: "Endpoint does not use HTTPS"
- **Solution**: HIPAA compliance requires HTTPS. Update endpoint to use `https://`

**Issue**: "Streaming responses not working"
- **Solution**: Ensure `EnableStreaming="true"` parameter is set on component

**Issue**: "PHI not being detected"
- **Solution**: Check PHI patterns match your data format. May need to adjust regex patterns.

## Security Best Practices

1. **Never log raw user input** - Always de-identify first
2. **Use Azure Key Vault** - Store API keys securely, not in appsettings.json
3. **Enable audit trail** - Log all AI operations with de-identified data
4. **Validate data residency** - Ensure Azure region is US-only for HIPAA
5. **Implement rate limiting** - Prevent abuse and control costs
6. **Monitor for anomalies** - Track unusual patterns in AI usage

## Next Steps

1. Deploy to Azure with App Service
2. Configure Azure Key Vault for secrets
3. Set up Application Insights for monitoring
4. Implement additional AI components using N360SmartChat as template
5. Add ML.NET models for predictive analytics
