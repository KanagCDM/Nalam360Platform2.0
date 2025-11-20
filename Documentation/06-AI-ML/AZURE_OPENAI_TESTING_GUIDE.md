# Azure OpenAI Testing Guide

## Prerequisites

Before testing the AI services with real Azure OpenAI, you need:

1. **Azure Subscription** with access to Azure OpenAI Service
2. **Azure OpenAI Resource** created in Azure Portal
3. **GPT-4 Model Deployment** in your Azure OpenAI instance
4. **API Key** from Azure OpenAI resource

## Step 1: Create Azure OpenAI Resource

### Using Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → Search for "Azure OpenAI"
3. Configure:
   - **Resource group**: Create or select existing
   - **Region**: Choose US region (e.g., East US, West US) for HIPAA compliance
   - **Name**: `nalam360-openai` (or your preferred name)
   - **Pricing tier**: Standard S0
4. Click "Review + Create" → "Create"
5. Wait for deployment (2-3 minutes)

### Using Azure CLI

```bash
# Login to Azure
az login

# Create resource group (if needed)
az group create --name nalam360-rg --location eastus

# Create Azure OpenAI resource
az cognitiveservices account create \
  --name nalam360-openai \
  --resource-group nalam360-rg \
  --kind OpenAI \
  --sku S0 \
  --location eastus \
  --yes

# Get the endpoint
az cognitiveservices account show \
  --name nalam360-openai \
  --resource-group nalam360-rg \
  --query properties.endpoint \
  --output tsv

# Get the API key
az cognitiveservices account keys list \
  --name nalam360-openai \
  --resource-group nalam360-rg \
  --query key1 \
  --output tsv
```

## Step 2: Deploy GPT-4 Model

### Using Azure Portal

1. Go to your Azure OpenAI resource
2. Click "Model deployments" → "Create new deployment"
3. Configure:
   - **Model**: gpt-4
   - **Model version**: 0613 (or latest available)
   - **Deployment name**: `gpt-4` (use this in your configuration)
   - **Capacity**: Start with 10K tokens per minute
4. Click "Create"

### Using Azure CLI

```bash
# Deploy GPT-4 model
az cognitiveservices account deployment create \
  --name nalam360-openai \
  --resource-group nalam360-rg \
  --deployment-name gpt-4 \
  --model-name gpt-4 \
  --model-version "0613" \
  --model-format OpenAI \
  --sku-capacity 10 \
  --sku-name "Standard"
```

## Step 3: Configure Application

### Update appsettings.Development.json

Replace the placeholder values in `appsettings.Development.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://nalam360-openai.openai.azure.com",
    "ApiKey": "your-actual-32-character-api-key",
    "DeploymentName": "gpt-4"
  }
}
```

**Security Best Practice**: Use Azure Key Vault instead of storing API key in config file.

### Using Azure Key Vault (Recommended)

```bash
# Create Key Vault
az keyvault create \
  --name nalam360-vault \
  --resource-group nalam360-rg \
  --location eastus

# Store API key in Key Vault
az keyvault secret set \
  --vault-name nalam360-vault \
  --name "AzureOpenAI--ApiKey" \
  --value "your-actual-api-key"

# Grant access to your application
az keyvault set-policy \
  --name nalam360-vault \
  --object-id <your-app-service-managed-identity-id> \
  --secret-permissions get list
```

Update `Program.cs` to use Key Vault:

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://nalam360-vault.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Step 4: Run Integration Tests

### Test 1: Basic Connectivity

Run this PowerShell script to test Azure OpenAI connectivity:

```powershell
$endpoint = "https://nalam360-openai.openai.azure.com"
$apiKey = "your-api-key"
$deploymentName = "gpt-4"

$headers = @{
    "api-key" = $apiKey
    "Content-Type" = "application/json"
}

$body = @{
    messages = @(
        @{
            role = "system"
            content = "You are a helpful healthcare assistant."
        },
        @{
            role = "user"
            content = "Hello, can you help me?"
        }
    )
    temperature = 0.7
    max_tokens = 150
} | ConvertTo-Json -Depth 10

$response = Invoke-RestMethod `
    -Uri "$endpoint/openai/deployments/$deploymentName/chat/completions?api-version=2024-02-15-preview" `
    -Method Post `
    -Headers $headers `
    -Body $body

$response.choices[0].message.content
```

### Test 2: N360SmartChat Component

1. Update `UseRealAI="true"` in your component usage
2. Run the example application:

```powershell
cd "d:\Mocero\Healthcare Platform\Nalam360EnterprisePlatform\examples\Nalam360.Platform.Example.Api"
dotnet run
```

3. Navigate to the page with N360SmartChat
4. Send test messages:
   - "I need to schedule an appointment" → Should detect **AppointmentScheduling** intent
   - "I'm feeling very anxious about my surgery" → Should detect **negative** sentiment
   - "My MRN is ABC123456" → Should detect and de-identify **PHI**

### Test 3: Run Unit Tests (After fixing UI build)

```powershell
dotnet test "tests\Nalam360Enterprise.UI.Tests\Nalam360Enterprise.UI.Tests\Nalam360Enterprise.UI.Tests.csproj" --filter "FullyQualifiedName~AI"
```

## Step 5: Validate HIPAA Compliance

### Check Data Residency

```csharp
var complianceService = serviceProvider.GetRequiredService<IAIComplianceService>();
var isCompliant = await complianceService.ValidateDataResidencyAsync(
    "https://nalam360-openai.openai.azure.com",
    "eastus",
    CancellationToken.None);
// Should return true for US regions
```

### Check Encryption

```csharp
var isEncrypted = await complianceService.ValidateEncryptionAsync(
    "https://nalam360-openai.openai.azure.com",
    CancellationToken.None);
// Should return true (HTTPS)
```

### Test PHI Detection

```csharp
var text = "Patient John Doe, MRN: ABC123456, SSN 123-45-6789";
var phiElements = await complianceService.DetectPHIAsync(text, CancellationToken.None);
// Should detect: NAME, MRN, SSN
```

## Step 6: Monitor and Optimize

### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app nalam360-insights \
  --location eastus \
  --resource-group nalam360-rg \
  --application-type web

# Get instrumentation key
az monitor app-insights component show \
  --app nalam360-insights \
  --resource-group nalam360-rg \
  --query instrumentationKey \
  --output tsv
```

Add to `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

### Monitor Costs

1. Go to Azure Portal → Cost Management + Billing
2. Set up budget alerts:
   - **Monthly budget**: $100-500 (adjust based on usage)
   - **Alert threshold**: 80%, 90%, 100%
3. Monitor token usage in Azure OpenAI metrics

### Optimize Performance

```csharp
// Use caching to reduce API calls
services.AddMemoryCache();
services.AddScoped<IAIService>(sp =>
{
    var baseService = new AzureOpenAIService(...);
    return new CachedAIService(baseService, sp.GetRequiredService<IMemoryCache>());
});
```

## Expected Results

### Intent Analysis
- **Response time**: 200-800ms
- **Accuracy**: 95%+ for healthcare intents
- **Format**: `{ "intent": "AppointmentScheduling", "confidence": 0.95, "entities": {...} }`

### Sentiment Analysis
- **Response time**: 200-800ms
- **Accuracy**: 90%+ for emotional sentiment
- **Format**: `{ "sentiment": "positive", "confidence": 0.92, "scores": {...} }`

### PHI Detection
- **Response time**: <50ms (local regex)
- **Accuracy**: 60-98% depending on type
- **Types detected**: MRN (95%), SSN (98%), PHONE (85%), DATE (80%), EMAIL (90%), ADDRESS (75%), NAME (60%)

### Response Generation
- **Response time**: 1000-2000ms
- **Quality**: Context-aware, professional healthcare language
- **Max tokens**: 300 (configurable)

## Troubleshooting

### Error 401: Unauthorized
**Cause**: Invalid API key
**Solution**: 
```powershell
# Get correct API key
az cognitiveservices account keys list --name nalam360-openai --resource-group nalam360-rg --query key1 --output tsv
```

### Error 404: Deployment Not Found
**Cause**: Incorrect deployment name
**Solution**: 
```powershell
# List deployments
az cognitiveservices account deployment list --name nalam360-openai --resource-group nalam360-rg --query "[].name"
```

### Error 429: Rate Limit Exceeded
**Cause**: Too many requests
**Solution**: 
- Implement retry with exponential backoff (included in AzureOpenAIService)
- Increase deployment capacity in Azure Portal
- Implement caching to reduce API calls

### Streaming Not Working
**Cause**: Response not recognized as event stream
**Solution**: Ensure `Content-Type: text/event-stream` in response headers

### PHI Not Detected
**Cause**: Text format doesn't match regex patterns
**Solution**: Check `HIPAAComplianceService.PhiPatterns` and add custom patterns if needed

## Next Steps

After successful testing:

1. ✅ Update remaining 17 AI components with `UseRealAI="true"`
2. ✅ Deploy to Azure App Service
3. ✅ Configure production Key Vault
4. ✅ Set up Application Insights monitoring
5. ✅ Implement rate limiting
6. ✅ Create CI/CD pipeline
7. ✅ Perform load testing

## Cost Estimation

### Azure OpenAI Pricing (as of 2024)

**GPT-4** (0613 model):
- **Input**: $0.03 per 1K tokens
- **Output**: $0.06 per 1K tokens

**Estimated Monthly Cost** (based on usage):
- 1,000 chat interactions/month: ~$50-100
- 10,000 chat interactions/month: ~$500-1,000
- 100,000 chat interactions/month: ~$5,000-10,000

**Cost Optimization Tips**:
- Use caching (15-minute TTL) → Reduce API calls by 40-60%
- Implement shorter max_tokens for simple responses
- Use GPT-3.5-Turbo for non-critical features (80% cheaper)
- Batch processing for multiple requests

## Support

For issues or questions:
- **Azure OpenAI Documentation**: https://learn.microsoft.com/azure/ai-services/openai/
- **GitHub Issues**: https://github.com/KanagCDM/Nalam360EnterprisePlatform/issues
- **Team Slack**: #nalam360-ai-services
