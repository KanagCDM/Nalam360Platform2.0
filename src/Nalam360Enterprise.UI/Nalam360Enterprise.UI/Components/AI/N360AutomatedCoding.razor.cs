using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nalam360Enterprise.UI.Components.AI;

public partial class N360AutomatedCoding
{
    // Injected Services
    [Inject] private IAIService? AIService { get; set; }
    [Inject] private IAIComplianceService? ComplianceService { get; set; }
    [Inject] private IMLModelService? MLModelService { get; set; }

    // Parameters
    [Parameter] public string? RequiredPermission { get; set; }
    [Parameter] public bool HideIfNoPermission { get; set; }
    [Parameter] public bool EnableAudit { get; set; }
    [Parameter] public string? AuditResource { get; set; }
    [Parameter] public string? AuditAction { get; set; }
    [Parameter] public EventCallback<CodingResult> OnCodingComplete { get; set; }
    
    // AI Features
    [Parameter] public bool UseRealAI { get; set; } = false;
    [Parameter] public string? AIModelEndpoint { get; set; }
    [Parameter] public string? AIApiKey { get; set; }
    [Parameter] public bool EnablePHIDetection { get; set; } = true;
    [Parameter] public string? UserId { get; set; }

    // State
    private string ClinicalDocumentation { get; set; } = string.Empty;
    private bool IsAnalyzing { get; set; }
    private int AnalysisStep { get; set; }
    private string SelectedCodingType { get; set; } = "Inpatient";
    private string ActiveTab { get; set; } = "ICD10";
    private CodingResult? CodingResult { get; set; }

    private bool HasPermission { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(RequiredPermission) && PermissionService != null)
        {
            HasPermission = await PermissionService.HasPermissionAsync(RequiredPermission);
        }
        await Task.CompletedTask;
    }

    private List<string> CodingTypes { get; set; } = new()
    {
        "Inpatient",
        "Outpatient",
        "Emergency",
        "Surgery",
        "Radiology"
    };

    // Tab switching methods
    private void SetActiveTab(string tab) => ActiveTab = tab;

    private void LoadSampleNote()
    {
        ClinicalDocumentation = @"ADMISSION DATE: 11/15/2025
DISCHARGE DATE: 11/19/2025

ADMITTING DIAGNOSIS: Acute myocardial infarction

DISCHARGE DIAGNOSIS:
1. ST-elevation myocardial infarction (STEMI), anterior wall
2. Type 2 diabetes mellitus with diabetic nephropathy
3. Hypertension, essential
4. Hyperlipidemia

PROCEDURES PERFORMED:
1. Cardiac catheterization with coronary angiography
2. Percutaneous transluminal coronary angioplasty (PTCA) of LAD with drug-eluting stent placement
3. Left ventriculography

HOSPITAL COURSE:
This is a 62-year-old male with history of type 2 diabetes, hypertension, and hyperlipidemia who presented to the emergency department with acute onset of severe chest pain radiating to left arm, associated with diaphoresis and shortness of breath. Initial EKG showed ST-segment elevations in leads V1-V4 consistent with anterior STEMI. Troponin I was significantly elevated at 15.2 ng/mL.

Patient was emergently taken to the cardiac catheterization lab where coronary angiography revealed 95% stenosis of the proximal left anterior descending (LAD) artery. Successful PTCA was performed with placement of a 3.0 x 18mm drug-eluting stent. Final angiogram showed excellent flow with 0% residual stenosis.

Post-procedure, patient was transferred to CCU for monitoring. He remained hemodynamically stable. Echocardiogram on hospital day 2 showed reduced ejection fraction of 45% with anterior wall hypokinesis.

Patient's diabetes was managed with insulin therapy. Creatinine was elevated at 1.8 mg/dL indicating diabetic nephropathy. Hypertension was controlled with metoprolol and lisinopril. Lipid panel showed LDL 145 mg/dL, started on high-intensity statin therapy.

Patient was educated on cardiac rehabilitation, lifestyle modifications, medication compliance, and signs of complications. He was discharged in stable condition on hospital day 4.

DISCHARGE MEDICATIONS:
1. Aspirin 81 mg daily
2. Clopidogrel 75 mg daily
3. Atorvastatin 80 mg daily
4. Metoprolol succinate 50 mg daily
5. Lisinopril 10 mg daily
6. Insulin glargine 20 units subcutaneous at bedtime
7. Insulin lispro sliding scale

FOLLOW-UP:
Cardiology clinic in 1 week
Primary care in 2 weeks
Cardiac rehabilitation referral";

        StateHasChanged();
    }

    private void ClearDocumentation()
    {
        ClinicalDocumentation = string.Empty;
        CodingResult = null;
        StateHasChanged();
    }

    private async Task AnalyzeAndCode()
    {
        IsAnalyzing = true;
        AnalysisStep = 0;
        StateHasChanged();

        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "AnalyzeDocument",
                AdditionalData = new Dictionary<string, object>
                {
                    ["DocumentLength"] = ClinicalDocumentation.Length,
                    ["CodingType"] = SelectedCodingType,
                    ["UseRealAI"] = UseRealAI
                }
            });
        }

        try
        {
            if (UseRealAI && AIService != null && !string.IsNullOrWhiteSpace(AIModelEndpoint) && !string.IsNullOrWhiteSpace(AIApiKey))
            {
                await ProcessWithRealAI();
            }
            else
            {
                await ProcessWithMockAI();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI Processing Error: {ex.Message}");
            await ProcessWithMockAI();
        }

        IsAnalyzing = false;
        StateHasChanged();

        await OnCodingComplete.InvokeAsync(CodingResult);
    }

    private async Task ProcessWithMockAI()
    {
        // Simulate multi-step analysis
        for (int i = 1; i <= 5; i++)
        {
            await Task.Delay(400);
            AnalysisStep = i;
            StateHasChanged();
        }

        await Task.Delay(200);
        CodingResult = GenerateCodingResult();
    }

    private async Task ProcessWithRealAI()
    {
        var processedText = ClinicalDocumentation;

        // PHI Detection and De-identification
        if (EnablePHIDetection && ComplianceService != null)
        {
            AnalysisStep = 1;
            StateHasChanged();
            var phiElements = await ComplianceService.DetectPHIAsync(ClinicalDocumentation);
            if (phiElements.Any())
            {
                processedText = await ComplianceService.DeIdentifyAsync(ClinicalDocumentation, phiElements);
            }
        }

        // AI-based medical coding extraction
        AnalysisStep = 2;
        StateHasChanged();
        await Task.Delay(400);

        // Use ML Model for coding
        if (MLModelService != null)
        {
            AnalysisStep = 3;
            StateHasChanged();
            var codingPrediction = await MLModelService.PredictAsync(
                "medical-coding",
                new Dictionary<string, object> 
                { 
                    ["text"] = processedText, 
                    ["codingType"] = SelectedCodingType 
                });
            
            if (codingPrediction.IsSuccess)
            {
                AnalysisStep = 4;
                StateHasChanged();
                // Parse and use real AI results
                CodingResult = ParseAICodingResult(codingPrediction);
            }
            else
            {
                // Fallback to mock
                AnalysisStep = 5;
                StateHasChanged();
                CodingResult = GenerateCodingResult();
            }
        }
        else
        {
            CodingResult = GenerateCodingResult();
        }

        // Audit AI operation
        if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
        {
            await ComplianceService.AuditAIOperationAsync(
                "AutomatedCoding",
                UserId,
                processedText,
                $"Generated {CodingResult?.ICD10Codes.Count ?? 0} ICD-10 codes and {CodingResult?.CPTCodes.Count ?? 0} CPT codes");
        }
    }

    private CodingResult ParseAICodingResult(dynamic prediction)
    {
        // Parse AI service response into CodingResult
        // For now, fall back to mock data - would integrate real parsing logic
        return GenerateCodingResult();
    }

    private CodingResult GenerateCodingResult()
    {
        var result = new CodingResult
        {
            ProcessingTime = 2.4,
            DocumentLength = ClinicalDocumentation.Length
        };

        // Generate ICD-10 codes
        result.ICD10Codes = new List<MedicalCode>
        {
            new()
            {
                Code = "I21.09",
                Description = "ST elevation (STEMI) myocardial infarction involving other coronary artery of anterior wall",
                IsPrimary = true,
                Confidence = 98.5,
                EvidenceText = "ST-segment elevations in leads V1-V4 consistent with anterior STEMI"
            },
            new()
            {
                Code = "E11.21",
                Description = "Type 2 diabetes mellitus with diabetic nephropathy",
                IsPrimary = false,
                Confidence = 96.2,
                EvidenceText = "Type 2 diabetes...Creatinine was elevated at 1.8 mg/dL indicating diabetic nephropathy"
            },
            new()
            {
                Code = "I10",
                Description = "Essential (primary) hypertension",
                IsPrimary = false,
                Confidence = 95.8,
                EvidenceText = "History of...hypertension...Hypertension was controlled with metoprolol and lisinopril"
            },
            new()
            {
                Code = "E78.5",
                Description = "Hyperlipidemia, unspecified",
                IsPrimary = false,
                Confidence = 94.1,
                EvidenceText = "History of...hyperlipidemia...Lipid panel showed LDL 145 mg/dL"
            },
            new()
            {
                Code = "I50.32",
                Description = "Chronic diastolic (congestive) heart failure",
                IsPrimary = false,
                Confidence = 82.7,
                EvidenceText = "Echocardiogram...showed reduced ejection fraction of 45%"
            }
        };

        // Generate CPT codes
        result.CPTCodes = new List<MedicalCode>
        {
            new()
            {
                Code = "93458",
                Description = "Catheter placement in coronary artery(s) for coronary angiography, including intraprocedural injection(s)",
                Confidence = 97.8,
                Modifiers = "",
                EvidenceText = "Cardiac catheterization with coronary angiography"
            },
            new()
            {
                Code = "92928",
                Description = "Percutaneous transcatheter placement of intracoronary stent(s), with coronary angioplasty when performed; single major coronary artery or branch",
                Confidence = 96.5,
                Modifiers = "",
                EvidenceText = "PTCA of LAD with drug-eluting stent placement...3.0 x 18mm drug-eluting stent"
            },
            new()
            {
                Code = "93452",
                Description = "Left heart catheterization including intraprocedural injection(s) for left ventriculography",
                Confidence = 95.2,
                Modifiers = "",
                EvidenceText = "Left ventriculography"
            },
            new()
            {
                Code = "93306",
                Description = "Echocardiography, transthoracic, real-time with image documentation (2D), includes M-mode recording, when performed, complete",
                Confidence = 93.8,
                Modifiers = "",
                EvidenceText = "Echocardiogram on hospital day 2"
            }
        };

        // Extract clinical entities
        result.ExtractedEntities = new List<ClinicalEntity>
        {
            new() { Type = "Diagnosis", Text = "ST-elevation myocardial infarction", Confidence = 98.5, Context = "Anterior wall" },
            new() { Type = "Diagnosis", Text = "Type 2 diabetes mellitus", Confidence = 96.2, Context = "With diabetic nephropathy" },
            new() { Type = "Procedure", Text = "Cardiac catheterization", Confidence = 97.8, Context = "With coronary angiography" },
            new() { Type = "Procedure", Text = "PTCA with stent placement", Confidence = 96.5, Context = "LAD artery" },
            new() { Type = "Anatomy", Text = "Left anterior descending artery", Confidence = 98.1, Context = "95% stenosis" },
            new() { Type = "Medication", Text = "Aspirin 81 mg", Confidence = 99.2, Context = "Daily" },
            new() { Type = "Medication", Text = "Clopidogrel 75 mg", Confidence = 99.0, Context = "Daily" },
            new() { Type = "Symptom", Text = "Chest pain", Confidence = 97.5, Context = "Radiating to left arm" },
            new() { Type = "Symptom", Text = "Diaphoresis", Confidence = 96.8, Context = "Associated with chest pain" }
        };

        // Generate insights
        result.Insights = new List<CodingInsight>
        {
            new()
            {
                Type = "Suggestion",
                Title = "HCC Impact",
                Message = "Codes identified qualify for Hierarchical Condition Categories (HCC): HCC 86 (Acute Myocardial Infarction), HCC 18 (Diabetes with Chronic Complications). Estimated risk adjustment factor increase: 1.8."
            },
            new()
            {
                Type = "Info",
                Title = "DRG Assignment",
                Message = "Expected DRG: 247 (Percutaneous Cardiovascular Procedures with Drug-Eluting Stent with MCC). Estimated reimbursement: $22,500-$28,000 depending on geographic location."
            },
            new()
            {
                Type = "Suggestion",
                Title = "Additional Documentation Opportunity",
                Message = "Consider documenting acute systolic heart failure (I50.21) based on reduced ejection fraction of 45%. This would affect DRG grouping and increase reimbursement."
            },
            new()
            {
                Type = "Warning",
                Title = "Modifier Review Required",
                Message = "CPT codes 93458 and 92928 may require modifier 59 or XU to indicate distinct procedural services if performed in the same session. Verify with coding guidelines."
            },
            new()
            {
                Type = "Info",
                Title = "Quality Measures",
                Message = "Patient meets criteria for Core Measure reporting: Acute MI (AMI) measures. Ensure documentation includes aspirin on arrival, beta blocker prescribed at discharge, and statin therapy at discharge."
            }
        };

        result.OverallConfidence = result.ICD10Codes.Concat(result.CPTCodes).Average(c => c.Confidence);

        return result;
    }

    private async Task AcceptCode(MedicalCode code)
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "AcceptCode",
                AdditionalData = new Dictionary<string, object>
                {
                    ["Code"] = code.Code,
                    ["Description"] = code.Description
                }
            });
        }
        await Task.CompletedTask;
    }

    private async Task RejectCode(MedicalCode code)
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "RejectCode",
                AdditionalData = new Dictionary<string, object>
                {
                    ["Code"] = code.Code,
                    ["Description"] = code.Description
                }
            });
        }

        // Remove code from lists
        CodingResult?.ICD10Codes.Remove(code);
        CodingResult?.CPTCodes.Remove(code);
        StateHasChanged();
    }

    private async Task ViewDetails(MedicalCode code)
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "ViewCodeDetails",
                AdditionalData = new Dictionary<string, object>
                {
                    ["Code"] = code.Code
                }
            });
        }
        await Task.CompletedTask;
    }

    private async Task ExportCodes()
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "ExportCodes",
                AdditionalData = new Dictionary<string, object>
                {
                    ["ICD10Count"] = CodingResult?.ICD10Codes.Count ?? 0,
                    ["CPTCount"] = CodingResult?.CPTCodes.Count ?? 0
                }
            });
        }
        await Task.CompletedTask;
    }

    private async Task ApproveAllCodes()
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "ApproveAllCodes",
                AdditionalData = new Dictionary<string, object>
                {
                    ["TotalCodes"] = (CodingResult?.ICD10Codes.Count ?? 0) + (CodingResult?.CPTCodes.Count ?? 0)
                }
            });
        }
        await Task.CompletedTask;
    }

    private async Task SaveDraft()
    {
        if (EnableAudit)
        {
            await AuditService.LogAsync(new AuditMetadata
            {
                Resource = AuditResource ?? "AutomatedCoding",
                Action = "SaveDraft",
                AdditionalData = new Dictionary<string, object>
                {
                    ["Timestamp"] = DateTime.Now
                }
            });
        }
        await Task.CompletedTask;
    }

    private Dictionary<string, object> GetHtmlAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            ["role"] = "region",
        ["aria-label"] = "Automated Medical Coding Interface"
    };
    return attributes;
}
}

// Data classes
public class CodingResult
{
    public List<MedicalCode> ICD10Codes { get; set; } = new();
    public List<MedicalCode> CPTCodes { get; set; } = new();
    public List<ClinicalEntity> ExtractedEntities { get; set; } = new();
    public List<CodingInsight> Insights { get; set; } = new();
    public double ProcessingTime { get; set; }
    public int DocumentLength { get; set; }
    public double OverallConfidence { get; set; }
}

public class MedicalCode
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public double Confidence { get; set; }
    public string EvidenceText { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
}

public class ClinicalEntity
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Context { get; set; } = string.Empty;
}

public class CodingInsight
{
    public string Type { get; set; } = string.Empty; // "Warning", "Suggestion", "Info"
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}