using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Components.AI
{
    public partial class N360GenomicsAnalysis : ComponentBase
    {
        [Inject] private IAIService? AIService { get; set; }
        [Inject] private IAIComplianceService? ComplianceService { get; set; }
        [Inject] private IMLModelService? MLModelService { get; set; }

        [Parameter] public string? RequiredPermission { get; set; } = "Genomics.View";
        [Parameter] public bool EnableAudit { get; set; } = true;
        
        // AI Features
        [Parameter] public bool UseRealAI { get; set; } = false;
        [Parameter] public string? AIModelEndpoint { get; set; }
        [Parameter] public string? AIApiKey { get; set; }
        [Parameter] public bool EnablePHIDetection { get; set; } = true;
        [Parameter] public string? UserId { get; set; }

        private bool HasPermission { get; set; }
        private bool IsAnalyzing { get; set; }

        // Input fields
        private string PatientId { get; set; } = "P-2024-7891";
        private string SelectedTestType { get; set; } = "Whole Exome Sequencing";
        private string SampleId { get; set; } = "S-WES-2024-001";
        private string ClinicalIndication { get; set; } = "Family history of breast cancer";

        private List<string> TestTypes = new()
        {
            "Whole Exome Sequencing",
            "Whole Genome Sequencing",
            "Cancer Panel",
            "Pharmacogenomics Panel",
            "Carrier Screening",
            "Prenatal Testing",
            "Ancestry + Health"
        };

        // Current analysis result
        private GenomicAnalysisResult? CurrentAnalysis { get; set; }

        protected override async Task OnInitializedAsync()
        {
            HasPermission = await PermissionService.HasPermissionAsync(RequiredPermission ?? "Genomics.View");

            if (HasPermission)
            {
                // Initialize with sample data
                InitializeSampleData();
            }

            await base.OnInitializedAsync();
        }

        private async Task AnalyzeGenomics()
        {
            IsAnalyzing = true;
            StateHasChanged();

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "AnalyzeGenome",
                    Resource = "Genomics",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object> { ["PatientId"] = PatientId ?? "", ["TestType"] = SelectedTestType }
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

            IsAnalyzing = false;
            StateHasChanged();
        }

        private async Task ProcessWithMockAI()
        {
            await Task.Delay(2500);
            InitializeSampleData();
        }

        private async Task ProcessWithRealAI()
        {
            var genomicData = new Dictionary<string, object>
            {
                ["patientId"] = PatientId,
                ["testType"] = SelectedTestType,
                ["sampleId"] = SampleId,
                ["indication"] = ClinicalIndication
            };

            if (MLModelService != null)
            {
                var analysisResult = await MLModelService.PredictAsync(
                    "genomic-analysis",
                    genomicData);
                
                if (analysisResult.IsSuccess)
                {
                    await Task.Delay(500);
                    InitializeSampleData();
                }
                else
                {
                    InitializeSampleData();
                }
            }

            if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
            {
                await ComplianceService.AuditAIOperationAsync(
                    "GenomicsAnalysis",
                    UserId,
                    $"Test: {SelectedTestType}, Sample: {SampleId}",
                    $"Analyzed {CurrentAnalysis?.Variants.Count ?? 0} variants");
            }
        }

        private void InitializeSampleData()
        {
            CurrentAnalysis = new GenomicAnalysisResult
            {
                PatientId = PatientId,
                TestType = SelectedTestType,
                AnalysisDate = DateTime.Now,
                
                Variants = new List<GeneticVariant>
                {
                    new GeneticVariant
                    {
                        Gene = "BRCA1",
                        Variant = "c.5266dupC (p.Gln1756Profs)",
                        Classification = "Pathogenic",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.0001",
                        ClinicalSignificance = "Increased breast/ovarian cancer risk"
                    },
                    new GeneticVariant
                    {
                        Gene = "TP53",
                        Variant = "c.817C>T (p.Arg273Cys)",
                        Classification = "Pathogenic",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.00008",
                        ClinicalSignificance = "Li-Fraumeni syndrome"
                    },
                    new GeneticVariant
                    {
                        Gene = "ATM",
                        Variant = "c.5762-2A>G",
                        Classification = "Likely Pathogenic",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.0003",
                        ClinicalSignificance = "Moderate cancer risk"
                    },
                    new GeneticVariant
                    {
                        Gene = "CHEK2",
                        Variant = "c.1100delC",
                        Classification = "Pathogenic",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.005",
                        ClinicalSignificance = "Increased breast cancer risk"
                    },
                    new GeneticVariant
                    {
                        Gene = "PALB2",
                        Variant = "c.3549C>A (p.Tyr1183Ter)",
                        Classification = "Likely Pathogenic",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.0002",
                        ClinicalSignificance = "Breast/pancreatic cancer risk"
                    },
                    new GeneticVariant
                    {
                        Gene = "RAD51C",
                        Variant = "c.379-2A>G",
                        Classification = "VUS",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.0001",
                        ClinicalSignificance = "Uncertain ovarian cancer risk"
                    },
                    new GeneticVariant
                    {
                        Gene = "APOE",
                        Variant = "ε3/ε4",
                        Classification = "Benign",
                        Zygosity = "Heterozygous",
                        AlleleFrequency = "0.15",
                        ClinicalSignificance = "Moderate Alzheimer's risk"
                    },
                    new GeneticVariant
                    {
                        Gene = "CFTR",
                        Variant = "c.1521_1523delCTT (p.Phe508del)",
                        Classification = "Pathogenic",
                        Zygosity = "Carrier",
                        AlleleFrequency = "0.03",
                        ClinicalSignificance = "Cystic fibrosis carrier"
                    }
                },

                DiseaseRisks = new List<DiseaseRisk>
                {
                    new DiseaseRisk
                    {
                        DiseaseName = "Breast Cancer",
                        RiskLevel = "High",
                        RelativeRisk = "5.2x",
                        LifetimeRisk = "65-72%",
                        AssociatedGenes = new[] { "BRCA1", "CHEK2", "PALB2" },
                        Recommendation = "Annual mammography + MRI starting age 25, consider prophylactic mastectomy"
                    },
                    new DiseaseRisk
                    {
                        DiseaseName = "Ovarian Cancer",
                        RiskLevel = "High",
                        RelativeRisk = "8.7x",
                        LifetimeRisk = "39-46%",
                        AssociatedGenes = new[] { "BRCA1" },
                        Recommendation = "Biannual transvaginal ultrasound + CA-125, consider prophylactic oophorectomy after age 40"
                    },
                    new DiseaseRisk
                    {
                        DiseaseName = "Li-Fraumeni Syndrome",
                        RiskLevel = "High",
                        RelativeRisk = "10.0x",
                        LifetimeRisk = "70-90%",
                        AssociatedGenes = new[] { "TP53" },
                        Recommendation = "Comprehensive cancer surveillance protocol, genetic counseling for family"
                    },
                    new DiseaseRisk
                    {
                        DiseaseName = "Pancreatic Cancer",
                        RiskLevel = "Moderate",
                        RelativeRisk = "3.8x",
                        LifetimeRisk = "8-12%",
                        AssociatedGenes = new[] { "BRCA1", "PALB2" },
                        Recommendation = "Consider pancreatic surveillance with EUS/MRI starting age 50"
                    },
                    new DiseaseRisk
                    {
                        DiseaseName = "Alzheimer's Disease",
                        RiskLevel = "Moderate",
                        RelativeRisk = "3.2x",
                        LifetimeRisk = "25-30%",
                        AssociatedGenes = new[] { "APOE" },
                        Recommendation = "Cognitive assessment, lifestyle modifications, monitor cardiovascular health"
                    }
                },

                PharmacogenomicProfile = new PharmacogenomicProfile
                {
                    DrugRecommendations = new List<DrugRecommendation>
                    {
                        new DrugRecommendation
                        {
                            DrugName = "Warfarin",
                            Category = "Anticoagulant",
                            Genotype = "CYP2C9*1/*3, VKORC1 -1639G>A (AG)",
                            MetabolizerStatus = "Intermediate Metabolizer",
                            Recommendation = "Use with Caution",
                            EvidenceLevel = "1A - Strong",
                            ClinicalGuidance = "Reduce initial dose by 25-50%. Requires more frequent INR monitoring. Increased bleeding risk with standard dosing.",
                            AlternativeDrug = "Apixaban or Rivaroxaban (no genetic interaction)"
                        },
                        new DrugRecommendation
                        {
                            DrugName = "Clopidogrel",
                            Category = "Antiplatelet",
                            Genotype = "CYP2C19*2/*17",
                            MetabolizerStatus = "Intermediate Metabolizer",
                            Recommendation = "Consider Alternative",
                            EvidenceLevel = "1A - Strong",
                            ClinicalGuidance = "Reduced conversion to active metabolite. Increased risk of cardiovascular events. Alternative P2Y12 inhibitor recommended.",
                            AlternativeDrug = "Prasugrel or Ticagrelor"
                        },
                        new DrugRecommendation
                        {
                            DrugName = "Simvastatin",
                            Category = "Statin",
                            Genotype = "SLCO1B1*5/*5",
                            MetabolizerStatus = "Poor Metabolizer",
                            Recommendation = "Consider Alternative",
                            EvidenceLevel = "1A - Strong",
                            ClinicalGuidance = "High risk of myopathy and rhabdomyolysis. Limit dose to 20mg/day or use alternative statin.",
                            AlternativeDrug = "Pravastatin or Rosuvastatin"
                        },
                        new DrugRecommendation
                        {
                            DrugName = "Codeine",
                            Category = "Opioid Analgesic",
                            Genotype = "CYP2D6*1/*4",
                            MetabolizerStatus = "Intermediate Metabolizer",
                            Recommendation = "Use with Caution",
                            EvidenceLevel = "1B - Moderate",
                            ClinicalGuidance = "Reduced conversion to morphine. May experience inadequate pain relief. Monitor efficacy closely.",
                            AlternativeDrug = "Morphine, Hydromorphone, or Oxycodone"
                        },
                        new DrugRecommendation
                        {
                            DrugName = "Tamoxifen",
                            Category = "Selective Estrogen Receptor Modulator",
                            Genotype = "CYP2D6*1/*1",
                            MetabolizerStatus = "Normal Metabolizer",
                            Recommendation = "Use Standard Dose",
                            EvidenceLevel = "1A - Strong",
                            ClinicalGuidance = "Normal conversion to active metabolite (endoxifen). Standard 20mg daily dosing appropriate.",
                            AlternativeDrug = ""
                        },
                        new DrugRecommendation
                        {
                            DrugName = "Azathioprine",
                            Category = "Immunosuppressant",
                            Genotype = "TPMT*1/*1, NUDT15*1/*1",
                            MetabolizerStatus = "Normal Metabolizer",
                            Recommendation = "Use Standard Dose",
                            EvidenceLevel = "1A - Strong",
                            ClinicalGuidance = "Normal TPMT and NUDT15 activity. Standard dosing (1-2.5 mg/kg/day) with routine monitoring.",
                            AlternativeDrug = ""
                        }
                    },

                    EnzymeActivity = new Dictionary<string, string>
                    {
                        { "CYP2C9", "Intermediate Metabolizer" },
                        { "CYP2C19", "Intermediate Metabolizer" },
                        { "CYP2D6", "Normal Metabolizer" },
                        { "CYP3A4", "Normal Metabolizer" },
                        { "CYP3A5", "Poor Metabolizer" },
                        { "TPMT", "Normal Metabolizer" },
                        { "SLCO1B1", "Poor Metabolizer" },
                        { "VKORC1", "Intermediate Metabolizer" }
                    }
                },

                CancerGenomics = new CancerGenomics
                {
                    SomaticMutations = 47,
                    ActionableMutations = 8,
                    TherapeuticOptions = 12,
                    ClinicalTrials = 23,

                    Biomarkers = new List<TumorBiomarker>
                    {
                        new TumorBiomarker
                        {
                            BiomarkerName = "BRCA1 Mutation",
                            Type = "Germline",
                            Status = "Positive",
                            Value = "c.5266dupC",
                            ClinicalImplication = "Homologous recombination deficiency. Tumor sensitive to PARP inhibitors and platinum chemotherapy.",
                            TargetedTherapy = "Olaparib, Talazoparib, Rucaparib"
                        },
                        new TumorBiomarker
                        {
                            BiomarkerName = "TP53 Mutation",
                            Type = "Germline",
                            Status = "Positive",
                            Value = "c.817C>T",
                            ClinicalImplication = "Loss of tumor suppressor function. Associated with aggressive tumor behavior and multiple cancer risk.",
                            TargetedTherapy = "APR-246 (investigational)"
                        },
                        new TumorBiomarker
                        {
                            BiomarkerName = "HER2",
                            Type = "Protein Expression",
                            Status = "Negative",
                            Value = "0 (IHC)",
                            ClinicalImplication = "Not eligible for HER2-targeted therapies. Consider other treatment pathways.",
                            TargetedTherapy = ""
                        },
                        new TumorBiomarker
                        {
                            BiomarkerName = "ER/PR",
                            Type = "Protein Expression",
                            Status = "Positive",
                            Value = "ER 85%, PR 70%",
                            ClinicalImplication = "Hormone receptor positive. Eligible for endocrine therapy.",
                            TargetedTherapy = "Tamoxifen, Aromatase Inhibitors"
                        },
                        new TumorBiomarker
                        {
                            BiomarkerName = "PD-L1",
                            Type = "Protein Expression",
                            Status = "Negative",
                            Value = "<1% (TPS)",
                            ClinicalImplication = "Low likelihood of response to single-agent checkpoint inhibitors.",
                            TargetedTherapy = ""
                        },
                        new TumorBiomarker
                        {
                            BiomarkerName = "TMB",
                            Type = "Genomic",
                            Status = "Positive",
                            Value = "14.2 mutations/Mb",
                            ClinicalImplication = "High tumor mutational burden. May benefit from immunotherapy.",
                            TargetedTherapy = "Pembrolizumab, Nivolumab"
                        }
                    },

                    TherapyRecommendations = new List<TherapyRecommendation>
                    {
                        new TherapyRecommendation
                        {
                            TherapyName = "Olaparib (Lynparza)",
                            Target = "PARP1/2",
                            EvidenceLevel = "Level 1A",
                            Rationale = "BRCA1 mutation confers sensitivity to PARP inhibition through synthetic lethality.",
                            ExpectedResponse = "60-70% response rate",
                            ClinicalTrials = 8
                        },
                        new TherapyRecommendation
                        {
                            TherapyName = "Platinum-based Chemotherapy",
                            Target = "DNA crosslinking",
                            EvidenceLevel = "Level 1A",
                            Rationale = "BRCA1-mutant tumors show increased sensitivity to platinum agents.",
                            ExpectedResponse = "70-80% response rate",
                            ClinicalTrials = 5
                        },
                        new TherapyRecommendation
                        {
                            TherapyName = "Pembrolizumab + Chemotherapy",
                            Target = "PD-1 checkpoint",
                            EvidenceLevel = "Level 2A",
                            Rationale = "High TMB suggests potential benefit from immunotherapy combination.",
                            ExpectedResponse = "40-50% response rate",
                            ClinicalTrials = 6
                        },
                        new TherapyRecommendation
                        {
                            TherapyName = "Endocrine Therapy",
                            Target = "Estrogen receptor",
                            EvidenceLevel = "Level 1A",
                            Rationale = "ER/PR positive tumor suitable for hormonal therapy in adjuvant setting.",
                            ExpectedResponse = "50-60% disease control",
                            ClinicalTrials = 4
                        }
                    }
                },

                AncestryProfile = new AncestryProfile
                {
                    Composition = new List<AncestryComposition>
                    {
                        new AncestryComposition { Region = "European", Percentage = 58.3 },
                        new AncestryComposition { Region = "South Asian", Percentage = 22.7 },
                        new AncestryComposition { Region = "East Asian", Percentage = 12.4 },
                        new AncestryComposition { Region = "Middle Eastern", Percentage = 4.8 },
                        new AncestryComposition { Region = "Sub-Saharan African", Percentage = 1.8 }
                    },

                    Traits = new List<GeneticTrait>
                    {
                        new GeneticTrait { TraitName = "Eye Color", Result = "Brown", Confidence = 94, IconClass = "fas fa-eye" },
                        new GeneticTrait { TraitName = "Hair Color", Result = "Dark Brown/Black", Confidence = 89, IconClass = "fas fa-user" },
                        new GeneticTrait { TraitName = "Lactose Tolerance", Result = "Tolerant", Confidence = 97, IconClass = "fas fa-glass-whiskey" },
                        new GeneticTrait { TraitName = "Caffeine Metabolism", Result = "Fast Metabolizer", Confidence = 92, IconClass = "fas fa-coffee" },
                        new GeneticTrait { TraitName = "Cilantro Taste", Result = "Normal (not soapy)", Confidence = 88, IconClass = "fas fa-leaf" },
                        new GeneticTrait { TraitName = "Muscle Composition", Result = "Fast-twitch fiber bias", Confidence = 81, IconClass = "fas fa-dumbbell" },
                        new GeneticTrait { TraitName = "Bitter Taste Sensitivity", Result = "High Sensitivity", Confidence = 95, IconClass = "fas fa-lemon" },
                        new GeneticTrait { TraitName = "Earwax Type", Result = "Wet", Confidence = 99, IconClass = "fas fa-ear-listen" }
                    },

                    PaternalHaplogroup = "R1b-M269",
                    MaternalHaplogroup = "H1a1"
                }
            };
        }

        private string GetEnzymeDescription(string enzyme)
        {
            return enzyme switch
            {
                "CYP2C9" => "Metabolizes warfarin, NSAIDs, phenytoin",
                "CYP2C19" => "Metabolizes clopidogrel, PPIs, SSRIs",
                "CYP2D6" => "Metabolizes codeine, tramadol, tamoxifen",
                "CYP3A4" => "Metabolizes statins, immunosuppressants",
                "CYP3A5" => "Metabolizes tacrolimus, statins",
                "TPMT" => "Metabolizes azathioprine, mercaptopurine",
                "SLCO1B1" => "Transports statins into hepatocytes",
                "VKORC1" => "Target of warfarin anticoagulation",
                _ => "Drug metabolizing enzyme"
            };
        }
    }

    // Data models
    public class GenomicAnalysisResult
    {
        public string PatientId { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<GeneticVariant> Variants { get; set; } = new();
        public List<DiseaseRisk> DiseaseRisks { get; set; } = new();
        public PharmacogenomicProfile PharmacogenomicProfile { get; set; } = new();
        public CancerGenomics? CancerGenomics { get; set; }
        public AncestryProfile AncestryProfile { get; set; } = new();
    }

    public class GeneticVariant
    {
        public string Gene { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string Zygosity { get; set; } = string.Empty;
        public string AlleleFrequency { get; set; } = string.Empty;
        public string ClinicalSignificance { get; set; } = string.Empty;
    }

    public class DiseaseRisk
    {
        public string DiseaseName { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string RelativeRisk { get; set; } = string.Empty;
        public string LifetimeRisk { get; set; } = string.Empty;
        public string[] AssociatedGenes { get; set; } = Array.Empty<string>();
        public string Recommendation { get; set; } = string.Empty;
    }

    public class PharmacogenomicProfile
    {
        public List<DrugRecommendation> DrugRecommendations { get; set; } = new();
        public Dictionary<string, string> EnzymeActivity { get; set; } = new();
    }

    public class DrugRecommendation
    {
        public string DrugName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Genotype { get; set; } = string.Empty;
        public string MetabolizerStatus { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string EvidenceLevel { get; set; } = string.Empty;
        public string ClinicalGuidance { get; set; } = string.Empty;
        public string AlternativeDrug { get; set; } = string.Empty;
    }

    public class CancerGenomics
    {
        public int SomaticMutations { get; set; }
        public int ActionableMutations { get; set; }
        public int TherapeuticOptions { get; set; }
        public int ClinicalTrials { get; set; }
        public List<TumorBiomarker> Biomarkers { get; set; } = new();
        public List<TherapyRecommendation> TherapyRecommendations { get; set; } = new();
    }

    public class TumorBiomarker
    {
        public string BiomarkerName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ClinicalImplication { get; set; } = string.Empty;
        public string TargetedTherapy { get; set; } = string.Empty;
    }

    public class TherapyRecommendation
    {
        public string TherapyName { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string EvidenceLevel { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public string ExpectedResponse { get; set; } = string.Empty;
        public int ClinicalTrials { get; set; }
    }

    public class AncestryProfile
    {
        public List<AncestryComposition> Composition { get; set; } = new();
        public List<GeneticTrait> Traits { get; set; } = new();
        public string PaternalHaplogroup { get; set; } = string.Empty;
        public string MaternalHaplogroup { get; set; } = string.Empty;
    }

    public class AncestryComposition
    {
        public string Region { get; set; } = string.Empty;
        public double Percentage { get; set; }
    }

    public class GeneticTrait
    {
        public string TraitName { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string IconClass { get; set; } = string.Empty;
    }
}
