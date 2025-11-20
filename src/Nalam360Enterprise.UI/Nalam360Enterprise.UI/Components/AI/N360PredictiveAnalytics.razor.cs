using Microsoft.AspNetCore.Components;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Components.AI
{
    public partial class N360PredictiveAnalytics : ComponentBase
    {
        [Inject] private IAIService? AIService { get; set; }
        [Inject] private IAIComplianceService? ComplianceService { get; set; }
        [Inject] private IMLModelService? MLModelService { get; set; }

        [Parameter] public string? RequiredPermission { get; set; }
        [Parameter] public bool EnableAudit { get; set; } = true;
        [Parameter] public bool UseRealAI { get; set; } = false;
        [Parameter] public string? AIModelEndpoint { get; set; }
        [Parameter] public string? AIApiKey { get; set; }
        [Parameter] public bool EnablePHIDetection { get; set; } = true;
        [Parameter] public string? UserId { get; set; }

        private bool HasPermission { get; set; }
        private bool IsPredicting { get; set; }

        private string PatientId { get; set; } = string.Empty;
        private string Age { get; set; } = string.Empty;
        private string AdmissionType { get; set; } = string.Empty;
        private string PrimaryDiagnosis { get; set; } = string.Empty;
        private string Comorbidities { get; set; } = string.Empty;
        private string VitalScore { get; set; } = string.Empty;

        private PatientPrediction? CurrentPrediction { get; set; }
        private PopulationMetrics? PopulationMetrics { get; set; }
        private List<ModelPerformance>? ModelMetrics { get; set; }
        private List<HighRiskPatient>? HighRiskPatients { get; set; }

        protected override async Task OnInitializedAsync()
        {
            HasPermission = string.IsNullOrEmpty(RequiredPermission) || 
                          await PermissionService.HasPermissionAsync(RequiredPermission);
            
            if (EnableAudit && HasPermission)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "Accessed",
                    Resource = "PredictiveAnalytics",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object> { ["Details"] = "User accessed Predictive Analytics system" }
                });
            }

            InitializeModelMetrics();
            InitializeHighRiskPatients();
        }

        private async Task GeneratePredictions()
        {
            if (string.IsNullOrWhiteSpace(PatientId))
            {
                return;
            }

            IsPredicting = true;
            StateHasChanged();

            try
            {
                // Check if we should use real AI services
                var useRealAI = UseRealAI && 
                               (AIService != null || MLModelService != null) &&
                               !string.IsNullOrWhiteSpace(AIModelEndpoint) && 
                               !string.IsNullOrWhiteSpace(AIApiKey);

                if (useRealAI)
                {
                    await GeneratePredictionsWithRealAI();
                }
                else
                {
                    await GeneratePredictionsWithMockAI();
                }

                if (EnableAudit)
                {
                    await AuditService.LogAsync(new AuditMetadata
                    {
                        Action = "PredictionGenerated",
                        Resource = "PredictiveAnalytics",
                        UserId = UserId,
                        AdditionalData = new Dictionary<string, object>
                        {
                            ["PatientId"] = PatientId ?? "",
                            ["AIType"] = useRealAI ? "Real AI" : "Mock AI"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Prediction Error: {ex.Message}");
                // Fallback to mock AI
                await GeneratePredictionsWithMockAI();
            }
            finally
            {
                IsPredicting = false;
                StateHasChanged();
            }
        }

        private async Task GeneratePredictionsWithRealAI()
        {
            // Prepare patient data with PHI detection
            var patientData = $"Patient: {PatientId}, Age: {Age}, Admission: {AdmissionType}, Diagnosis: {PrimaryDiagnosis}, Comorbidities: {Comorbidities}, Vital Score: {VitalScore}";
            
            var processedData = patientData;
            if (EnablePHIDetection && ComplianceService != null)
            {
                var phiElements = await ComplianceService.DetectPHIAsync(patientData);
                if (phiElements.Any())
                {
                    processedData = await ComplianceService.DeIdentifyAsync(patientData, phiElements);
                }
            }

            // Try ML.NET models first (if available)
            if (MLModelService != null)
            {
                var mlPrediction = await GenerateMLPrediction(processedData);
                if (mlPrediction != null)
                {
                    CurrentPrediction = mlPrediction;
                    
                    // Audit AI operation
                    if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
                    {
                        await ComplianceService.AuditAIOperationAsync(
                            "PredictiveAnalytics",
                            UserId,
                            processedData,
                            $"Readmission: {mlPrediction.ReadmissionRisk?.Probability}%, LOS: {mlPrediction.LengthOfStay?.PredictedDays} days");
                    }
                    return;
                }
            }

            // Fallback to Azure OpenAI for analysis
            if (AIService != null && !string.IsNullOrWhiteSpace(AIModelEndpoint) && !string.IsNullOrWhiteSpace(AIApiKey))
            {
                var context = "Analyze patient data and predict healthcare outcomes:";
                var response = await AIService.GenerateResponseAsync(context, processedData);

                // Parse AI response and create prediction
                CurrentPrediction = ParseAIPrediction(response);
                
                // Audit AI operation
                if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
                {
                    await ComplianceService.AuditAIOperationAsync(
                        "PredictiveAnalytics",
                        UserId,
                        processedData,
                        response);
                }
            }
        }

        private async Task<PatientPrediction?> GenerateMLPrediction(string patientData)
        {
            if (MLModelService == null) return null;

            try
            {
                // Prepare input features
                var features = new Dictionary<string, object>
                {
                    ["Age"] = int.TryParse(Age, out var age) ? age : 50,
                    ["AdmissionType"] = AdmissionType,
                    ["PrimaryDiagnosis"] = PrimaryDiagnosis,
                    ["Comorbidities"] = Comorbidities,
                    ["VitalScore"] = double.TryParse(VitalScore, out var vital) ? vital : 75.0
                };

                // Get predictions from ML models
                var readmissionTask = MLModelService.PredictAsync("readmission-risk", features);
                var losTask = MLModelService.PredictAsync("length-of-stay", features);
                var mortalityTask = MLModelService.PredictAsync("mortality-risk", features);

                await Task.WhenAll(readmissionTask, losTask, mortalityTask);

                var readmissionResult = await readmissionTask;
                var losResult = await losTask;
                var mortalityResult = await mortalityTask;

                return new PatientPrediction
                {
                    PatientId = PatientId,
                    PredictionTimestamp = DateTime.UtcNow,
                    ReadmissionRisk = new RiskPrediction
                    {
                        Probability = readmissionResult.Score * 100,
                        RiskLevel = GetRiskLevel(readmissionResult.Score),
                        ConfidenceInterval = $"(±{readmissionResult.Confidence:F1}%)"
                    },
                    MortalityRisk = new RiskPrediction
                    {
                        Probability = mortalityResult.Score * 100,
                        RiskLevel = GetRiskLevel(mortalityResult.Score),
                        ConfidenceInterval = $"(±{mortalityResult.Confidence:F1}%)"
                    },
                    LengthOfStay = new LOSPrediction
                    {
                        PredictedDays = losResult.Score,
                        ConfidenceInterval = losResult.Confidence,
                        AverageDays = 5.8
                    },
                    ComplicationRisk = new RiskPrediction
                    {
                        Probability = (readmissionResult.Score + mortalityResult.Score) * 50,
                        RiskLevel = GetRiskLevel((readmissionResult.Score + mortalityResult.Score) / 2),
                        ConfidenceInterval = "(±3.5%)"
                    },
                    ModelUsed = "ML.NET",
                    ModelVersion = "1.0",
                    ModelAccuracy = readmissionResult.Confidence
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Prediction Error: {ex.Message}");
                return null;
            }
        }

        private string GetRiskLevel(double score)
        {
            if (score > 0.7) return "High";
            if (score > 0.4) return "Moderate";
            return "Low";
        }

        private PatientPrediction ParseAIPrediction(string aiResponse)
        {
            // Simple parsing logic - in production, use structured JSON responses
            var isHighRisk = aiResponse.ToLower().Contains("high risk") || 
                           aiResponse.ToLower().Contains("severe");

            return new PatientPrediction
            {
                PatientId = PatientId,
                PredictionTimestamp = DateTime.UtcNow,
                ReadmissionRisk = new RiskPrediction
                {
                    Probability = isHighRisk ? 34.5 : 18.2,
                    RiskLevel = isHighRisk ? "High" : "Moderate",
                    ConfidenceInterval = "(±4.2%)"
                },
                MortalityRisk = new RiskPrediction
                {
                    Probability = isHighRisk ? 12.8 : 4.3,
                    RiskLevel = isHighRisk ? "Moderate" : "Low",
                    ConfidenceInterval = "(±2.1%)"
                },
                LengthOfStay = new LOSPrediction
                {
                    PredictedDays = isHighRisk ? 7.2 : 4.5,
                    ConfidenceInterval = 1.5,
                    AverageDays = 5.8
                },
                ComplicationRisk = new RiskPrediction
                {
                    Probability = isHighRisk ? 28.7 : 14.3,
                    RiskLevel = isHighRisk ? "High" : "Moderate",
                    ConfidenceInterval = "(±3.5%)"
                },
                ModelUsed = "Azure OpenAI GPT-4",
                ModelVersion = "1.0",
                ModelAccuracy = 0.85
            };
        }

        private async Task GeneratePredictionsWithMockAI()
        {
            await Task.Delay(2000); // Simulate AI processing
            CurrentPrediction = GeneratePrediction();
        }

        private PatientPrediction GeneratePrediction()
        {
            var isHighRisk = PrimaryDiagnosis.ToLower().Contains("heart") || 
                           PrimaryDiagnosis.ToLower().Contains("failure") ||
                           Comorbidities.ToLower().Contains("diabetes");

            var prediction = new PatientPrediction
            {
                PatientId = PatientId,
                PredictionTimestamp = DateTime.UtcNow,
                ReadmissionRisk = new RiskPrediction
                {
                    Probability = isHighRisk ? 34.5 : 18.2,
                    RiskLevel = isHighRisk ? "High" : "Moderate",
                    ConfidenceInterval = "(±4.2%)"
                },
                MortalityRisk = new RiskPrediction
                {
                    Probability = isHighRisk ? 12.8 : 4.3,
                    RiskLevel = isHighRisk ? "Moderate" : "Low",
                    ConfidenceInterval = "(±2.1%)"
                },
                LengthOfStay = new LOSPrediction
                {
                    PredictedDays = isHighRisk ? 7.2 : 4.5,
                    ConfidenceInterval = 1.5,
                    AverageDays = 5.8
                },
                ComplicationRisk = new RiskPrediction
                {
                    Probability = isHighRisk ? 28.7 : 14.3,
                    RiskLevel = isHighRisk ? "High" : "Moderate",
                    ConfidenceInterval = "(±3.5%)"
                }
            };

            // Risk factors
            prediction.RiskFactors.Add(new RiskFactor
            {
                Factor = "Age > 65 years",
                Impact = "High",
                Contribution = 24.5,
                Description = "Advanced age significantly increases readmission and complication risk"
            });

            prediction.RiskFactors.Add(new RiskFactor
            {
                Factor = "Heart Failure History",
                Impact = "High",
                Contribution = 22.3,
                Description = "Prior heart failure episodes are strong predictors of readmission"
            });

            prediction.RiskFactors.Add(new RiskFactor
            {
                Factor = "Emergency Admission",
                Impact = "Moderate",
                Contribution = 18.7,
                Description = "Unplanned admissions have higher complication rates than elective"
            });

            prediction.RiskFactors.Add(new RiskFactor
            {
                Factor = "Multiple Comorbidities",
                Impact = "Moderate",
                Contribution = 15.2,
                Description = "Diabetes and hypertension complicate recovery and management"
            });

            prediction.RiskFactors.Add(new RiskFactor
            {
                Factor = "Abnormal Vital Signs",
                Impact = "Moderate",
                Contribution = 12.8,
                Description = "Elevated heart rate and blood pressure indicate physiological stress"
            });

            prediction.RiskFactors.Add(new RiskFactor
            {
                Factor = "Previous Admissions",
                Impact = "Low",
                Contribution = 6.5,
                Description = "History of multiple admissions in past 6 months"
            });

            // Interventions
            prediction.RecommendedInterventions.Add(new Intervention
            {
                Title = "Intensive Discharge Planning",
                Description = "Implement comprehensive discharge planning with patient education, medication reconciliation, and follow-up scheduling before discharge.",
                Priority = "High",
                ExpectedReduction = 8.5,
                EvidenceLevel = "Strong (Level A)"
            });

            prediction.RecommendedInterventions.Add(new Intervention
            {
                Title = "Post-Discharge Telehealth",
                Description = "Schedule telehealth check-in within 48-72 hours post-discharge to assess symptoms, medication adherence, and early warning signs.",
                Priority = "High",
                ExpectedReduction = 6.3,
                EvidenceLevel = "Strong (Level A)"
            });

            prediction.RecommendedInterventions.Add(new Intervention
            {
                Title = "Care Transition Coordinator",
                Description = "Assign dedicated care coordinator to manage transition from hospital to home, ensuring all services and medications are in place.",
                Priority = "Medium",
                ExpectedReduction = 5.7,
                EvidenceLevel = "Moderate (Level B)"
            });

            prediction.RecommendedInterventions.Add(new Intervention
            {
                Title = "Home Health Services",
                Description = "Arrange home health nursing visits for wound care, vital sign monitoring, and medication management support.",
                Priority = "Medium",
                ExpectedReduction = 4.2,
                EvidenceLevel = "Moderate (Level B)"
            });

            prediction.RecommendedInterventions.Add(new Intervention
            {
                Title = "Patient Education Materials",
                Description = "Provide written and video education materials on disease management, symptom recognition, and when to seek care.",
                Priority = "Low",
                ExpectedReduction = 2.8,
                EvidenceLevel = "Moderate (Level B)"
            });

            return prediction;
        }

        private async Task LoadPopulationData()
        {
            await Task.Delay(1500);

            PopulationMetrics = new PopulationMetrics
            {
                TotalPatients = 1247,
                HighRiskCount = 189,
                HighRiskPercent = 15.2,
                AverageLOS = 5.8,
                PredictedCosts = "8.4M",
                RiskDistribution = new List<PredictiveChartDataPoint>
                {
                    new PredictiveChartDataPoint { Category = "Low Risk", Count = 623 },
                    new PredictiveChartDataPoint { Category = "Moderate", Count = 435 },
                    new PredictiveChartDataPoint { Category = "High Risk", Count = 156 },
                    new PredictiveChartDataPoint { Category = "Critical", Count = 33 }
                },
                ResourceForecast = new List<ResourceDataPoint>
                {
                    new ResourceDataPoint { Period = "Week 1", Utilization = 78 },
                    new ResourceDataPoint { Period = "Week 2", Utilization = 82 },
                    new ResourceDataPoint { Period = "Week 3", Utilization = 85 },
                    new ResourceDataPoint { Period = "Week 4", Utilization = 88 },
                    new ResourceDataPoint { Period = "Week 5", Utilization = 84 },
                    new ResourceDataPoint { Period = "Week 6", Utilization = 79 }
                }
            };

            StateHasChanged();
        }

        private void InitializeModelMetrics()
        {
            ModelMetrics = new List<ModelPerformance>
            {
                new ModelPerformance
                {
                    ModelName = "Readmission Risk Predictor",
                    Status = "Active",
                    Accuracy = 87.3,
                    Precision = 84.6,
                    Recall = 89.2,
                    F1Score = 86.8,
                    AUCROC = 0.912,
                    Algorithm = "Gradient Boosting (XGBoost)",
                    LastTrainingDate = DateTime.UtcNow.AddDays(-15),
                    TrainingSampleSize = 45782
                },
                new ModelPerformance
                {
                    ModelName = "Mortality Risk Predictor",
                    Status = "Active",
                    Accuracy = 92.1,
                    Precision = 90.4,
                    Recall = 88.7,
                    F1Score = 89.5,
                    AUCROC = 0.943,
                    Algorithm = "Deep Neural Network",
                    LastTrainingDate = DateTime.UtcNow.AddDays(-8),
                    TrainingSampleSize = 38945
                },
                new ModelPerformance
                {
                    ModelName = "Length of Stay Predictor",
                    Status = "Active",
                    Accuracy = 81.5,
                    Precision = 79.8,
                    Recall = 83.2,
                    F1Score = 81.5,
                    AUCROC = 0.867,
                    Algorithm = "Random Forest",
                    LastTrainingDate = DateTime.UtcNow.AddDays(-22),
                    TrainingSampleSize = 52341
                },
                new ModelPerformance
                {
                    ModelName = "Complication Risk Predictor",
                    Status = "Active",
                    Accuracy = 85.7,
                    Precision = 83.9,
                    Recall = 87.1,
                    F1Score = 85.5,
                    AUCROC = 0.895,
                    Algorithm = "Ensemble (XGBoost + Neural Net)",
                    LastTrainingDate = DateTime.UtcNow.AddDays(-12),
                    TrainingSampleSize = 41256
                }
            };
        }

        private void InitializeHighRiskPatients()
        {
            HighRiskPatients = new List<HighRiskPatient>
            {
                new HighRiskPatient
                {
                    PatientId = "PT-8821",
                    PatientName = "Johnson, Robert",
                    Diagnosis = "Acute Heart Failure",
                    ReadmissionRisk = 42.3,
                    MortalityRisk = 15.7,
                    OverallRisk = "Critical",
                    DaysInHospital = 8
                },
                new HighRiskPatient
                {
                    PatientId = "PT-7654",
                    PatientName = "Williams, Mary",
                    Diagnosis = "Sepsis",
                    ReadmissionRisk = 38.9,
                    MortalityRisk = 18.2,
                    OverallRisk = "Critical",
                    DaysInHospital = 12
                },
                new HighRiskPatient
                {
                    PatientId = "PT-9432",
                    PatientName = "Davis, James",
                    Diagnosis = "COPD Exacerbation",
                    ReadmissionRisk = 36.4,
                    MortalityRisk = 8.9,
                    OverallRisk = "High",
                    DaysInHospital = 6
                },
                new HighRiskPatient
                {
                    PatientId = "PT-5523",
                    PatientName = "Martinez, Linda",
                    Diagnosis = "Pneumonia",
                    ReadmissionRisk = 33.1,
                    MortalityRisk = 7.2,
                    OverallRisk = "High",
                    DaysInHospital = 5
                },
                new HighRiskPatient
                {
                    PatientId = "PT-6789",
                    PatientName = "Anderson, Patricia",
                    Diagnosis = "Diabetic Complications",
                    ReadmissionRisk = 31.5,
                    MortalityRisk = 5.4,
                    OverallRisk = "High",
                    DaysInHospital = 7
                },
                new HighRiskPatient
                {
                    PatientId = "PT-4412",
                    PatientName = "Thompson, Michael",
                    Diagnosis = "Post-Surgical Infection",
                    ReadmissionRisk = 29.8,
                    MortalityRisk = 6.7,
                    OverallRisk = "High",
                    DaysInHospital = 9
                },
                new HighRiskPatient
                {
                    PatientId = "PT-3398",
                    PatientName = "Garcia, Elizabeth",
                    Diagnosis = "Acute Kidney Injury",
                    ReadmissionRisk = 28.3,
                    MortalityRisk = 9.1,
                    OverallRisk = "High",
                    DaysInHospital = 10
                }
            };
        }

        private string GetRiskColor(string riskLevel)
        {
            return riskLevel.ToLower() switch
            {
                "critical" => "#c0392b",
                "high" => "#e74c3c",
                "moderate" => "#f39c12",
                "medium" => "#f39c12",
                "low" => "#27ae60",
                _ => "#95a5a6"
            };
        }

        private string GetRiskLevelFromPercent(double percent)
        {
            if (percent >= 35) return "Critical";
            if (percent >= 25) return "High";
            if (percent >= 15) return "Moderate";
            return "Low";
        }
    }

    public class PatientPrediction
    {
        public string PatientId { get; set; } = string.Empty;
        public DateTime PredictionTimestamp { get; set; }
        public RiskPrediction ReadmissionRisk { get; set; } = new();
        public RiskPrediction MortalityRisk { get; set; } = new();
        public LOSPrediction LengthOfStay { get; set; } = new();
        public RiskPrediction ComplicationRisk { get; set; } = new();
        public List<RiskFactor> RiskFactors { get; set; } = new();
        public List<Intervention> RecommendedInterventions { get; set; } = new();
        public string ModelUsed { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public double ModelAccuracy { get; set; }
    }

    public class RiskPrediction
    {
        public double Probability { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string ConfidenceInterval { get; set; } = string.Empty;
    }

    public class LOSPrediction
    {
        public double PredictedDays { get; set; }
        public double ConfidenceInterval { get; set; }
        public double AverageDays { get; set; }
    }

    public class RiskFactor
    {
        public string Factor { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public double Contribution { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Intervention
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double ExpectedReduction { get; set; }
        public string EvidenceLevel { get; set; } = string.Empty;
    }

    public class PopulationMetrics
    {
        public int TotalPatients { get; set; }
        public int HighRiskCount { get; set; }
        public double HighRiskPercent { get; set; }
        public double AverageLOS { get; set; }
        public string PredictedCosts { get; set; } = string.Empty;
        public List<PredictiveChartDataPoint> RiskDistribution { get; set; } = new();
        public List<ResourceDataPoint> ResourceForecast { get; set; } = new();
    }

    public class PredictiveChartDataPoint
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ResourceDataPoint
    {
        public string Period { get; set; } = string.Empty;
        public int Utilization { get; set; }
    }

    public class ModelPerformance
    {
        public string ModelName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public double AUCROC { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public DateTime LastTrainingDate { get; set; }
        public int TrainingSampleSize { get; set; }
    }

    public class HighRiskPatient
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public double ReadmissionRisk { get; set; }
        public double MortalityRisk { get; set; }
        public string OverallRisk { get; set; } = string.Empty;
        public int DaysInHospital { get; set; }
    }
}
