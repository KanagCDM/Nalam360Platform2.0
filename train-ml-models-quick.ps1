# Quick ML Model Training Script - Generate Sample Data and Train Models
# This script generates synthetic healthcare data and trains ML.NET models

Write-Host "=== Nalam360 ML.NET Model Training ===" -ForegroundColor Cyan
Write-Host ""

# Check if ML.NET CLI is installed
Write-Host "Checking ML.NET CLI..." -ForegroundColor Yellow
try {
    $null = mlnet --version 2>&1
    Write-Host "✓ ML.NET CLI is installed" -ForegroundColor Green
} catch {
    Write-Host "✗ ML.NET CLI not found - Installing..." -ForegroundColor Yellow
    dotnet tool install -g mlnet --verbosity quiet
    Write-Host "✓ ML.NET CLI installed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Generating sample training data..." -ForegroundColor Cyan

# Create Data directory if not exists
if (-not (Test-Path "ML/Data")) {
    New-Item -ItemType Directory -Path "ML/Data" -Force | Out-Null
}

# Generate Readmission Risk data
Write-Host "  Creating readmission-risk-training.csv..." -ForegroundColor Gray
$csvContent = "PatientId,Age,Gender,DiagnosisCode,PreviousAdmissions,LengthOfStay,HasComorbidities,Readmitted"
$random = New-Object System.Random

for ($i = 1; $i -le 1000; $i++) {
    $patientId = "P{0:D4}" -f $i
    $age = $random.Next(25, 95)
    $gender = @("M", "F")[$random.Next(2)]
    $diagnosisCodes = @("I50.9", "J44.0", "I21.0", "E11.9", "I63.9", "K80.2", "N18.3", "I25.10")
    $diagnosisCode = $diagnosisCodes[$random.Next($diagnosisCodes.Length)]
    $previousAdmissions = $random.Next(0, 6)
    $lengthOfStay = $random.Next(1, 15)
    $hasComorbidities = @("true", "false")[$random.Next(2)]
    
    # Calculate readmission risk
    $risk = 0.0
    if ($age -gt 70) { $risk += 0.3 }
    if ($previousAdmissions -gt 2) { $risk += 0.3 }
    if ($lengthOfStay -gt 5) { $risk += 0.2 }
    if ($hasComorbidities -eq "true") { $risk += 0.2 }
    $readmitted = if ($random.NextDouble() -lt $risk) { "true" } else { "false" }
    
    $csvContent += "`n$patientId,$age,$gender,$diagnosisCode,$previousAdmissions,$lengthOfStay,$hasComorbidities,$readmitted"
}

Set-Content -Path "ML/Data/readmission-risk-training.csv" -Value $csvContent
Write-Host "    ✓ 1000 records created" -ForegroundColor Green

# Generate Length of Stay data
Write-Host "  Creating length-of-stay-training.csv..." -ForegroundColor Gray
$csvContent = "PatientId,Age,AdmissionType,DiagnosisCode,SeverityScore,ActualLOS"

for ($i = 1; $i -le 1000; $i++) {
    $patientId = "P{0:D4}" -f $i
    $age = $random.Next(25, 95)
    $admissionType = @("Emergency", "Elective", "Urgent")[$random.Next(3)]
    $diagnosisCodes = @("I21.0", "I63.9", "I50.1", "K80.2", "E11.9", "M17.11")
    $diagnosisCode = $diagnosisCodes[$random.Next($diagnosisCodes.Length)]
    $severityScore = $random.Next(20, 100)
    
    # Calculate LOS
    $baseLOS = 3.0
    if ($admissionType -eq "Emergency") { $baseLOS += 2.0 }
    if ($admissionType -eq "Urgent") { $baseLOS += 1.0 }
    $baseLOS += ($severityScore / 100) * 5.0
    if ($age -gt 70) { $baseLOS += 2.0 }
    $actualLOS = [Math]::Max(1.0, [Math]::Round($baseLOS + ($random.NextDouble() * 2 - 1), 1))
    
    $csvContent += "`n$patientId,$age,$admissionType,$diagnosisCode,$severityScore,$actualLOS"
}

Set-Content -Path "ML/Data/length-of-stay-training.csv" -Value $csvContent
Write-Host "    ✓ 1000 records created" -ForegroundColor Green

# Generate Mortality Risk data
Write-Host "  Creating mortality-risk-training.csv..." -ForegroundColor Gray
$csvContent = "PatientId,Age,VitalSignsScore,LabResultsAbnormal,ICUAdmission,Deceased"

for ($i = 1; $i -le 1000; $i++) {
    $patientId = "P{0:D4}" -f $i
    $age = $random.Next(25, 95)
    $vitalSignsScore = $random.Next(50, 100)
    $labResultsAbnormal = @("true", "false")[$random.Next(2)]
    $icuAdmission = @("true", "false")[$random.Next(4)]  # 25% ICU admission rate
    
    # Calculate mortality risk
    $risk = 0.05
    if ($age -gt 80) { $risk += 0.15 }
    if ($vitalSignsScore -gt 85) { $risk += 0.20 }
    if ($labResultsAbnormal -eq "true") { $risk += 0.15 }
    if ($icuAdmission -eq "true") { $risk += 0.25 }
    $deceased = if ($random.NextDouble() -lt $risk) { "true" } else { "false" }
    
    $csvContent += "`n$patientId,$age,$vitalSignsScore,$labResultsAbnormal,$icuAdmission,$deceased"
}

Set-Content -Path "ML/Data/mortality-risk-training.csv" -Value $csvContent
Write-Host "    ✓ 1000 records created" -ForegroundColor Green

Write-Host ""
Write-Host "✓ Sample data generation complete" -ForegroundColor Green
Write-Host ""

# Train models
Write-Host "Training ML models (120 seconds each)..." -ForegroundColor Cyan
Write-Host ""

$successCount = 0

# Train Readmission Risk model
Write-Host "Training readmission-risk model..." -ForegroundColor Yellow
try {
    mlnet classification `
        --dataset "ML/Data/readmission-risk-training.csv" `
        --label-col "Readmitted" `
        --train-time 120 `
        --output "ML/Models" `
        --name "readmission-risk" `
        --verbosity quiet
    
    Write-Host "✓ Readmission risk model trained" -ForegroundColor Green
    $successCount++
} catch {
    Write-Host "✗ Readmission risk training failed: $_" -ForegroundColor Red
}

Write-Host ""

# Train Length of Stay model
Write-Host "Training length-of-stay model..." -ForegroundColor Yellow
try {
    mlnet regression `
        --dataset "ML/Data/length-of-stay-training.csv" `
        --label-col "ActualLOS" `
        --train-time 120 `
        --output "ML/Models" `
        --name "length-of-stay" `
        --verbosity quiet
    
    Write-Host "✓ Length of stay model trained" -ForegroundColor Green
    $successCount++
} catch {
    Write-Host "✗ Length of stay training failed: $_" -ForegroundColor Red
}

Write-Host ""

# Train Mortality Risk model
Write-Host "Training mortality-risk model..." -ForegroundColor Yellow
try {
    mlnet classification `
        --dataset "ML/Data/mortality-risk-training.csv" `
        --label-col "Deceased" `
        --train-time 120 `
        --output "ML/Models" `
        --name "mortality-risk" `
        --verbosity quiet
    
    Write-Host "✓ Mortality risk model trained" -ForegroundColor Green
    $successCount++
} catch {
    Write-Host "✗ Mortality risk training failed: $_" -ForegroundColor Red
}

Write-Host ""

# Deploy models
Write-Host "Deploying models to application..." -ForegroundColor Cyan
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
    Write-Host "  ⚠ No model files found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Training Complete ===" -ForegroundColor Cyan
Write-Host "Models trained: $successCount/3" -ForegroundColor Green
Write-Host ""

if ($successCount -eq 3) {
    Write-Host "✓ All models ready for use!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Build the application: dotnet build" -ForegroundColor Gray
    Write-Host "  2. Run the API: dotnet run --project examples/Nalam360.Platform.Example.Api" -ForegroundColor Gray
    Write-Host "  3. Test predictions with sample data" -ForegroundColor Gray
} else {
    Write-Host "⚠ Some models failed to train - check errors above" -ForegroundColor Yellow
}

Write-Host ""
