# ML.NET Model Training Script
# Trains all 4 healthcare ML models for the Nalam360 Enterprise Platform

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("readmission-risk", "length-of-stay", "mortality-risk", "anomaly-detection", "all")]
    [string]$Model = "all",
    
    [Parameter(Mandatory=$false)]
    [int]$TrainTimeSeconds = 600,
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateSampleData,
    
    [Parameter(Mandatory=$false)]
    [switch]$TestAfterTraining
)

$ErrorActionPreference = "Stop"

Write-Host "=== Nalam360 ML.NET Model Training ===" -ForegroundColor Cyan
Write-Host ""

# Check if ML.NET CLI is installed
try {
    $mlnetVersion = mlnet --version 2>&1
    Write-Host "✓ ML.NET CLI installed: $mlnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ML.NET CLI not installed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Installing ML.NET CLI..." -ForegroundColor Yellow
    dotnet tool install -g mlnet
    Write-Host "✓ ML.NET CLI installed successfully" -ForegroundColor Green
}

Write-Host ""

# Generate sample data if requested
if ($GenerateSampleData) {
    Write-Host "Generating sample training data..." -ForegroundColor Yellow
    
    # Readmission Risk Sample Data (1000 records)
    Write-Host "  Creating readmission-risk sample data (1000 records)..." -ForegroundColor Gray
    $readmissionData = @"
PatientId,Age,Gender,DiagnosisCode,PreviousAdmissions,LengthOfStay,HasComorbidities,Readmitted
P001,65,M,I50.9,2,5,true,true
P002,42,F,J44.0,0,3,false,false
P003,78,M,I21.0,3,7,true,true
P004,55,F,E11.9,1,4,true,false
P005,68,M,I50.1,2,6,true,true
P006,35,F,O80,0,2,false,false
P007,82,M,I63.9,4,8,true,true
P008,45,M,K80.2,0,3,false,false
P009,71,F,I25.10,2,5,true,true
P010,39,F,N18.3,1,4,true,false
"@
    
    # Generate 1000 synthetic records
    $random = New-Object System.Random
    for ($i = 11; $i -le 1000; $i++) {
        $patientId = "P{0:D4}" -f $i
        $age = $random.Next(25, 95)
        $gender = if ($random.Next(2) -eq 0) { "M" } else { "F" }
        $diagnosisCodes = @("I50.9", "J44.0", "I21.0", "E11.9", "I63.9", "K80.2", "N18.3", "I25.10")
        $diagnosisCode = $diagnosisCodes[$random.Next($diagnosisCodes.Length)]
        $previousAdmissions = $random.Next(0, 6)
        $lengthOfStay = $random.Next(1, 15)
        $hasComorbidities = if ($random.Next(2) -eq 0) { "true" } else { "false" }
        
        # Readmission logic: higher risk with age>70, previous admissions>2, LOS>5
        $readmissionRisk = 0
        if ($age -gt 70) { $readmissionRisk += 0.3 }
        if ($previousAdmissions -gt 2) { $readmissionRisk += 0.3 }
        if ($lengthOfStay -gt 5) { $readmissionRisk += 0.2 }
        if ($hasComorbidities -eq "true") { $readmissionRisk += 0.2 }
        $readmitted = if (($random.NextDouble()) -lt $readmissionRisk) { "true" } else { "false" }
        
        $readmissionData += "`n$patientId,$age,$gender,$diagnosisCode,$previousAdmissions,$lengthOfStay,$hasComorbidities,$readmitted"
    }
    
    Set-Content -Path "ML/Data/readmission-risk-training.csv" -Value $readmissionData
    Write-Host "    ✓ Created ML/Data/readmission-risk-training.csv" -ForegroundColor Green
    
    # Length of Stay Sample Data (1000 records)
    Write-Host "  Creating length-of-stay sample data (1000 records)..." -ForegroundColor Gray
    $losData = @"
PatientId,Age,AdmissionType,DiagnosisCode,SeverityScore,ActualLOS
P001,65,Emergency,I21.0,85,7.5
P002,42,Elective,M17.11,45,2.0
P003,78,Emergency,I63.9,92,12.3
P004,55,Urgent,K80.2,68,4.5
P005,68,Emergency,I50.1,88,9.2
P006,35,Elective,O80,30,1.8
P007,82,Emergency,I50.9,95,14.7
P008,45,Elective,K40.90,40,2.5
P009,71,Urgent,E11.9,75,5.8
P010,39,Elective,M51.26,50,3.2
"@
    
    for ($i = 11; $i -le 1000; $i++) {
        $patientId = "P{0:D4}" -f $i
        $age = $random.Next(25, 95)
        $admissionTypes = @("Emergency", "Elective", "Urgent")
        $admissionType = $admissionTypes[$random.Next($admissionTypes.Length)]
        $diagnosisCodes = @("I21.0", "I63.9", "I50.1", "I50.9", "K80.2", "E11.9", "M17.11", "K40.90", "M51.26", "O80")
        $diagnosisCode = $diagnosisCodes[$random.Next($diagnosisCodes.Length)]
        $severityScore = $random.Next(20, 100)
        
        # LOS calculation based on severity, age, admission type
        $baseLOS = 3.0
        if ($admissionType -eq "Emergency") { $baseLOS += 2.0 }
        if ($admissionType -eq "Urgent") { $baseLOS += 1.0 }
        $baseLOS += ($severityScore / 100) * 5.0
        if ($age -gt 70) { $baseLOS += 2.0 }
        $actualLOS = [Math]::Round($baseLOS + ($random.NextDouble() * 2 - 1), 1)
        if ($actualLOS -lt 1.0) { $actualLOS = 1.0 }
        
        $losData += "`n$patientId,$age,$admissionType,$diagnosisCode,$severityScore,$actualLOS"
    }
    
    Set-Content -Path "ML/Data/length-of-stay-training.csv" -Value $losData
    Write-Host "    ✓ Created ML/Data/length-of-stay-training.csv" -ForegroundColor Green
    
    # Mortality Risk Sample Data (1000 records)
    Write-Host "  Creating mortality-risk sample data (1000 records)..." -ForegroundColor Gray
    $mortalityData = @"
PatientId,Age,VitalSignsScore,LabResultsAbnormal,ICUAdmission,Deceased
P001,78,82,true,true,true
P002,55,65,false,false,false
P003,85,88,true,true,true
P004,42,70,false,false,false
P005,91,92,true,true,true
P006,38,68,false,false,false
P007,76,85,true,true,false
P008,45,72,false,false,false
P009,89,90,true,true,true
P010,50,75,false,false,false
"@
    
    for ($i = 11; $i -le 1000; $i++) {
        $patientId = "P{0:D4}" -f $i
        $age = $random.Next(25, 95)
        $vitalSignsScore = $random.Next(50, 100)
        $labResultsAbnormal = if ($random.Next(2) -eq 0) { "true" } else { "false" }
        $icuAdmission = if ($random.Next(3) -eq 0) { "true" } else { "false" }
        
        # Mortality risk: high with age>80, vital score>85, abnormal labs, ICU
        $mortalityRisk = 0.05
        if ($age -gt 80) { $mortalityRisk += 0.15 }
        if ($vitalSignsScore -gt 85) { $mortalityRisk += 0.20 }
        if ($labResultsAbnormal -eq "true") { $mortalityRisk += 0.15 }
        if ($icuAdmission -eq "true") { $mortalityRisk += 0.25 }
        $deceased = if (($random.NextDouble()) -lt $mortalityRisk) { "true" } else { "false" }
        
        $mortalityData += "`n$patientId,$age,$vitalSignsScore,$labResultsAbnormal,$icuAdmission,$deceased"
    }
    
    Set-Content -Path "ML/Data/mortality-risk-training.csv" -Value $mortalityData
    Write-Host "    ✓ Created ML/Data/mortality-risk-training.csv" -ForegroundColor Green
    
    # Anomaly Detection Sample Data (1000 records)
    Write-Host "  Creating anomaly-detection sample data (1000 records)..." -ForegroundColor Gray
    $anomalyData = @"
Timestamp,Value,IsAnomaly
2025-01-01T00:00:00,98.6,false
2025-01-01T00:15:00,98.7,false
2025-01-01T00:30:00,103.5,true
2025-01-01T00:45:00,98.5,false
2025-01-01T01:00:00,98.8,false
2025-01-01T01:15:00,98.6,false
2025-01-01T01:30:00,105.2,true
2025-01-01T01:45:00,98.4,false
2025-01-01T02:00:00,98.7,false
2025-01-01T02:15:00,98.9,false
"@
    
    $baseValue = 98.6
    $timestamp = Get-Date "2025-01-01T02:30:00"
    for ($i = 11; $i -le 1000; $i++) {
        $timestamp = $timestamp.AddMinutes(15)
        $value = $baseValue + ($random.NextDouble() * 1.0 - 0.5)  # Normal variation ±0.5
        $isAnomaly = "false"
        
        # Inject anomalies (5% of data)
        if ($random.NextDouble() -lt 0.05) {
            $value = $baseValue + ($random.Next(4, 8))  # Spike
            $isAnomaly = "true"
        }
        
        $value = [Math]::Round($value, 1)
        $anomalyData += "`n$($timestamp.ToString('yyyy-MM-ddTHH:mm:ss')),$value,$isAnomaly"
    }
    
    Set-Content -Path "ML/Data/anomaly-detection-training.csv" -Value $anomalyData
    Write-Host "    ✓ Created ML/Data/anomaly-detection-training.csv" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "✓ Sample training data generated successfully" -ForegroundColor Green
    Write-Host ""
}

# Training function
function Invoke-ModelTraining {
    param(
        [string]$ModelName,
        [string]$TaskType,
        [string]$Dataset,
        [string]$LabelColumn,
        [int]$TrainTime
    )
    
    Write-Host "Training $ModelName model..." -ForegroundColor Cyan
    Write-Host "  Task Type: $TaskType" -ForegroundColor Gray
    Write-Host "  Dataset: $Dataset" -ForegroundColor Gray
    Write-Host "  Label Column: $LabelColumn" -ForegroundColor Gray
    Write-Host "  Training Time: $TrainTime seconds" -ForegroundColor Gray
    Write-Host ""
    
    $startTime = Get-Date
    
    try {
        $output = mlnet $TaskType `
            --dataset $Dataset `
            --label-col $LabelColumn `
            --train-time $TrainTime `
            --output "ML/Models" `
            --name $ModelName `
            2>&1
        
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalSeconds
        
        Write-Host ""
        Write-Host "✓ $ModelName training completed in $([Math]::Round($duration, 1))s" -ForegroundColor Green
        
        # Extract metrics from output
        if ($output -match "Accuracy:\s+([\d.]+)") {
            Write-Host "  Accuracy: $($Matches[1])" -ForegroundColor Cyan
        }
        if ($output -match "AUC:\s+([\d.]+)") {
            Write-Host "  AUC: $($Matches[1])" -ForegroundColor Cyan
        }
        if ($output -match "F1 Score:\s+([\d.]+)") {
            Write-Host "  F1 Score: $($Matches[1])" -ForegroundColor Cyan
        }
        if ($output -match "R-squared:\s+([\d.]+)") {
            Write-Host "  R-squared: $($Matches[1])" -ForegroundColor Cyan
        }
        if ($output -match "RMSE:\s+([\d.]+)") {
            Write-Host "  RMSE: $($Matches[1])" -ForegroundColor Cyan
        }
        
        Write-Host ""
        return $true
    } catch {
        Write-Host ""
        Write-Host "✗ $ModelName training failed: $_" -ForegroundColor Red
        Write-Host ""
        return $false
    }
}

# Train models based on selection
$successCount = 0
$failCount = 0

if ($Model -eq "all" -or $Model -eq "readmission-risk") {
    if (Test-Path "ML/Data/readmission-risk-training.csv") {
        if (Invoke-ModelTraining -ModelName "readmission-risk" -TaskType "classification" -Dataset "ML/Data/readmission-risk-training.csv" -LabelColumn "Readmitted" -TrainTime $TrainTimeSeconds) {
            $successCount++
        } else {
            $failCount++
        }
    } else {
        Write-Host "✗ Training data not found: ML/Data/readmission-risk-training.csv" -ForegroundColor Red
        Write-Host "  Run with -GenerateSampleData flag to create sample data" -ForegroundColor Yellow
        Write-Host ""
        $failCount++
    }
}

if ($Model -eq "all" -or $Model -eq "length-of-stay") {
    if (Test-Path "ML/Data/length-of-stay-training.csv") {
        if (Invoke-ModelTraining -ModelName "length-of-stay" -TaskType "regression" -Dataset "ML/Data/length-of-stay-training.csv" -LabelColumn "ActualLOS" -TrainTime $TrainTimeSeconds) {
            $successCount++
        } else {
            $failCount++
        }
    } else {
        Write-Host "✗ Training data not found: ML/Data/length-of-stay-training.csv" -ForegroundColor Red
        Write-Host "  Run with -GenerateSampleData flag to create sample data" -ForegroundColor Yellow
        Write-Host ""
        $failCount++
    }
}

if ($Model -eq "all" -or $Model -eq "mortality-risk") {
    if (Test-Path "ML/Data/mortality-risk-training.csv") {
        if (Invoke-ModelTraining -ModelName "mortality-risk" -TaskType "classification" -Dataset "ML/Data/mortality-risk-training.csv" -LabelColumn "Deceased" -TrainTime $TrainTimeSeconds) {
            $successCount++
        } else {
            $failCount++
        }
    } else {
        Write-Host "✗ Training data not found: ML/Data/mortality-risk-training.csv" -ForegroundColor Red
        Write-Host "  Run with -GenerateSampleData flag to create sample data" -ForegroundColor Yellow
        Write-Host ""
        $failCount++
    }
}

if ($Model -eq "all" -or $Model -eq "anomaly-detection") {
    Write-Host "⚠ Anomaly detection requires custom training code (not supported by ML.NET CLI)" -ForegroundColor Yellow
    Write-Host "  See ML/Training/train-anomaly-detection.cs for implementation" -ForegroundColor Gray
    Write-Host ""
}

# Deploy models to application
Write-Host "Deploying trained models to application..." -ForegroundColor Cyan
$targetPath = "src/Nalam360Enterprise.UI/Nalam360Enterprise.UI/wwwroot/ML/Models"
if (-not (Test-Path $targetPath)) {
    New-Item -ItemType Directory -Force -Path $targetPath | Out-Null
}

$modelFiles = Get-ChildItem "ML/Models/*.zip" -ErrorAction SilentlyContinue
if ($modelFiles) {
    foreach ($file in $modelFiles) {
        Copy-Item $file.FullName -Destination $targetPath -Force
        Write-Host "  ✓ Deployed $($file.Name)" -ForegroundColor Green
    }
} else {
    Write-Host "  ⚠ No model files found to deploy" -ForegroundColor Yellow
}

Write-Host ""

# Test models if requested
if ($TestAfterTraining) {
    Write-Host "Testing trained models..." -ForegroundColor Cyan
    Write-Host ""
    
    # TODO: Add model testing logic here
    # dotnet run --project ML/Testing/ModelTests.csproj
    
    Write-Host "Model testing not yet implemented" -ForegroundColor Yellow
    Write-Host ""
}

# Summary
Write-Host "=== Training Summary ===" -ForegroundColor Cyan
Write-Host "Models trained successfully: $successCount" -ForegroundColor Green
if ($failCount -gt 0) {
    Write-Host "Models failed: $failCount" -ForegroundColor Red
}
Write-Host ""

if ($successCount -gt 0) {
    Write-Host "✓ Models are ready for use in the application!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Test predictions: dotnet run --project examples/Nalam360.Platform.Example.Api" -ForegroundColor Gray
    Write-Host "  2. Monitor accuracy: Set up Application Insights" -ForegroundColor Gray
    Write-Host "  3. Retrain monthly: Schedule automated retraining pipeline" -ForegroundColor Gray
}

Write-Host ""
