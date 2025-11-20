# Azure OpenAI Setup Guide

## Quick Start - Get Your Azure OpenAI Credentials

### Option 1: Azure Portal (Recommended)

1. **Create Azure OpenAI Resource:**
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
     --location eastus
   ```

2. **Deploy GPT-4 Model:**
   ```bash
   # Deploy GPT-4 model
   az cognitiveservices account deployment create \
     --name nalam360-openai \
     --resource-group nalam360-rg \
     --deployment-name gpt-4 \
     --model-name gpt-4 \
     --model-version 0613 \
     --sku-capacity 10 \
     --sku-name Standard
   ```

3. **Get Credentials:**
   ```bash
   # Get endpoint
   az cognitiveservices account show \
     --name nalam360-openai \
     --resource-group nalam360-rg \
     --query "properties.endpoint" -o tsv
   
   # Get API key
   az cognitiveservices account keys list \
     --name nalam360-openai \
     --resource-group nalam360-rg \
     --query "key1" -o tsv
   ```

### Option 2: Azure Portal UI

1. Go to [Azure Portal](https://portal.azure.com)
2. Search for "Azure OpenAI" and click "Create"
3. Fill in:
   - **Resource Group:** nalam360-rg (or create new)
   - **Region:** East US (recommended for healthcare)
   - **Name:** nalam360-openai
   - **Pricing Tier:** Standard S0
4. Click "Review + Create"
5. Once deployed, go to resource → "Keys and Endpoint"
   - Copy **Endpoint URL**
   - Copy **Key 1** (API Key)
6. Go to "Model deployments" → "Manage Deployments" → "Create new deployment"
   - **Model:** gpt-4
   - **Deployment name:** gpt-4
   - **Model version:** 0613 (recommended)
   - **Deployment type:** Standard
   - **Tokens per minute:** 10K (adjust based on needs)

## Configure Application

### Update appsettings.Development.json

Replace the placeholder values:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://nalam360-openai.openai.azure.com",  // Your endpoint
    "ApiKey": "your-actual-api-key-here",                     // Your API key
    "DeploymentName": "gpt-4",                                // Your deployment name
    "TimeoutSeconds": 60,
    "EnablePHIDetection": true,
    "EnableAuditLogging": true
  }
}
```

**Location:** `examples/Nalam360.Platform.Example.Api/appsettings.Development.json`

⚠️ **IMPORTANT:** Never commit real API keys to source control!

## Test Connectivity

### Quick Test

```powershell
# Run test script
.\test-azure-openai.ps1

# Or with explicit parameters
.\test-azure-openai.ps1 `
  -Endpoint "https://nalam360-openai.openai.azure.com" `
  -ApiKey "your-api-key" `
  -DeploymentName "gpt-4"
```

**Expected Output:**
```
=== Azure OpenAI Service Test Script ===
Configuration:
  Endpoint: https://nalam360-openai.openai.azure.com
  Deployment: gpt-4
  API Key: sk-abc12...

[Test 1/5] Basic Connectivity...
✓ Connected successfully to Azure OpenAI

[Test 2/5] Intent Analysis...
✓ Intent analysis working (Response time: 450ms)

[Test 3/5] Sentiment Analysis...
✓ Sentiment analysis working (Response time: 520ms)

[Test 4/5] Streaming Endpoint...
✓ Streaming endpoint accessible

[Test 5/5] PHI Detection...
✓ PHI detection patterns working (7 types detected)

=== Summary ===
✓ All tests passed! (5/5)
Average response time: 485ms
```

## Cost Estimation

### GPT-4 Pricing (East US - November 2025)

| Model | Input (per 1K tokens) | Output (per 1K tokens) |
|-------|----------------------|------------------------|
| GPT-4 | $0.03 | $0.06 |
| GPT-4-32K | $0.06 | $0.12 |

### Expected Costs per Component

| Component | Avg Tokens | Cost per Request | Daily Cost (100 users, 10 requests each) |
|-----------|------------|------------------|------------------------------------------|
| SmartChat | 500 | $0.025 | $25.00 |
| PredictiveAnalytics | 800 | $0.040 | $40.00 |
| ClinicalDecisionSupport | 1200 | $0.060 | $60.00 |
| DocumentIntelligence | 2000 | $0.100 | $100.00 |
| **Total (4 high-use)** | - | - | **$225/day** (~$6,750/month) |

### Cost Optimization Strategies

1. **Use ML.NET for Predictable Tasks** (Recommended ✅)
   - Readmission risk: ML.NET (free) vs GPT-4 ($0.04/request)
   - Savings: ~$4,000/month for 100K predictions
   
2. **Enable Caching** (40-60% savings)
   - Cache similar queries for 15 minutes
   - Example: 1000 requests → 400 cache hits → $24 saved per day
   
3. **Rate Limiting**
   - Limit: 100 requests/user/hour
   - Prevents runaway costs from errors or abuse
   
4. **Token Optimization**
   - Use shorter system prompts
   - Truncate long inputs (keep last 2000 tokens)
   - Request fewer output tokens (max_tokens parameter)

## Security Best Practices

### 1. Store Credentials Securely

**Azure Key Vault (Recommended):**
```bash
# Store API key in Key Vault
az keyvault secret set \
  --vault-name nalam360-kv \
  --name AzureOpenAI-ApiKey \
  --value "your-api-key"

# Reference in appsettings
{
  "AzureOpenAI": {
    "ApiKey": "@Microsoft.KeyVault(SecretUri=https://nalam360-kv.vault.azure.net/secrets/AzureOpenAI-ApiKey/)"
  }
}
```

**User Secrets (Development):**
```powershell
# Set user secrets
cd examples\Nalam360.Platform.Example.Api
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://nalam360-openai.openai.azure.com"
```

### 2. Enable Managed Identity (Production)

```bash
# Enable system-assigned managed identity on App Service
az webapp identity assign \
  --name nalam360-api \
  --resource-group nalam360-rg

# Grant identity access to Azure OpenAI
az role assignment create \
  --assignee <managed-identity-id> \
  --role "Cognitive Services OpenAI User" \
  --scope /subscriptions/<subscription-id>/resourceGroups/nalam360-rg/providers/Microsoft.CognitiveServices/accounts/nalam360-openai
```

### 3. Network Security

```bash
# Restrict Azure OpenAI to private endpoint
az cognitiveservices account update \
  --name nalam360-openai \
  --resource-group nalam360-rg \
  --public-network-access Disabled

# Create private endpoint
az network private-endpoint create \
  --name nalam360-openai-pe \
  --resource-group nalam360-rg \
  --vnet-name nalam360-vnet \
  --subnet default \
  --private-connection-resource-id /subscriptions/<subscription-id>/resourceGroups/nalam360-rg/providers/Microsoft.CognitiveServices/accounts/nalam360-openai \
  --group-id account \
  --connection-name nalam360-openai-connection
```

## HIPAA Compliance Configuration

All 17 AI components have built-in HIPAA compliance features:

### PHI Detection (7 Types)

| Type | Pattern | Confidence |
|------|---------|-----------|
| Medical Record Number (MRN) | MRN#12345678 | 95% |
| Social Security Number (SSN) | 123-45-6789 | 98% |
| Phone Number | (555) 123-4567 | 85% |
| Date of Birth | DOB: 01/15/1980 | 80% |
| Email | patient@email.com | 90% |
| Address | 123 Main St, City, ST 12345 | 75% |
| Patient Name | John Doe | 60% |

### Enable PHI Protection

```razor
<N360PredictiveAnalytics 
    UseRealAI="true"
    EnablePHIDetection="true"    <!-- Automatically detects and de-identifies PHI -->
    AIModelEndpoint="@endpoint"
    AIApiKey="@apiKey"
    UserId="user-123" />
```

### Audit Logging

All AI operations are automatically logged:

```csharp
// Automatic audit entry for each AI operation
{
    "Timestamp": "2025-11-19T10:30:45Z",
    "Component": "PredictiveAnalytics",
    "UserId": "user-123",
    "Operation": "GeneratePrediction",
    "InputLength": 1024,
    "OutputLength": 512,
    "PHIDetected": true,
    "PHITypes": ["MRN", "SSN"],
    "DeIdentified": true,
    "ResponseTime": "1.2s",
    "Model": "gpt-4"
}
```

## Monitoring & Alerts

### Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app nalam360-ai-insights \
  --location eastus \
  --resource-group nalam360-rg

# Get instrumentation key
az monitor app-insights component show \
  --app nalam360-ai-insights \
  --resource-group nalam360-rg \
  --query "instrumentationKey" -o tsv
```

**Add to appsettings.json:**
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

### Key Metrics to Monitor

1. **Response Time**
   - Target: <2s for 90% of requests
   - Alert: >5s for any request

2. **Error Rate**
   - Target: <1% errors
   - Alert: >5% errors in 5-minute window

3. **Cost per Request**
   - Target: <$0.05 average
   - Alert: >$0.10 per request

4. **PHI Detection Rate**
   - Monitor: % of requests with PHI detected
   - Alert: Sudden increase (potential data exposure)

5. **Cache Hit Rate**
   - Target: >40% cache hits
   - Alert: <20% cache hits (optimization needed)

## Test with Real AI Components

### Example: Test PredictiveAnalytics

```razor
@page "/test-ai"
@inject IConfiguration Configuration

<h3>Test Real AI - Predictive Analytics</h3>

<N360PredictiveAnalytics 
    UseRealAI="true"
    AIModelEndpoint="@Configuration["AzureOpenAI:Endpoint"]"
    AIApiKey="@Configuration["AzureOpenAI:ApiKey"]"
    EnablePHIDetection="true"
    UserId="test-user-123"
    PatientId="P123456"
    Age="65"
    PrimaryDiagnosis="Diabetes Type 2"
    SecondaryDiagnoses="@(new[] { "Hypertension", "CAD" })" />

@code {
    // Test data with mock PHI
    private string testPatientData = @"
        Patient: John Doe
        MRN: MRN#12345678
        DOB: 01/15/1958
        SSN: 123-45-6789
        Phone: (555) 123-4567
        Diagnosis: Diabetes Type 2, Hypertension, CAD
        Last A1C: 8.5
        Blood Pressure: 145/92
    ";
}
```

**Expected Behavior:**
1. ✅ Real AI badge shows in UI
2. ✅ PHI detected: MRN, SSN, Phone, DOB, Name
3. ✅ PHI de-identified before sending to Azure OpenAI
4. ✅ Prediction generated using GPT-4
5. ✅ Audit log entry created
6. ✅ Response time <2s
7. ✅ Graceful fallback to mock AI on errors

## Troubleshooting

### Common Issues

**1. "Unauthorized" Error (401)**
```
Error: 401 Unauthorized
```
**Solution:** 
- Verify API key is correct
- Check API key hasn't been regenerated in Azure Portal
- Ensure endpoint URL includes "https://"

**2. "Deployment Not Found" (404)**
```
Error: 404 The API deployment for this resource does not exist
```
**Solution:**
- Verify deployment name matches exactly (case-sensitive)
- Check deployment is in "Succeeded" state in Azure Portal
- Default deployment name: "gpt-4"

**3. "Rate Limit Exceeded" (429)**
```
Error: 429 Rate limit exceeded
```
**Solution:**
- Wait and retry (automatic retry after 60s)
- Increase tokens per minute in Azure Portal
- Enable caching to reduce requests

**4. "Timeout" Error**
```
Error: Request timeout after 60 seconds
```
**Solution:**
- Check network connectivity
- Increase TimeoutSeconds in appsettings
- Try simpler prompts with fewer tokens

**5. PHI Not Detected**
```
Warning: Expected PHI not detected in input
```
**Solution:**
- Check PHI patterns in ComplianceService
- Verify EnablePHIDetection="true"
- Test with sample data from examples above

## Next Steps

✅ **Setup Complete?** Test connectivity:
```powershell
.\test-azure-openai.ps1
```

✅ **All Tests Pass?** Deploy ML.NET models:
```powershell
.\deploy-ml-models.ps1
```

✅ **Ready for Production?** Enable gradual rollout:
```csharp
// Start with 3 high-priority components
services.AddNalam360EnterpriseUI<CustomPermissionService>(options =>
{
    options.EnableRealAI = true;
    options.EnabledComponents = new[] 
    { 
        "PredictiveAnalytics", 
        "ClinicalDecisionSupport", 
        "SmartChat" 
    };
});
```

## Resources

- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [AI_COMPONENTS_UPDATE_COMPLETE.md](./AI_COMPONENTS_UPDATE_COMPLETE.md) - All 17 AI components guide
- [HIPAA Compliance Guide](./docs/HIPAA_COMPLIANCE_GUIDE.md) - Healthcare compliance
- [Cost Optimization Guide](./docs/COST_OPTIMIZATION_GUIDE.md) - Save 40-60% on AI costs

---

**Questions?** Check [AI_COMPONENTS_UPDATE_COMPLETE.md](./AI_COMPONENTS_UPDATE_COMPLETE.md) for detailed usage examples.
