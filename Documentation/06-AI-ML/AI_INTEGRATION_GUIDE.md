# AI Services Integration Guide

## Quick Start

This guide shows how to integrate the AI services into your Blazor application.

### 1. Install NuGet Packages

The following packages are already included in `Nalam360Enterprise.UI`:

```xml
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="9.0.0" />
```

### 2. Configure Services

#### Option A: Using appsettings.json

**appsettings.json:**
```json
{
  "AI": {
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/openai/deployments/gpt-4/chat/completions?api-version=2024-02-15-preview",
      "ApiKey": "your-api-key-here"
    }
  }
}
```

**Program.cs:**
```csharp
using Nalam360Enterprise.UI.Core.AI;

var builder = WebApplication.CreateBuilder(args);

// Add AI services
builder.Services.AddNalam360AIServices(builder.Configuration);

var app = builder.Build();
app.Run();
```

#### Option B: Using Code Configuration

**Program.cs:**
```csharp
using Nalam360Enterprise.UI.Core.AI;

var builder = WebApplication.CreateBuilder(args);

// Add AI services with custom configuration
builder.Services.AddNalam360AIServices(options =>
{
    options.AzureOpenAIEndpoint = "https://your-resource.openai.azure.com/openai/deployments/gpt-4/chat/completions?api-version=2024-02-15-preview";
    options.AzureOpenAIApiKey = "your-api-key-here";
    options.DeploymentName = "gpt-4";
    options.TimeoutSeconds = 120;
    options.MaxRetries = 3;
    options.EnableHIPAACompliance = true;
    options.AutoDeIdentifyPHI = true;
    options.AllowedRegions = new List<string> { "us", "usa", "united-states" };
});

var app = builder.Build();
app.Run();
```

### 3. Use AI Services in Components

#### Example: Smart Chat Component

```razor
@page "/chat"
@using Nalam360Enterprise.UI.Core.AI.Services
@using Nalam360Enterprise.UI.Core.AI.Models
@inject IAIService AIService
@inject IAIComplianceService ComplianceService

<N360SmartChat 
    EnableAIResponse="true"
    EnableIntentRecognition="true"
    EnableSentimentAnalysis="true"
    AIModelEndpoint="@aiEndpoint"
    AIApiKey="@aiApiKey"
    OnMessageSent="HandleMessageAsync"
    RequiredPermission="USE_AI_CHAT" />

@code {
    private string aiEndpoint = "https://your-resource.openai.azure.com/...";
    private string aiApiKey = "your-api-key";
    
    private async Task HandleMessageAsync(string message)
    {
        // De-identify PHI before sending to AI
        var deidentified = await ComplianceService.DeIdentifyAsync(message);
        
        // Analyze intent
        var intent = await AIService.AnalyzeIntentAsync(deidentified, aiEndpoint, aiApiKey);
        
        // Generate response
        var response = await AIService.GenerateResponseAsync("", deidentified, aiEndpoint, aiApiKey);
        
        // Audit the operation
        await ComplianceService.AuditAIOperationAsync(
            "ChatInteraction",
            "user-id",
            deidentified,
            response);
    }
}
```

#### Example: Predictive Analytics

```razor
@page "/analytics"
@using Nalam360Enterprise.UI.Core.AI.Services
@inject IMLModelService MLService

<N360PredictiveAnalytics 
    OnPredictionRequested="HandlePredictionAsync"
    RequiredPermission="VIEW_ANALYTICS" />

@code {
    private async Task HandlePredictionAsync(object features)
    {
        // Make prediction using ML model
        var result = await MLService.PredictAsync<double>(
            "readmission-risk",
            features);
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"Predicted risk: {result.Value:P2}");
            Console.WriteLine($"Confidence: {result.Confidence:P2}");
            
            // Show feature importance
            foreach (var feature in result.FeatureImportance.OrderByDescending(f => f.Value))
            {
                Console.WriteLine($"{feature.Key}: {feature.Value:P2}");
            }
        }
    }
}
```

### 4. HIPAA Compliance

#### Automatic PHI De-identification

```csharp
@inject IAIComplianceService ComplianceService

// Detect PHI
var phiElements = await ComplianceService.DetectPHIAsync(patientNote);

// De-identify before AI processing
var deidentified = await ComplianceService.DeIdentifyAsync(patientNote);

// Validate data residency
var isCompliant = await ComplianceService.ValidateDataResidencyAsync(aiEndpoint);

// Validate encryption
var isSecure = await ComplianceService.ValidateEncryptionAsync(aiEndpoint);

// Audit AI operation
await ComplianceService.AuditAIOperationAsync(
    "Prediction",
    userId,
    deidentified,
    result);
```

#### Example: PHI Detection

```csharp
var text = "Patient John Smith (MRN: ABC123456) called from 555-1234";

var phiElements = await ComplianceService.DetectPHIAsync(text);

foreach (var phi in phiElements)
{
    Console.WriteLine($"{phi.Type}: {phi.Value} (confidence: {phi.Confidence:P2})");
}

// Output:
// NAME: John (confidence: 60%)
// NAME: Smith (confidence: 60%)
// MRN: ABC123456 (confidence: 95%)
// PHONE: 555-1234 (confidence: 85%)

var deidentified = await ComplianceService.DeIdentifyAsync(text);
// Output: "Patient [PATIENT] [PATIENT] (MRN: [MRN]) called from [PHONE]"
```

### 5. ML Model Management

#### Preload Models

```csharp
@inject IMLModelService MLService

protected override async Task OnInitializedAsync()
{
    // Preload models for faster predictions
    await MLService.PreloadModelAsync("readmission-risk");
    await MLService.PreloadModelAsync("length-of-stay");
    await MLService.PreloadModelAsync("mortality-risk");
}
```

#### Check Model Status

```csharp
var isLoaded = await MLService.IsModelLoadedAsync("readmission-risk");

var metadata = await MLService.GetModelMetadataAsync("readmission-risk");
Console.WriteLine($"Model: {metadata.Name}");
Console.WriteLine($"Version: {metadata.Version}");
Console.WriteLine($"Accuracy: {metadata.Metrics["Accuracy"]:P2}");
```

#### Batch Predictions

```csharp
var patientFeatures = new List<object>
{
    new { Age = 72, Comorbidities = 3, PreviousAdmissions = 2 },
    new { Age = 45, Comorbidities = 1, PreviousAdmissions = 0 },
    new { Age = 68, Comorbidities = 4, PreviousAdmissions = 3 }
};

var results = await MLService.PredictBatchAsync<double>(
    "readmission-risk",
    patientFeatures);

Console.WriteLine($"Processed {results.Predictions.Count} predictions");
Console.WriteLine($"Success: {results.SuccessCount}, Failed: {results.FailureCount}");
```

### 6. Streaming AI Responses

```razor
@inject IAIService AIService

<div class="chat-message ai-response">@aiResponse</div>

@code {
    private string aiResponse = "";
    
    private async Task StreamAIResponse(string prompt)
    {
        aiResponse = "";
        StateHasChanged();
        
        await foreach (var token in AIService.StreamResponseAsync("", prompt, aiEndpoint, aiApiKey))
        {
            aiResponse += token;
            StateHasChanged();
            await Task.Delay(10); // Smooth streaming effect
        }
    }
}
```

### 7. Error Handling

```csharp
try
{
    var result = await AIService.GenerateResponseAsync(context, prompt, endpoint, apiKey);
    
    // AI service uses fallback logic - check for errors in response
    if (result.Contains("error") || result.Contains("couldn't generate"))
    {
        // Handle AI service error
        logger.LogWarning("AI service returned error response");
    }
}
catch (HttpRequestException ex)
{
    // Network error
    logger.LogError(ex, "Failed to connect to AI service");
}
catch (TaskCanceledException ex)
{
    // Timeout
    logger.LogError(ex, "AI service request timed out");
}
```

## Azure OpenAI Setup

### 1. Create Azure OpenAI Resource

```bash
az cognitiveservices account create \
  --name your-openai-resource \
  --resource-group your-rg \
  --kind OpenAI \
  --sku S0 \
  --location eastus \
  --custom-domain your-openai-resource
```

### 2. Deploy GPT-4 Model

```bash
az cognitiveservices account deployment create \
  --name your-openai-resource \
  --resource-group your-rg \
  --deployment-name gpt-4 \
  --model-name gpt-4 \
  --model-version "0613" \
  --model-format OpenAI \
  --sku-capacity 10 \
  --sku-name "Standard"
```

### 3. Get API Key

```bash
az cognitiveservices account keys list \
  --name your-openai-resource \
  --resource-group your-rg
```

### 4. Configure Endpoint

Endpoint format:
```
https://{your-resource-name}.openai.azure.com/openai/deployments/{deployment-name}/chat/completions?api-version=2024-02-15-preview
```

## Security Best Practices

### 1. Secure API Keys

**Never commit API keys to source control!**

Use Azure Key Vault or user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "AI:AzureOpenAI:ApiKey" "your-api-key"
```

### 2. Data Residency Compliance

Configure allowed regions:

```csharp
options.AllowedRegions = new List<string> { "us-east", "us-west" };
```

### 3. PHI De-identification

Always de-identify before sending to external AI:

```csharp
var deidentified = await ComplianceService.DeIdentifyAsync(clinicalNote);
var result = await AIService.AnalyzeIntentAsync(deidentified, endpoint, apiKey);
```

### 4. Audit Trail

Enable comprehensive auditing:

```csharp
await ComplianceService.AuditAIOperationAsync(
    operation: "IntentAnalysis",
    userId: currentUserId,
    dataProcessed: deidentifiedData,
    result: analysisResult);
```

## Performance Optimization

### 1. Enable Caching

```csharp
options.EnableCaching = true;
options.CacheDurationMinutes = 15;
```

### 2. Request Batching

Batch multiple predictions:

```csharp
var results = await MLService.PredictBatchAsync<double>(modelId, featuresList);
```

### 3. Model Preloading

Load frequently used models at startup:

```csharp
await MLService.PreloadModelAsync("readmission-risk");
```

### 4. Circuit Breaker

Configured automatically with Polly:
- 3 retries with exponential backoff
- Circuit breaker opens after 5 consecutive failures
- 30-second break duration

## Cost Optimization

### Estimated Costs (Azure OpenAI)

- **GPT-4:** ~$0.03/1K input tokens, ~$0.06/1K output tokens
- **GPT-3.5-Turbo:** ~$0.0015/1K input tokens, ~$0.002/1K output tokens

### Optimization Strategies

1. **Use caching** - 70% cost reduction for repeated queries
2. **Use GPT-3.5 for simple tasks** - 95% cheaper than GPT-4
3. **Implement request batching** - 30% reduction in API calls
4. **Set max_tokens limits** - Control output length
5. **Use on-device ML models** - No API costs for predictions

## Troubleshooting

### Issue: "Circuit breaker opened"

**Cause:** Too many failed requests  
**Solution:** Check API key, endpoint URL, and network connectivity

### Issue: "PHI detected in response"

**Cause:** AI response contains patient information  
**Solution:** Enable `AutoDeIdentifyPHI` option

### Issue: "Model not found"

**Cause:** Model ID doesn't exist in registry  
**Solution:** Check registered models:

```csharp
var metadata = await MLService.GetModelMetadataAsync(modelId);
```

### Issue: "Timeout"

**Cause:** AI request taking too long  
**Solution:** Increase timeout:

```csharp
options.TimeoutSeconds = 180; // 3 minutes
```

## Next Steps

1. ✅ Configure Azure OpenAI endpoint and API key
2. ✅ Add AI services to your application
3. ✅ Enable HIPAA compliance features
4. ✅ Test AI components with real data
5. ✅ Monitor costs and performance
6. ⏭️ Deploy ML.NET models for on-device predictions
7. ⏭️ Implement SignalR for real-time streaming
8. ⏭️ Add AI explainability features

## Support

For issues or questions:
- Create an issue on GitHub
- Check the `AI_COMPONENTS_ANALYSIS.md` document
- Review `SESSION_11_COMPLETE.md` for implementation details
