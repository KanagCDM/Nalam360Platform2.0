#!/usr/bin/env pwsh
# MedWayHealthCare 2.0 - Project Structure Generation Script
# This script creates the complete project structure for the Hospital Management System

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  MedWayHealthCare 2.0 Structure Generator" -ForegroundColor Cyan
Write-Host "  Building Complete Hospital Management System" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

$baseDir = $PSScriptRoot
$srcDir = Join-Path $baseDir "src"
$testsDir = Join-Path $baseDir "tests"

# Navigate to base directory
Set-Location $baseDir

Write-Host "📁 Creating directory structure..." -ForegroundColor Yellow

# Core Projects
Write-Host "`n🔷 Creating CORE projects..." -ForegroundColor Green
dotnet new classlib -n MedWay.Domain -o "$srcDir/Core/MedWay.Domain" -f net10.0
dotnet new classlib -n MedWay.Application -o "$srcDir/Core/MedWay.Application" -f net10.0
dotnet new classlib -n MedWay.Contracts -o "$srcDir/Core/MedWay.Contracts" -f net10.0

# Module Projects
Write-Host "`n🏥 Creating MODULE projects..." -ForegroundColor Green

$modules = @(
    "PatientManagement",
    "ClinicalManagement",
    "AppointmentManagement",
    "EmergencyManagement",
    "PharmacyManagement",
    "LaboratoryManagement",
    "RadiologyManagement",
    "BillingManagement",
    "InventoryManagement",
    "HumanResources",
    "OperatingRoomManagement",
    "ReportingAnalytics"
)

foreach ($module in $modules) {
    Write-Host "  Creating $module..." -ForegroundColor Cyan
    dotnet new classlib -n "MedWay.$module" -o "$srcDir/Modules/$module/MedWay.$module" -f net10.0
    dotnet new classlib -n "MedWay.$module.Domain" -o "$srcDir/Modules/$module/MedWay.$module.Domain" -f net10.0
    dotnet new classlib -n "MedWay.$module.Application" -o "$srcDir/Modules/$module/MedWay.$module.Application" -f net10.0
    dotnet new classlib -n "MedWay.$module.Infrastructure" -o "$srcDir/Modules/$module/MedWay.$module.Infrastructure" -f net10.0
}

# Infrastructure Projects
Write-Host "`n⚙️  Creating INFRASTRUCTURE projects..." -ForegroundColor Green
dotnet new classlib -n MedWay.Infrastructure -o "$srcDir/Infrastructure/MedWay.Infrastructure" -f net10.0
dotnet new classlib -n MedWay.Infrastructure.AI -o "$srcDir/Infrastructure/MedWay.Infrastructure.AI" -f net10.0
dotnet new classlib -n MedWay.Infrastructure.Integration -o "$srcDir/Infrastructure/MedWay.Infrastructure.Integration" -f net10.0

# Presentation Projects
Write-Host "`n🖥️  Creating PRESENTATION projects..." -ForegroundColor Green
dotnet new webapi -n MedWay.WebAPI -o "$srcDir/Presentation/MedWay.WebAPI" -f net10.0
dotnet new blazorserver -n MedWay.BlazorApp -o "$srcDir/Presentation/MedWay.BlazorApp" -f net9.0

# Shared Projects
Write-Host "`n📦 Creating SHARED projects..." -ForegroundColor Green
dotnet new classlib -n MedWay.Shared -o "$srcDir/Shared/MedWay.Shared" -f net10.0
dotnet new classlib -n MedWay.Shared.Tests -o "$srcDir/Shared/MedWay.Shared.Tests" -f net10.0

# Test Projects
Write-Host "`n🧪 Creating TEST projects..." -ForegroundColor Green
dotnet new xunit -n MedWay.UnitTests -o "$testsDir/MedWay.UnitTests" -f net10.0
dotnet new xunit -n MedWay.IntegrationTests -o "$testsDir/MedWay.IntegrationTests" -f net10.0
dotnet new xunit -n MedWay.EndToEndTests -o "$testsDir/MedWay.EndToEndTests" -f net10.0

# Add all projects to solution
Write-Host "`n➕ Adding projects to solution..." -ForegroundColor Yellow

# Core
dotnet sln add "$srcDir/Core/MedWay.Domain/MedWay.Domain.csproj"
dotnet sln add "$srcDir/Core/MedWay.Application/MedWay.Application.csproj"
dotnet sln add "$srcDir/Core/MedWay.Contracts/MedWay.Contracts.csproj"

# Modules
foreach ($module in $modules) {
    dotnet sln add "$srcDir/Modules/$module/MedWay.$module/MedWay.$module.csproj"
    dotnet sln add "$srcDir/Modules/$module/MedWay.$module.Domain/MedWay.$module.Domain.csproj"
    dotnet sln add "$srcDir/Modules/$module/MedWay.$module.Application/MedWay.$module.Application.csproj"
    dotnet sln add "$srcDir/Modules/$module/MedWay.$module.Infrastructure/MedWay.$module.Infrastructure.csproj"
}

# Infrastructure
dotnet sln add "$srcDir/Infrastructure/MedWay.Infrastructure/MedWay.Infrastructure.csproj"
dotnet sln add "$srcDir/Infrastructure/MedWay.Infrastructure.AI/MedWay.Infrastructure.AI.csproj"
dotnet sln add "$srcDir/Infrastructure/MedWay.Infrastructure.Integration/MedWay.Infrastructure.Integration.csproj"

# Presentation
dotnet sln add "$srcDir/Presentation/MedWay.WebAPI/MedWay.WebAPI.csproj"
dotnet sln add "$srcDir/Presentation/MedWay.BlazorApp/MedWay.BlazorApp.csproj"

# Shared
dotnet sln add "$srcDir/Shared/MedWay.Shared/MedWay.Shared.csproj"
dotnet sln add "$srcDir/Shared/MedWay.Shared.Tests/MedWay.Shared.Tests.csproj"

# Tests
dotnet sln add "$testsDir/MedWay.UnitTests/MedWay.UnitTests.csproj"
dotnet sln add "$testsDir/MedWay.IntegrationTests/MedWay.IntegrationTests.csproj"
dotnet sln add "$testsDir/MedWay.EndToEndTests/MedWay.EndToEndTests.csproj"

Write-Host "`n✅ Project structure created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Project Summary:" -ForegroundColor Cyan
Write-Host "  - Core Projects: 3" -ForegroundColor White
Write-Host "  - Module Projects: $($modules.Count * 4) (12 modules × 4 layers)" -ForegroundColor White
Write-Host "  - Infrastructure Projects: 3" -ForegroundColor White
Write-Host "  - Presentation Projects: 2" -ForegroundColor White
Write-Host "  - Shared Projects: 2" -ForegroundColor White
Write-Host "  - Test Projects: 3" -ForegroundColor White
Write-Host "  - Total Projects: $((3 + ($modules.Count * 4) + 3 + 2 + 2 + 3))" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: dotnet restore" -ForegroundColor White
Write-Host "  2. Run: dotnet build" -ForegroundColor White
Write-Host "  3. Configure appsettings.json files" -ForegroundColor White
Write-Host "  4. Setup database connections" -ForegroundColor White
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
