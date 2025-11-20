# Simple ML Training Script for Nalam360 Platform
# Generates sample data and trains ML.NET models

Write-Host "=== Nalam360 ML Model Training ===" -ForegroundColor Cyan
Write-Host ""

# Generate sample readmission data
Write-Host "Generating training data..." -ForegroundColor Yellow

$random = New-Object System.Random
$content = "PatientId,Age,Gender,DiagnosisCode,PreviousAdmissions,LengthOfStay,HasComorbidities,Readmitted"

for ($i = 1; $i -le 1000; $i++) {
    $age = $random.Next(25, 95)
    $gender = if ($random.Next(2) -eq 0) { "M" } else { "F" }
    $diagnosis = @("I50.9","J44.0","I21.0","E11.9")[$random.Next(4)]
    $prevAdm = $random.Next(0, 6)
    $los = $random.Next(1, 15)
    $comorbid = if ($random.Next(2) -eq 0) { "true" } else { "false" }
    
    $risk = 0.0
    if ($age -gt 70) { $risk += 0.3 }
    if ($prevAdm -gt 2) { $risk += 0.3 }
    if ($los -gt 5) { $risk += 0.2 }
    if ($comorbid -eq "true") { $risk += 0.2 }
    
    $readmit = if ($random.NextDouble() -lt $risk) { "true" } else { "false" }
    $content += "`nP{0:D4},$age,$gender,$diagnosis,$prevAdm,$los,$comorbid,$readmit" -f $i
}

New-Item -ItemType Directory -Path "ML/Data" -Force | Out-Null
Set-Content -Path "ML/Data/readmission-training.csv" -Value $content
Write-Host "  Created ML/Data/readmission-training.csv (1000 records)" -ForegroundColor Green

# Generate LOS data
$content = "PatientId,Age,AdmissionType,DiagnosisCode,SeverityScore,ActualLOS"
for ($i = 1; $i -le 1000; $i++) {
    $age = $random.Next(25, 95)
    $admType = @("Emergency","Elective","Urgent")[$random.Next(3)]
    $diagnosis = @("I21.0","I63.9","I50.1","K80.2")[$random.Next(4)]
    $severity = $random.Next(20, 100)
    
    $los = 3.0
    if ($admType -eq "Emergency") { $los += 2.0 }
    if ($admType -eq "Urgent") { $los += 1.0 }
    $los += ($severity / 100) * 5.0
    if ($age -gt 70) { $los += 2.0 }
    $los = [Math]::Max(1.0, [Math]::Round($los + ($random.NextDouble() * 2 - 1), 1))
    
    $content += "`nP{0:D4},$age,$admType,$diagnosis,$severity,$los" -f $i
}

Set-Content -Path "ML/Data/los-training.csv" -Value $content
Write-Host "  Created ML/Data/los-training.csv (1000 records)" -ForegroundColor Green

# Generate mortality data
$content = "PatientId,Age,VitalSignsScore,LabResultsAbnormal,ICUAdmission,Deceased"
for ($i = 1; $i -le 1000; $i++) {
    $age = $random.Next(25, 95)
    $vitals = $random.Next(50, 100)
    $labAbn = if ($random.Next(2) -eq 0) { "true" } else { "false" }
    $icu = if ($random.Next(4) -eq 0) { "true" } else { "false" }
    
    $risk = 0.05
    if ($age -gt 80) { $risk += 0.15 }
    if ($vitals -gt 85) { $risk += 0.20 }
    if ($labAbn -eq "true") { $risk += 0.15 }
    if ($icu -eq "true") { $risk += 0.25 }
    
    $deceased = if ($random.NextDouble() -lt $risk) { "true" } else { "false" }
    $content += "`nP{0:D4},$age,$vitals,$labAbn,$icu,$deceased" -f $i
}

Set-Content -Path "ML/Data/mortality-training.csv" -Value $content
Write-Host "  Created ML/Data/mortality-training.csv (1000 records)" -ForegroundColor Green

Write-Host ""
Write-Host "Data generation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To train models, install ML.NET CLI:" -ForegroundColor Cyan
Write-Host "  dotnet tool install -g mlnet" -ForegroundColor Gray
Write-Host ""
Write-Host "Then train each model:" -ForegroundColor Cyan
Write-Host "  mlnet classification --dataset ML/Data/readmission-training.csv --label-col Readmitted --train-time 120 --output ML/Models --name readmission-risk" -ForegroundColor Gray
Write-Host "  mlnet regression --dataset ML/Data/los-training.csv --label-col ActualLOS --train-time 120 --output ML/Models --name length-of-stay" -ForegroundColor Gray
Write-Host "  mlnet classification --dataset ML/Data/mortality-training.csv --label-col Deceased --train-time 120 --output ML/Models --name mortality-risk" -ForegroundColor Gray
Write-Host ""
