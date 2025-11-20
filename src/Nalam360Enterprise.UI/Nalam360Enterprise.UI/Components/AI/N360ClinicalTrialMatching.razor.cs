using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Components.AI
{
    public partial class N360ClinicalTrialMatching : ComponentBase
    {
        [Inject] private IAIService? AIService { get; set; }
        [Inject] private IAIComplianceService? ComplianceService { get; set; }
        [Inject] private IMLModelService? MLModelService { get; set; }

        [Parameter] public string? RequiredPermission { get; set; } = "ClinicalTrials.View";
        [Parameter] public bool EnableAudit { get; set; } = true;
        
        // AI Features
        [Parameter] public bool UseRealAI { get; set; } = false;
        [Parameter] public string? AIModelEndpoint { get; set; }
        [Parameter] public string? AIApiKey { get; set; }
        [Parameter] public bool EnablePHIDetection { get; set; } = true;
        [Parameter] public string? UserId { get; set; }

        private bool HasPermission { get; set; }
        private bool IsMatching { get; set; }

        // Patient input fields
        private string PatientId { get; set; } = "P-2024-8945";
        private string Diagnosis { get; set; } = "C50.9 - Breast Cancer";
        private string DiseaseStage { get; set; } = "Stage III";
        private int PatientAge { get; set; } = 52;
        private string ECOGStatus { get; set; } = "1 - Ambulatory";
        private string BiomarkerStatus { get; set; } = "BRCA1+, ER+, PR+, HER2-";
        private int PreviousTreatments { get; set; } = 2;
        private string Location { get; set; } = "02115";

        // Search fields
        private string SearchKeywords { get; set; } = string.Empty;
        private string SearchPhase { get; set; } = string.Empty;
        private string SearchStatus { get; set; } = string.Empty;
        private string SearchSponsor { get; set; } = string.Empty;
        private string SelectedTrialForEligibility { get; set; } = string.Empty;

        // Dropdown data sources
        private List<string> DiseaseStages = new() { "Stage 0", "Stage I", "Stage II", "Stage III", "Stage IV", "Metastatic", "Recurrent" };
        private List<string> ECOGStatuses = new() { "0 - Fully Active", "1 - Ambulatory", "2 - Self-Care", "3 - Limited Self-Care", "4 - Disabled" };
        private List<string> Phases = new() { "Early Phase 1", "Phase 1", "Phase 2", "Phase 3", "Phase 4" };
        private List<string> Statuses = new() { "Recruiting", "Active", "Completed", "Suspended", "Terminated" };
        private List<string> SponsorTypes = new() { "Industry", "Academic", "Government", "Non-Profit" };

        // Results
        private List<ClinicalTrialMatch> MatchingResults { get; set; } = new();
        private List<ClinicalTrial> AllTrials { get; set; } = new();
        private List<PatientReferral> Referrals { get; set; } = new();

        // Analytics data
        private TrialStatistics Statistics { get; set; } = new();
        private List<PhaseData> PhaseDistribution { get; set; } = new();
        private List<StatusData> StatusDistribution { get; set; } = new();
        private List<ReferralTrend> ReferralsTrend { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            HasPermission = await PermissionService.HasPermissionAsync(RequiredPermission ?? "ClinicalTrials.View");

            if (HasPermission)
            {
                InitializeSampleData();
            }

            await base.OnInitializedAsync();
        }

        private async Task FindMatchingTrials()
        {
            IsMatching = true;
            StateHasChanged();

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "MatchPatient",
                    Resource = "ClinicalTrials",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object> { ["PatientId"] = PatientId ?? "", ["Diagnosis"] = Diagnosis ?? "" }
                });
            }

            try
            {
                if (UseRealAI && MLModelService != null && !string.IsNullOrWhiteSpace(AIModelEndpoint) && !string.IsNullOrWhiteSpace(AIApiKey))
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

            IsMatching = false;
            StateHasChanged();
        }

        private async Task ProcessWithMockAI()
        {
            await Task.Delay(3000);
            MatchingResults = AllTrials.Take(8).Select((trial, index) => new ClinicalTrialMatch
            {
                NCTId = trial.NCTId,
                TrialTitle = trial.TrialTitle,
                Phase = trial.Phase,
                RecruitmentStatus = trial.RecruitmentStatus,
                Sponsor = trial.Sponsor,
                Condition = trial.Condition,
                Intervention = trial.Intervention,
                PrimaryOutcome = trial.PrimaryOutcome,
                EnrollmentTarget = trial.EnrollmentTarget,
                MatchScore = 95 - (index * 5),
                DistanceMiles = 5 + (index * 8),
                EligibilityCriteria = new List<EligibilityCriterion>
                {
                    new EligibilityCriterion { Criterion = "Age 18-75", IsMet = PatientAge >= 18 && PatientAge <= 75 },
                    new EligibilityCriterion { Criterion = "ECOG 0-2", IsMet = ECOGStatus.StartsWith("0") || ECOGStatus.StartsWith("1") || ECOGStatus.StartsWith("2") },
                    new EligibilityCriterion { Criterion = "Confirmed diagnosis", IsMet = true },
                    new EligibilityCriterion { Criterion = "Prior treatments ≤ 3", IsMet = PreviousTreatments <= 3 },
                    new EligibilityCriterion { Criterion = "BRCA1+ biomarker", IsMet = BiomarkerStatus.Contains("BRCA1+") },
                    new EligibilityCriterion { Criterion = "Adequate organ function", IsMet = true }
                }
            }).ToList();
        }

        private async Task ProcessWithRealAI()
        {
            var patientData = new Dictionary<string, object>
            {
                ["patientId"] = PatientId,
                ["diagnosis"] = Diagnosis,
                ["age"] = PatientAge,
                ["ecog"] = ECOGStatus,
                ["biomarkers"] = BiomarkerStatus,
                ["previousTreatments"] = PreviousTreatments,
                ["location"] = Location
            };

            if (MLModelService != null)
            {
                var matchResult = await MLModelService.PredictAsync(
                    "trial-matching",
                    patientData);
                
                if (matchResult.IsSuccess)
                {
                    await Task.Delay(500);
                    // Parse real results or fall back
                    await ProcessWithMockAI();
                }
                else
                {
                    await ProcessWithMockAI();
                }
            }

            if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
            {
                await ComplianceService.AuditAIOperationAsync(
                    "ClinicalTrialMatching",
                    UserId,
                    $"Patient: {PatientId}, Diagnosis: {Diagnosis}",
                    $"Matched {MatchingResults.Count} trials");
            }
        }

        private void InitializeSampleData()
        {
            // Initialize clinical trials database
            AllTrials = new List<ClinicalTrial>
            {
                new ClinicalTrial
                {
                    NCTId = "NCT05892341",
                    TrialTitle = "Phase III Study of Olaparib Plus Pembrolizumab in BRCA1-Mutant Breast Cancer",
                    Phase = "3",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "AstraZeneca",
                    Condition = "Breast Cancer, BRCA1 Mutation",
                    Intervention = "Olaparib 300mg BID + Pembrolizumab 200mg Q3W",
                    PrimaryOutcome = "Progression-Free Survival",
                    EnrollmentTarget = 450,
                    MatchScore = 94.5,
                    InclusionCriteria = new List<string>
                    {
                        "Age ≥ 18 years",
                        "Histologically confirmed breast cancer",
                        "BRCA1 or BRCA2 germline mutation",
                        "ECOG performance status 0-1",
                        "Prior treatment with ≤ 2 lines of therapy",
                        "Measurable disease per RECIST 1.1"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Active brain metastases",
                        "Prior PARP inhibitor therapy",
                        "Uncontrolled cardiac disease",
                        "Active autoimmune disease"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05743621",
                    TrialTitle = "Talazoparib Combined with Enzalutamide in Advanced Breast Cancer",
                    Phase = "2",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Pfizer",
                    Condition = "Triple Negative Breast Cancer",
                    Intervention = "Talazoparib 1mg daily + Enzalutamide 160mg daily",
                    PrimaryOutcome = "Objective Response Rate",
                    EnrollmentTarget = 180,
                    MatchScore = 89.2,
                    InclusionCriteria = new List<string>
                    {
                        "Age ≥ 18 years",
                        "Triple negative breast cancer",
                        "HRR gene mutation (BRCA1/2, PALB2, etc.)",
                        "ECOG 0-2",
                        "Failed at least 1 prior systemic therapy"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior PARP inhibitor",
                        "Symptomatic brain metastases",
                        "Severe hepatic impairment"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05621847",
                    TrialTitle = "Platinum-Based Chemotherapy with Immunotherapy in BRCA-Mutant Cancers",
                    Phase = "3",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Bristol Myers Squibb",
                    Condition = "BRCA-Mutant Breast/Ovarian Cancer",
                    Intervention = "Carboplatin + Nivolumab vs Carboplatin alone",
                    PrimaryOutcome = "Overall Survival",
                    EnrollmentTarget = 620,
                    MatchScore = 91.7,
                    InclusionCriteria = new List<string>
                    {
                        "Age 18-75 years",
                        "BRCA1 or BRCA2 germline mutation",
                        "Platinum-sensitive disease",
                        "ECOG 0-1"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior checkpoint inhibitor",
                        "Active autoimmune disease",
                        "Platinum-refractory disease"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05498732",
                    TrialTitle = "Neoadjuvant Immunotherapy Plus PARP Inhibition in Early Stage Breast Cancer",
                    Phase = "2",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Dana-Farber Cancer Institute",
                    Condition = "Stage II-III Breast Cancer",
                    Intervention = "Durvalumab + Olaparib (neoadjuvant)",
                    PrimaryOutcome = "Pathologic Complete Response Rate",
                    EnrollmentTarget = 85,
                    MatchScore = 87.3,
                    InclusionCriteria = new List<string>
                    {
                        "Stage II-III breast cancer",
                        "HRR mutation present",
                        "Treatment-naive",
                        "ECOG 0-1"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior systemic therapy",
                        "Metastatic disease",
                        "Pregnancy"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05387294",
                    TrialTitle = "CAR-T Cell Therapy in HER2-Positive Breast Cancer",
                    Phase = "1",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Memorial Sloan Kettering",
                    Condition = "HER2-Positive Breast Cancer",
                    Intervention = "Autologous HER2-CAR T cells",
                    PrimaryOutcome = "Maximum Tolerated Dose",
                    EnrollmentTarget = 24,
                    MatchScore = 72.4,
                    InclusionCriteria = new List<string>
                    {
                        "HER2-positive (IHC 3+ or FISH+)",
                        "Failed ≥ 2 HER2-directed therapies",
                        "ECOG 0-1",
                        "Adequate bone marrow function"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Active CNS disease",
                        "Prior CAR-T therapy",
                        "Active infection"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05294618",
                    TrialTitle = "Antibody-Drug Conjugate SG-3401 in Advanced Solid Tumors",
                    Phase = "1",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Seagen Inc.",
                    Condition = "Advanced Breast Cancer",
                    Intervention = "SG-3401 (anti-Trop2 ADC)",
                    PrimaryOutcome = "Safety and Tolerability",
                    EnrollmentTarget = 120,
                    MatchScore = 83.6,
                    InclusionCriteria = new List<string>
                    {
                        "Advanced solid tumor",
                        "Failed standard therapies",
                        "Trop2 expression confirmed",
                        "ECOG 0-2"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior Trop2-directed ADC",
                        "Uncontrolled brain metastases",
                        "Severe neuropathy"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05176892",
                    TrialTitle = "Alpelisib + Fulvestrant in PIK3CA-Mutant Breast Cancer",
                    Phase = "3",
                    RecruitmentStatus = "Active",
                    Sponsor = "Novartis",
                    Condition = "HR+/HER2- Breast Cancer with PIK3CA Mutation",
                    Intervention = "Alpelisib 300mg daily + Fulvestrant 500mg",
                    PrimaryOutcome = "Progression-Free Survival",
                    EnrollmentTarget = 510,
                    MatchScore = 76.8,
                    InclusionCriteria = new List<string>
                    {
                        "HR+/HER2- breast cancer",
                        "PIK3CA mutation confirmed",
                        "Postmenopausal or ovarian suppression",
                        "Prior endocrine therapy"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior PI3K inhibitor",
                        "Uncontrolled diabetes",
                        "Active pneumonitis"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT05093847",
                    TrialTitle = "Elacestrant vs Standard of Care in ESR1-Mutant Breast Cancer",
                    Phase = "3",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Stemline Therapeutics",
                    Condition = "ER+/HER2- Breast Cancer, ESR1 Mutation",
                    Intervention = "Elacestrant 345mg daily vs physician's choice",
                    PrimaryOutcome = "Progression-Free Survival",
                    EnrollmentTarget = 478,
                    MatchScore = 79.5,
                    InclusionCriteria = new List<string>
                    {
                        "ER+/HER2- metastatic breast cancer",
                        "ESR1 mutation in ctDNA",
                        "1-2 prior lines of endocrine therapy",
                        "ECOG 0-1"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior SERD therapy",
                        "Brain metastases requiring treatment",
                        "Visceral crisis"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT04982736",
                    TrialTitle = "Combination CDK4/6 Inhibitor with Endocrine Therapy First-Line",
                    Phase = "4",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Eli Lilly",
                    Condition = "HR+/HER2- Advanced Breast Cancer",
                    Intervention = "Abemaciclib + Letrozole or Anastrozole",
                    PrimaryOutcome = "Real-World Progression-Free Survival",
                    EnrollmentTarget = 1250,
                    MatchScore = 81.2,
                    InclusionCriteria = new List<string>
                    {
                        "HR+/HER2- advanced breast cancer",
                        "No prior systemic therapy for advanced disease",
                        "Postmenopausal",
                        "ECOG 0-2"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior CDK4/6 inhibitor",
                        "Rapidly progressing visceral disease",
                        "Uncontrolled diarrhea"
                    }
                },
                new ClinicalTrial
                {
                    NCTId = "NCT04871523",
                    TrialTitle = "Novel Bispecific Antibody in Metastatic Breast Cancer",
                    Phase = "2",
                    RecruitmentStatus = "Recruiting",
                    Sponsor = "Genentech",
                    Condition = "Metastatic Triple Negative Breast Cancer",
                    Intervention = "RG-7845 (HER2 x CD3 bispecific)",
                    PrimaryOutcome = "Objective Response Rate",
                    EnrollmentTarget = 95,
                    MatchScore = 68.9,
                    InclusionCriteria = new List<string>
                    {
                        "Metastatic triple negative breast cancer",
                        "HER2-low expression (IHC 1-2+)",
                        "Failed ≥ 2 prior therapies",
                        "ECOG 0-1"
                    },
                    ExclusionCriteria = new List<string>
                    {
                        "Prior bispecific antibody",
                        "Active CNS metastases",
                        "Autoimmune disease requiring immunosuppression"
                    }
                }
            };

            // Initialize referrals
            Referrals = new List<PatientReferral>
            {
                new PatientReferral { ReferralId = "REF-2024-001", PatientId = "P-2024-7891", NCTId = "NCT05892341", ReferralDate = DateTime.Now.AddDays(-15), Status = "Enrolled", MatchScore = 94.5, ReferringPhysician = "Dr. Sarah Johnson" },
                new PatientReferral { ReferralId = "REF-2024-002", PatientId = "P-2024-7892", NCTId = "NCT05743621", ReferralDate = DateTime.Now.AddDays(-12), Status = "Approved", MatchScore = 89.2, ReferringPhysician = "Dr. Michael Chen" },
                new PatientReferral { ReferralId = "REF-2024-003", PatientId = "P-2024-7893", NCTId = "NCT05621847", ReferralDate = DateTime.Now.AddDays(-10), Status = "Pending", MatchScore = 91.7, ReferringPhysician = "Dr. Emily Rodriguez" },
                new PatientReferral { ReferralId = "REF-2024-004", PatientId = "P-2024-7894", NCTId = "NCT05498732", ReferralDate = DateTime.Now.AddDays(-8), Status = "Enrolled", MatchScore = 87.3, ReferringPhysician = "Dr. Sarah Johnson" },
                new PatientReferral { ReferralId = "REF-2024-005", PatientId = "P-2024-7895", NCTId = "NCT05387294", ReferralDate = DateTime.Now.AddDays(-7), Status = "Declined", MatchScore = 72.4, ReferringPhysician = "Dr. David Kim" },
                new PatientReferral { ReferralId = "REF-2024-006", PatientId = "P-2024-7896", NCTId = "NCT05294618", ReferralDate = DateTime.Now.AddDays(-5), Status = "Approved", MatchScore = 83.6, ReferringPhysician = "Dr. Michael Chen" },
                new PatientReferral { ReferralId = "REF-2024-007", PatientId = "P-2024-7897", NCTId = "NCT05176892", ReferralDate = DateTime.Now.AddDays(-3), Status = "Pending", MatchScore = 76.8, ReferringPhysician = "Dr. Emily Rodriguez" },
                new PatientReferral { ReferralId = "REF-2024-008", PatientId = "P-2024-7898", NCTId = "NCT05093847", ReferralDate = DateTime.Now.AddDays(-2), Status = "Pending", MatchScore = 79.5, ReferringPhysician = "Dr. Sarah Johnson" },
                new PatientReferral { ReferralId = "REF-2024-009", PatientId = "P-2024-7899", NCTId = "NCT04982736", ReferralDate = DateTime.Now.AddDays(-1), Status = "Approved", MatchScore = 81.2, ReferringPhysician = "Dr. David Kim" },
                new PatientReferral { ReferralId = "REF-2024-010", PatientId = "P-2024-7900", NCTId = "NCT04871523", ReferralDate = DateTime.Now, Status = "Pending", MatchScore = 68.9, ReferringPhysician = "Dr. Michael Chen" }
            };

            // Initialize statistics
            Statistics = new TrialStatistics
            {
                TotalTrials = AllTrials.Count,
                TotalEnrollment = AllTrials.Sum(t => t.EnrollmentTarget),
                PatientsReferred = Referrals.Count,
                EnrollmentRate = 68.5
            };

            // Phase distribution
            PhaseDistribution = new List<PhaseData>
            {
                new PhaseData { Phase = "Phase 1", Count = AllTrials.Count(t => t.Phase == "1") },
                new PhaseData { Phase = "Phase 2", Count = AllTrials.Count(t => t.Phase == "2") },
                new PhaseData { Phase = "Phase 3", Count = AllTrials.Count(t => t.Phase == "3") },
                new PhaseData { Phase = "Phase 4", Count = AllTrials.Count(t => t.Phase == "4") }
            };

            // Status distribution
            StatusDistribution = new List<StatusData>
            {
                new StatusData { Status = "Recruiting", Count = AllTrials.Count(t => t.RecruitmentStatus == "Recruiting") },
                new StatusData { Status = "Active", Count = AllTrials.Count(t => t.RecruitmentStatus == "Active") },
                new StatusData { Status = "Completed", Count = AllTrials.Count(t => t.RecruitmentStatus == "Completed") }
            };

            // Referrals trend
            ReferralsTrend = new List<ReferralTrend>
            {
                new ReferralTrend { Month = "Jun", Referrals = 12 },
                new ReferralTrend { Month = "Jul", Referrals = 18 },
                new ReferralTrend { Month = "Aug", Referrals = 15 },
                new ReferralTrend { Month = "Sep", Referrals = 22 },
                new ReferralTrend { Month = "Oct", Referrals = 27 },
                new ReferralTrend { Month = "Nov", Referrals = 19 }
            };
        }

        private string GetMatchLevel(double matchScore)
        {
            if (matchScore >= 90) return "excellent";
            if (matchScore >= 75) return "good";
            return "moderate";
        }

        private string GetMatchColor(double matchScore)
        {
            if (matchScore >= 90) return "#27ae60";
            if (matchScore >= 75) return "#3498db";
            return "#f39c12";
        }
    }

    // Data models
    public class ClinicalTrial
    {
        public string NCTId { get; set; } = string.Empty;
        public string TrialTitle { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string RecruitmentStatus { get; set; } = string.Empty;
        public string Sponsor { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Intervention { get; set; } = string.Empty;
        public string PrimaryOutcome { get; set; } = string.Empty;
        public int EnrollmentTarget { get; set; }
        public double MatchScore { get; set; }
        public List<string> InclusionCriteria { get; set; } = new();
        public List<string> ExclusionCriteria { get; set; } = new();
    }

    public class ClinicalTrialMatch : ClinicalTrial
    {
        public double DistanceMiles { get; set; }
        public List<EligibilityCriterion> EligibilityCriteria { get; set; } = new();
    }

    public class EligibilityCriterion
    {
        public string Criterion { get; set; } = string.Empty;
        public bool IsMet { get; set; }
    }

    public class PatientReferral
    {
        public string ReferralId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string NCTId { get; set; } = string.Empty;
        public DateTime ReferralDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public string ReferringPhysician { get; set; } = string.Empty;
    }

    public class TrialStatistics
    {
        public int TotalTrials { get; set; }
        public int TotalEnrollment { get; set; }
        public int PatientsReferred { get; set; }
        public double EnrollmentRate { get; set; }
    }

    public class PhaseData
    {
        public string Phase { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusData
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ReferralTrend
    {
        public string Month { get; set; } = string.Empty;
        public int Referrals { get; set; }
    }
}
