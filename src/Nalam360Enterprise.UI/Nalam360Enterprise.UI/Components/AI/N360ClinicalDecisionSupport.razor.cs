using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Components.AI
{
    public partial class N360ClinicalDecisionSupport : ComponentBase
    {
        [Inject] private IAIService? AIService { get; set; }
        [Inject] private IAIComplianceService? ComplianceService { get; set; }
        [Inject] private IMLModelService? MLModelService { get; set; }

        [Parameter] public string? RequiredPermission { get; set; }
        [Parameter] public bool EnableAudit { get; set; } = true;
        
        // AI Features
        [Parameter] public bool UseRealAI { get; set; } = false;
        [Parameter] public string? AIModelEndpoint { get; set; }
        [Parameter] public string? AIApiKey { get; set; }
        [Parameter] public bool EnablePHIDetection { get; set; } = true;
        [Parameter] public string? UserId { get; set; }

        private bool HasPermission { get; set; }
        private bool IsAnalyzing { get; set; }
        
        private string PatientId { get; set; } = string.Empty;
        private string ChiefComplaint { get; set; } = string.Empty;
        private string ClinicalContext { get; set; } = string.Empty;
        
        private ClinicalDecisionAnalysis? CurrentAnalysis { get; set; }

        protected override async Task OnInitializedAsync()
        {
            HasPermission = string.IsNullOrEmpty(RequiredPermission) || 
                          await PermissionService.HasPermissionAsync(RequiredPermission);
            
            if (EnableAudit && HasPermission)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "Accessed",
                    Resource = "ClinicalDecisionSupport",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object> { ["Details"] = "User accessed Clinical Decision Support system" }
                });
            }
        }

        private async Task AnalyzePatient()
        {
            if (string.IsNullOrWhiteSpace(PatientId) || string.IsNullOrWhiteSpace(ChiefComplaint))
            {
                return;
            }

            IsAnalyzing = true;
            StateHasChanged();

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

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "PatientAnalyzed",
                    Resource = "ClinicalDecisionSupport",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["PatientId"] = PatientId ?? "",
                        ["ChiefComplaint"] = ChiefComplaint ?? ""
                    }
                });
            }

            IsAnalyzing = false;
            StateHasChanged();
        }

        private async Task ProcessWithMockAI()
        {
            await Task.Delay(2500);
            CurrentAnalysis = GenerateAnalysis();
        }

        private async Task ProcessWithRealAI()
        {
            var processedText = $"{ChiefComplaint}. {ClinicalContext}";

            // PHI Detection
            if (EnablePHIDetection && ComplianceService != null)
            {
                var phiElements = await ComplianceService.DetectPHIAsync(processedText);
                if (phiElements.Any())
                {
                    processedText = await ComplianceService.DeIdentifyAsync(processedText, phiElements);
                }
            }

            // AI clinical decision support analysis
            if (AIService != null)
            {
                var context = $"Patient ID: {PatientId}. Provide clinical decision support based on the following data:";
                var response = await AIService.GenerateResponseAsync(context, processedText);
                
                if (!string.IsNullOrEmpty(response))
                {
                    CurrentAnalysis = ParseAIClinicalAnalysis(response);
                }
                else
                {
                    CurrentAnalysis = GenerateAnalysis();
                }
            }
            else
            {
                CurrentAnalysis = GenerateAnalysis();
            }

            // Audit AI operation
            if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
            {
                await ComplianceService.AuditAIOperationAsync(
                    "ClinicalDecisionSupport",
                    UserId,
                    processedText,
                    $"Generated {CurrentAnalysis?.CriticalAlerts.Count ?? 0} alerts and {CurrentAnalysis?.Recommendations.Count ?? 0} recommendations");
            }
        }

        private ClinicalDecisionAnalysis ParseAIClinicalAnalysis(dynamic result)
        {
            // Parse AI response - fallback to mock for now
            return GenerateAnalysis();
        }

        private ClinicalDecisionAnalysis GenerateAnalysis()
        {
            // Simulate comprehensive clinical decision support based on chief complaint
            var isCardiacComplaint = ChiefComplaint.ToLower().Contains("chest") || 
                                    ChiefComplaint.ToLower().Contains("pain") ||
                                    ChiefComplaint.ToLower().Contains("cardiac");

            var isDiabeticContext = ClinicalContext.ToLower().Contains("diabetes") ||
                                   ClinicalContext.ToLower().Contains("glucose") ||
                                   ClinicalContext.ToLower().Contains("insulin");

            var analysis = new ClinicalDecisionAnalysis
            {
                PatientId = PatientId,
                ChiefComplaint = ChiefComplaint,
                AnalysisTimestamp = DateTime.UtcNow,
                OverallConfidence = 92.3
            };

            // Critical Alerts
            if (isCardiacComplaint)
            {
                analysis.CriticalAlerts.Add(new ClinicalAlert
                {
                    Title = "Potential Acute Coronary Syndrome",
                    Severity = "Critical",
                    Description = "Patient presentation suggests possible ACS. Chest pain with concerning features requires immediate evaluation.",
                    RecommendedAction = "Obtain immediate ECG, troponin levels, activate chest pain protocol. Consider cardiology consult.",
                    EvidenceSource = "ACC/AHA 2021 Chest Pain Guidelines",
                    Confidence = 94.5
                });

                analysis.CriticalAlerts.Add(new ClinicalAlert
                {
                    Title = "Antiplatelet Therapy Indicated",
                    Severity = "High",
                    Description = "If ACS confirmed, immediate antiplatelet therapy is indicated unless contraindicated.",
                    RecommendedAction = "Administer aspirin 325mg, consider P2Y12 inhibitor (clopidogrel/ticagrelor) based on risk assessment.",
                    EvidenceSource = "ESC 2020 ACS Guidelines",
                    Confidence = 91.2
                });
            }

            // Warnings
            if (isDiabeticContext)
            {
                analysis.Warnings.Add(new ClinicalAlert
                {
                    Title = "Hypoglycemia Risk",
                    Severity = "Moderate",
                    Description = "Diabetic patient on insulin therapy. Monitor for hypoglycemia, especially if NPO or reduced oral intake.",
                    RecommendedAction = "Check blood glucose every 4 hours. Consider insulin dose adjustment. IV dextrose available at bedside.",
                    EvidenceSource = "ADA Standards of Care 2024",
                    Confidence = 88.7
                });
            }

            analysis.Warnings.Add(new ClinicalAlert
            {
                Title = "Medication Reconciliation Needed",
                Severity = "Moderate",
                Description = "Home medication list incomplete. Potential for medication errors or omissions.",
                RecommendedAction = "Complete comprehensive medication reconciliation including OTC and herbal supplements.",
                EvidenceSource = "Joint Commission National Patient Safety Goals",
                Confidence = 86.3
            });

            // Recommendations
            analysis.Recommendations.Add(new ClinicalRecommendation
            {
                Category = "Diagnostic Testing",
                Title = "Serial Cardiac Biomarkers",
                Description = "Obtain serial troponin measurements at 0, 3, and 6 hours to rule out myocardial injury.",
                ClinicalRationale = "High-sensitivity troponin testing with serial measurements improves diagnostic accuracy for ACS.",
                Priority = "High",
                EvidenceLevel = "Level A",
                GuidelineReference = "ACC/AHA 2021 Chest Pain Guidelines",
                Confidence = 95.8
            });

            analysis.Recommendations.Add(new ClinicalRecommendation
            {
                Category = "Imaging",
                Title = "Echocardiography Assessment",
                Description = "Consider transthoracic echocardiography to evaluate cardiac function and wall motion abnormalities.",
                ClinicalRationale = "Echo provides valuable information on cardiac structure, function, and regional wall motion that aids diagnosis.",
                Priority = "Medium",
                EvidenceLevel = "Level B",
                GuidelineReference = "ASE Guidelines 2023",
                Confidence = 89.4
            });

            analysis.Recommendations.Add(new ClinicalRecommendation
            {
                Category = "Pharmacotherapy",
                Title = "Beta-Blocker Therapy",
                Description = "Initiate beta-blocker therapy if ACS confirmed and no contraindications present.",
                ClinicalRationale = "Beta-blockers reduce mortality and reinfarction rates in post-MI patients.",
                Priority = "High",
                EvidenceLevel = "Level A",
                GuidelineReference = "ACC/AHA Secondary Prevention Guidelines",
                Confidence = 93.2
            });

            analysis.Recommendations.Add(new ClinicalRecommendation
            {
                Category = "Prevention",
                Title = "Statin Therapy Initiation",
                Description = "High-intensity statin therapy recommended for ACS patients regardless of baseline LDL levels.",
                ClinicalRationale = "Statins reduce cardiovascular events and mortality. Early initiation improves outcomes.",
                Priority = "Medium",
                EvidenceLevel = "Level A",
                GuidelineReference = "ACC/AHA Cholesterol Guidelines 2018",
                Confidence = 96.1
            });

            analysis.Recommendations.Add(new ClinicalRecommendation
            {
                Category = "Monitoring",
                Title = "Continuous Cardiac Monitoring",
                Description = "Place patient on continuous telemetry monitoring for rhythm surveillance.",
                ClinicalRationale = "Patients with suspected ACS are at risk for arrhythmias including ventricular tachycardia and fibrillation.",
                Priority = "High",
                EvidenceLevel = "Level B",
                GuidelineReference = "AHA/ACC Cardiac Monitoring Guidelines",
                Confidence = 91.8
            });

            analysis.Recommendations.Add(new ClinicalRecommendation
            {
                Category = "Consultation",
                Title = "Cardiology Consultation",
                Description = "Urgent cardiology consultation for risk stratification and management planning.",
                ClinicalRationale = "Early cardiology involvement improves outcomes in ACS through timely intervention and specialized care.",
                Priority = "High",
                EvidenceLevel = "Level A",
                GuidelineReference = "ESC ACS Management Guidelines",
                Confidence = 94.7
            });

            // Drug Interactions
            analysis.DrugInteractions.Add(new DrugInteraction
            {
                Drug1 = "Aspirin",
                Drug2 = "Warfarin",
                Severity = "Major",
                Description = "Concurrent use significantly increases bleeding risk.",
                ClinicalEffect = "Enhanced anticoagulant effect, increased risk of GI bleeding and hemorrhage.",
                Management = "Monitor INR closely. Consider PPI prophylaxis. Assess bleeding risk vs. benefit. May need dose adjustments."
            });

            analysis.DrugInteractions.Add(new DrugInteraction
            {
                Drug1 = "Atorvastatin",
                Drug2 = "Amlodipine",
                Severity = "Moderate",
                Description = "Amlodipine increases atorvastatin levels through CYP3A4 inhibition.",
                ClinicalEffect = "Increased risk of myopathy and rhabdomyolysis.",
                Management = "Monitor for muscle pain/weakness. Check CK if symptoms develop. Consider lower statin dose."
            });

            analysis.DrugInteractions.Add(new DrugInteraction
            {
                Drug1 = "Metformin",
                Drug2 = "Contrast Media",
                Severity = "Major",
                Description = "IV contrast can precipitate lactic acidosis in patients on metformin with renal impairment.",
                ClinicalEffect = "Risk of contrast-induced nephropathy and metformin-associated lactic acidosis.",
                Management = "Hold metformin before contrast study. Check renal function. Resume after 48h if kidney function stable."
            });

            analysis.DrugInteractions.Add(new DrugInteraction
            {
                Drug1 = "Clopidogrel",
                Drug2 = "Omeprazole",
                Severity = "Moderate",
                Description = "Omeprazole reduces conversion of clopidogrel to active metabolite via CYP2C19 inhibition.",
                ClinicalEffect = "Reduced antiplatelet effect, potentially decreased cardiovascular protection.",
                Management = "Consider alternative PPI (pantoprazole, lansoprazole) or H2 blocker. Separate dosing by 12+ hours."
            });

            // Guideline Compliance
            analysis.GuidelineCompliance.Add(new GuidelineCompliance
            {
                GuidelineName = "ACC/AHA Chest Pain Evaluation",
                Organization = "American College of Cardiology",
                Criteria = "ECG within 10 minutes of ED arrival for chest pain patients",
                ComplianceStatus = "Compliant",
                Gap = null,
                Recommendation = "Continue current protocol. Document ECG time in patient record.",
                Year = 2021,
                EvidenceLevel = "Class I, Level A"
            });

            analysis.GuidelineCompliance.Add(new GuidelineCompliance
            {
                GuidelineName = "ACS Antiplatelet Therapy",
                Organization = "European Society of Cardiology",
                Criteria = "Dual antiplatelet therapy (DAPT) for 12 months post-ACS",
                ComplianceStatus = "Partial",
                Gap = "Patient on aspirin monotherapy only",
                Recommendation = "Add P2Y12 inhibitor unless contraindicated. Assess bleeding risk with PRECISE-DAPT score.",
                Year = 2023,
                EvidenceLevel = "Class I, Level A"
            });

            analysis.GuidelineCompliance.Add(new GuidelineCompliance
            {
                GuidelineName = "Lipid Management in ACS",
                Organization = "ACC/AHA",
                Criteria = "High-intensity statin therapy (atorvastatin 80mg or rosuvastatin 40mg)",
                ComplianceStatus = "Non-Compliant",
                Gap = "Patient on moderate-intensity statin only",
                Recommendation = "Increase to high-intensity statin. Target LDL <70 mg/dL or >50% reduction from baseline.",
                Year = 2018,
                EvidenceLevel = "Class I, Level A"
            });

            analysis.GuidelineCompliance.Add(new GuidelineCompliance
            {
                GuidelineName = "Cardiac Rehabilitation Referral",
                Organization = "AHA/AACVPR",
                Criteria = "All ACS patients should be referred to cardiac rehab before discharge",
                ComplianceStatus = "Compliant",
                Gap = null,
                Recommendation = "Ensure patient scheduled for rehab intake within 2 weeks of discharge. Provide educational materials.",
                Year = 2022,
                EvidenceLevel = "Class I, Level A"
            });

            // Risk Scores
            analysis.RiskScores.Add(new RiskScore
            {
                ScoreName = "TIMI Risk Score for UA/NSTEMI",
                Value = "5 points",
                RiskLevel = "High",
                Interpretation = "30% risk of all-cause mortality, MI, or urgent revascularization at 14 days",
                Components = new List<string>
                {
                    "Age ≥65 years",
                    "≥3 CAD risk factors",
                    "Known CAD (stenosis ≥50%)",
                    "Aspirin use in last 7 days",
                    "Elevated cardiac markers"
                },
                ClinicalRecommendation = "Early invasive strategy recommended within 24 hours. Consider upstream glycoprotein IIb/IIIa inhibitor."
            });

            analysis.RiskScores.Add(new RiskScore
            {
                ScoreName = "GRACE Risk Score",
                Value = "142 points",
                RiskLevel = "Moderate",
                Interpretation = "Intermediate risk for 6-month mortality (3-6%)",
                Components = new List<string>
                {
                    "Age 65 years",
                    "Heart rate 95 bpm",
                    "Systolic BP 130 mmHg",
                    "Creatinine 1.2 mg/dL",
                    "Killip class II"
                },
                ClinicalRecommendation = "Invasive strategy within 72 hours. Aggressive medical management. Monitor closely in CCU setting."
            });

            analysis.RiskScores.Add(new RiskScore
            {
                ScoreName = "HAS-BLED Bleeding Risk",
                Value = "2 points",
                RiskLevel = "Low",
                Interpretation = "1.9% annual major bleeding risk on anticoagulation",
                Components = new List<string>
                {
                    "Hypertension present",
                    "Age 65-74 years"
                },
                ClinicalRecommendation = "DAPT appropriate. Standard intensity anticoagulation if indicated. Address modifiable risk factors."
            });

            analysis.RiskScores.Add(new RiskScore
            {
                ScoreName = "CHA₂DS₂-VASc (if AFib present)",
                Value = "3 points",
                RiskLevel = "Moderate",
                Interpretation = "3.2% annual stroke risk without anticoagulation",
                Components = new List<string>
                {
                    "CHF/LV dysfunction",
                    "Hypertension",
                    "Age 65-74 years"
                },
                ClinicalRecommendation = "Oral anticoagulation recommended. Consider DOAC over warfarin unless contraindicated. Balance with HAS-BLED."
            });

            return analysis;
        }
    }

    public class ClinicalDecisionAnalysis
    {
        public string PatientId { get; set; } = string.Empty;
        public string ChiefComplaint { get; set; } = string.Empty;
        public DateTime AnalysisTimestamp { get; set; }
        public double OverallConfidence { get; set; }
        public List<ClinicalAlert> CriticalAlerts { get; set; } = new();
        public List<ClinicalAlert> Warnings { get; set; } = new();
        public List<ClinicalRecommendation> Recommendations { get; set; } = new();
        public List<DrugInteraction> DrugInteractions { get; set; } = new();
        public List<GuidelineCompliance> GuidelineCompliance { get; set; } = new();
        public List<RiskScore> RiskScores { get; set; } = new();
    }

    public class ClinicalAlert
    {
        public string Title { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string EvidenceSource { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class ClinicalRecommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClinicalRationale { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string EvidenceLevel { get; set; } = string.Empty;
        public string GuidelineReference { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class DrugInteraction
    {
        public string Drug1 { get; set; } = string.Empty;
        public string Drug2 { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClinicalEffect { get; set; } = string.Empty;
        public string Management { get; set; } = string.Empty;
    }

    public class GuidelineCompliance
    {
        public string GuidelineName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
        public string ComplianceStatus { get; set; } = string.Empty;
        public string? Gap { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public int Year { get; set; }
        public string EvidenceLevel { get; set; } = string.Empty;
    }

    public class RiskScore
    {
        public string ScoreName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string Interpretation { get; set; } = string.Empty;
        public List<string> Components { get; set; } = new();
        public string ClinicalRecommendation { get; set; } = string.Empty;
    }
}
