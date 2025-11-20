# AI Services Quick Test Guide

## 🚀 Quickest Way to Test (No Build Required)

```powershell
# Run this from project root
.\test-azure-openai.ps1
```

**Requirements:**
- Azure OpenAI resource with GPT-4 deployment
- Update `examples/Nalam360.Platform.Example.Api/appsettings.Development.json`

## ⚙️ Configuration (60 Seconds)

1. **Get Azure OpenAI Credentials:**
   ```bash
   az cognitiveservices account show --name nalam360-openai --resource-group nalam360-rg --query properties.endpoint
   az cognitiveservices account keys list --name nalam360-openai --resource-group nalam360-rg --query key1
   ```

2. **Update appsettings.Development.json:**
   ```json
   {
     "AzureOpenAI": {
       "Endpoint": "https://nalam360-openai.openai.azure.com",
       "ApiKey": "your-32-character-api-key",
       "DeploymentName": "gpt-4"
     }
   }
   ```

3. **Run Test Script:**
   ```powershell
   .\test-azure-openai.ps1
   ```

## ✅ Expected Output

```
=== Azure OpenAI Service Test Script ===
Test 1: Basic Connectivity
✓ Connection successful!

Test 2: Intent Analysis
✓ Intent analysis successful!
✓ Correct intent detected: AppointmentScheduling

Test 3: Sentiment Analysis
✓ Sentiment analysis successful!
✓ Correct sentiment detected: Negative

Test 4: Streaming Response
✓ Streaming endpoint responding!

Test 5: PHI Detection (Local Regex)
✓ PHI Detection working!

=== Test Summary ===
✓ All 5 tests passed
```

## 🧪 What Gets Tested

1. **Basic Connectivity** - Simple chat completion request
2. **Intent Analysis** - "schedule appointment" → AppointmentScheduling (95%+)
3. **Sentiment Analysis** - "anxious about surgery" → Negative (92%+)
4. **Streaming** - Real-time token streaming endpoint
5. **PHI Detection** - 7 types (MRN, SSN, PHONE, DATE, EMAIL, ADDRESS, NAME)

## 📊 Performance Targets

| Test | Target Time | Accuracy |
|------|-------------|----------|
| Intent | 200-800ms | 95%+ |
| Sentiment | 200-800ms | 90%+ |
| PHI | <50ms | 60-98% |
| Response | 1-2s | Context-aware |

## 🔧 Troubleshooting

### ❌ Error 401: Unauthorized
**Fix:** Check API key in appsettings.Development.json
```bash
az cognitiveservices account keys list --name nalam360-openai --resource-group nalam360-rg
```

### ❌ Error 404: Deployment Not Found
**Fix:** Verify deployment name
```bash
az cognitiveservices account deployment list --name nalam360-openai --resource-group nalam360-rg
```

### ⚠️ Using Placeholder Values
**Fix:** Update appsettings.Development.json with real values (not "your-openai-instance" or "your-api-key")

## 📚 Full Documentation

- **Complete Guide:** `docs/AZURE_OPENAI_TESTING_GUIDE.md`
- **API Reference:** `AI_SERVICES_USAGE_GUIDE.md`
- **Test Summary:** `AI_TESTING_COMPLETE.md`

## 💰 Cost Estimate

**Development Testing (1,000 interactions/month):**
- Estimated cost: $50-100/month
- Per test run: ~$0.05-0.10

**With Caching (40-60% reduction):**
- Monthly cost: $20-60/month

## 🎯 Next Steps

1. ✅ Run `.\test-azure-openai.ps1` (2 minutes)
2. ⏳ Test N360SmartChat with `UseRealAI="true"`
3. ⏳ Run unit tests: `dotnet test --filter "FullyQualifiedName~AI"`
4. ⏳ Update remaining 17 AI components

## 📞 Support

- **Issues:** GitHub Issues
- **Documentation:** docs/AZURE_OPENAI_TESTING_GUIDE.md
- **Examples:** examples/AI_Example_Component.razor
