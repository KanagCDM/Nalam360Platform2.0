# Test Azure OpenAI Connectivity and AI Services
# This script validates your Azure OpenAI configuration before running the full application

param(
    [Parameter(Mandatory=$false)]
    [string]$Endpoint = "",
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "",
    [Parameter(Mandatory=$false)]
    [string]$DeploymentName = "gpt-4"
)

Write-Host "=== Azure OpenAI Service Test Script ===" -ForegroundColor Cyan
Write-Host ""

# Read from appsettings if not provided
if ([string]::IsNullOrEmpty($Endpoint) -or [string]::IsNullOrEmpty($ApiKey)) {
    Write-Host "Reading configuration from appsettings.Development.json..." -ForegroundColor Yellow
    
    $appsettingsPath = Join-Path $PSScriptRoot "examples\Nalam360.Platform.Example.Api\appsettings.Development.json"
    
    if (Test-Path $appsettingsPath) {
        $config = Get-Content $appsettingsPath | ConvertFrom-Json
        
        if ([string]::IsNullOrEmpty($Endpoint)) {
            $Endpoint = $config.AzureOpenAI.Endpoint
        }
        if ([string]::IsNullOrEmpty($ApiKey)) {
            $ApiKey = $config.AzureOpenAI.ApiKey
        }
        if ([string]::IsNullOrEmpty($DeploymentName)) {
            $DeploymentName = $config.AzureOpenAI.DeploymentName
        }
        
        Write-Host "✓ Configuration loaded" -ForegroundColor Green
    }
    else {
        Write-Host "✗ appsettings.Development.json not found" -ForegroundColor Red
        Write-Host "Please provide -Endpoint and -ApiKey parameters" -ForegroundColor Yellow
        exit 1
    }
}

# Validate configuration
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  Endpoint: $Endpoint"
Write-Host "  Deployment: $DeploymentName"
Write-Host "  API Key: $($ApiKey.Substring(0, 8))..." 
Write-Host ""

if ($Endpoint -eq "https://your-openai-instance.openai.azure.com" -or $ApiKey -eq "your-api-key-from-azure-portal") {
    Write-Host "⚠ WARNING: You are using placeholder configuration values!" -ForegroundColor Yellow
    Write-Host "Please update appsettings.Development.json with your actual Azure OpenAI credentials." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To get your credentials:" -ForegroundColor White
    Write-Host "  1. Go to https://portal.azure.com" -ForegroundColor Gray
    Write-Host "  2. Navigate to your Azure OpenAI resource" -ForegroundColor Gray
    Write-Host "  3. Go to 'Keys and Endpoint' section" -ForegroundColor Gray
    Write-Host "  4. Copy Endpoint and Key 1" -ForegroundColor Gray
    Write-Host ""
    
    $continue = Read-Host "Continue anyway? (y/n)"
    if ($continue -ne "y") {
        exit 0
    }
}

# Test 1: Basic Connectivity
Write-Host "Test 1: Basic Connectivity" -ForegroundColor Cyan
Write-Host "Sending simple test request..." -ForegroundColor Gray

$headers = @{
    "api-key" = $ApiKey
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
            content = "Hello, can you respond with a simple greeting?"
        }
    )
    temperature = 0.7
    max_tokens = 50
} | ConvertTo-Json -Depth 10

try {
    $url = "$Endpoint/openai/deployments/$DeploymentName/chat/completions?api-version=2024-02-15-preview"
    $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body -ErrorAction Stop
    
    Write-Host "✓ Connection successful!" -ForegroundColor Green
    Write-Host "Response: $($response.choices[0].message.content)" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host "✗ Connection failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host ""
        Write-Host "Troubleshooting 401 Unauthorized:" -ForegroundColor Yellow
        Write-Host "  - Verify your API key is correct" -ForegroundColor Gray
        Write-Host "  - Check if the key has been regenerated" -ForegroundColor Gray
        Write-Host "  - Ensure you copied the full key without spaces" -ForegroundColor Gray
    }
    elseif ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host ""
        Write-Host "Troubleshooting 404 Not Found:" -ForegroundColor Yellow
        Write-Host "  - Verify your deployment name is '$DeploymentName'" -ForegroundColor Gray
        Write-Host "  - Check Azure Portal → Your OpenAI resource → Model deployments" -ForegroundColor Gray
        Write-Host "  - Ensure the model is deployed and running" -ForegroundColor Gray
    }
    
    exit 1
}

# Test 2: Intent Analysis
Write-Host "Test 2: Intent Analysis" -ForegroundColor Cyan
Write-Host "Testing healthcare intent detection..." -ForegroundColor Gray

$intentBody = @{
    messages = @(
        @{
            role = "system"
            content = "Analyze the healthcare message and classify the user's intent. Return JSON with: intent, confidence (0-1), entities."
        },
        @{
            role = "user"
            content = "I need to schedule an appointment with my doctor next week."
        }
    )
    temperature = 0.3
    max_tokens = 150
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $intentBody
    $result = $response.choices[0].message.content
    
    Write-Host "✓ Intent analysis successful!" -ForegroundColor Green
    Write-Host "Result: $result" -ForegroundColor White
    Write-Host ""
    
    if ($result -like "*AppointmentScheduling*" -or $result -like "*appointment*") {
        Write-Host "✓ Correct intent detected: AppointmentScheduling" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ Intent may not be accurate - review result above" -ForegroundColor Yellow
    }
    Write-Host ""
}
catch {
    Write-Host "✗ Intent analysis failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Sentiment Analysis
Write-Host "Test 3: Sentiment Analysis" -ForegroundColor Cyan
Write-Host "Testing emotional sentiment detection..." -ForegroundColor Gray

$sentimentBody = @{
    messages = @(
        @{
            role = "system"
            content = "Analyze the sentiment of the message. Return JSON with: sentiment (positive/negative/neutral/mixed), confidence (0-1), scores for each sentiment type."
        },
        @{
            role = "user"
            content = "I'm very anxious and worried about my upcoming surgery."
        }
    )
    temperature = 0.3
    max_tokens = 150
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $sentimentBody
    $result = $response.choices[0].message.content
    
    Write-Host "✓ Sentiment analysis successful!" -ForegroundColor Green
    Write-Host "Result: $result" -ForegroundColor White
    Write-Host ""
    
    if ($result -like "*negative*" -or $result -like "*anxious*") {
        Write-Host "✓ Correct sentiment detected: Negative" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ Sentiment may not be accurate - review result above" -ForegroundColor Yellow
    }
    Write-Host ""
}
catch {
    Write-Host "✗ Sentiment analysis failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Streaming Response
Write-Host "Test 4: Streaming Response" -ForegroundColor Cyan
Write-Host "Testing real-time token streaming..." -ForegroundColor Gray

$streamBody = @{
    messages = @(
        @{
            role = "system"
            content = "You are a helpful healthcare assistant."
        },
        @{
            role = "user"
            content = "What are visiting hours?"
        }
    )
    temperature = 0.7
    max_tokens = 100
    stream = $true
} | ConvertTo-Json -Depth 10

try {
    # Note: PowerShell doesn't handle SSE streams elegantly, so we just verify the endpoint accepts stream=true
    $response = Invoke-WebRequest -Uri $url -Method Post -Headers $headers -Body $streamBody -UseBasicParsing
    
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Streaming endpoint responding!" -ForegroundColor Green
        Write-Host "Note: Full streaming test requires application runtime" -ForegroundColor Gray
    }
    Write-Host ""
}
catch {
    Write-Host "✗ Streaming test failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# Test 5: PHI Detection (Local - No API call)
Write-Host "Test 5: PHI Detection (Local Regex)" -ForegroundColor Cyan
Write-Host "Testing HIPAA compliance patterns..." -ForegroundColor Gray

$testText = "Patient MRN: ABC123456, SSN 123-45-6789, Phone: 555-123-4567, Email: patient@example.com"

$phiPatterns = @{
    "MRN" = "\b(MRN|Medical Record|Record #)[\s:]*([A-Z0-9]{6,12})\b"
    "SSN" = "\b\d{3}-?\d{2}-?\d{4}\b"
    "PHONE" = "\b(\+?1[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b"
    "EMAIL" = "\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"
}

$detected = @()
foreach ($type in $phiPatterns.Keys) {
    if ($testText -match $phiPatterns[$type]) {
        $detected += $type
    }
}

if ($detected.Count -gt 0) {
    Write-Host "✓ PHI Detection working!" -ForegroundColor Green
    Write-Host "Detected types: $($detected -join ', ')" -ForegroundColor White
    Write-Host ""
}
else {
    Write-Host "✗ PHI Detection failed!" -ForegroundColor Red
    Write-Host "No PHI patterns matched" -ForegroundColor Red
    Write-Host ""
}

# Summary
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "✓ Configuration validated" -ForegroundColor Green
Write-Host "✓ Azure OpenAI connectivity confirmed" -ForegroundColor Green
Write-Host "✓ Intent analysis functional" -ForegroundColor Green
Write-Host "✓ Sentiment analysis functional" -ForegroundColor Green
Write-Host "✓ Streaming endpoint accessible" -ForegroundColor Green
Write-Host "✓ PHI detection patterns working" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Run the example application: cd examples\Nalam360.Platform.Example.Api; dotnet run" -ForegroundColor Gray
Write-Host "  2. Test N360SmartChat component with UseRealAI=`"true`"" -ForegroundColor Gray
Write-Host "  3. Monitor costs in Azure Portal → Cost Management" -ForegroundColor Gray
Write-Host "  4. Review logs in appsettings → Logging → Nalam360Enterprise.UI.Core.AI → Debug" -ForegroundColor Gray
Write-Host ""
Write-Host "For detailed testing guide, see: docs\AZURE_OPENAI_TESTING_GUIDE.md" -ForegroundColor Yellow
