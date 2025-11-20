using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Nalam360Enterprise.UI.Core.Security;
using Nalam360Enterprise.UI.Core.AI.Services;
using Nalam360Enterprise.UI.Core.AI.Models;

namespace Nalam360Enterprise.UI.Components.AI
{
    public partial class N360PatientEngagement : ComponentBase
    {
        [Inject] private IAIService? AIService { get; set; }
        [Inject] private IAIComplianceService? ComplianceService { get; set; }
        [Inject] private IMLModelService? MLModelService { get; set; }

        [Parameter] public string? RequiredPermission { get; set; } = "PatientEngagement.View";
        [Parameter] public bool EnableAudit { get; set; } = true;
        
        // AI Features
        [Parameter] public bool UseRealAI { get; set; } = false;
        [Parameter] public string? AIModelEndpoint { get; set; }
        [Parameter] public string? AIApiKey { get; set; }
        [Parameter] public bool EnablePHIDetection { get; set; } = true;
        [Parameter] public string? UserId { get; set; }

        private bool HasPermission { get; set; }
        private bool IsSendingMessage { get; set; }

        // Chat
        private string SelectedPatientId { get; set; } = "P-2024-8945";
        private string CurrentMessage { get; set; } = string.Empty;
        private List<ChatMessage> ChatMessages { get; set; } = new();
        private ElementReference chatMessagesRef;

        // Data
        private List<PatientProfile> Patients { get; set; } = new();
        private List<MedicationRecord> MedicationRecords { get; set; } = new();
        private List<EducationalContent> EducationalContent { get; set; } = new();
        private List<Appointment> Appointments { get; set; } = new();

        // Analytics
        private EngagementStatistics Statistics { get; set; } = new();
        private List<DailyActiveUser> DailyActiveUsers { get; set; } = new();
        private List<MessageVolume> MessageVolumeByHour { get; set; } = new();
        private List<ContentType> ContentTypeDistribution { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            HasPermission = await PermissionService.HasPermissionAsync(RequiredPermission ?? "PatientEngagement.View");

            if (HasPermission)
            {
                InitializeSampleData();
            }

            await base.OnInitializedAsync();
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage))
                return;

            IsSendingMessage = true;

            // Add patient message
            ChatMessages.Add(new ChatMessage
            {
                Sender = "Patient",
                Message = CurrentMessage,
                Timestamp = DateTime.Now,
                IsPatient = true
            });

            var userMessage = CurrentMessage;
            CurrentMessage = string.Empty;
            StateHasChanged();

            if (EnableAudit)
            {
                await AuditService.LogAsync(new AuditMetadata
                {
                    Action = "SendMessage",
                    Resource = "PatientEngagement",
                    UserId = UserId,
                    AdditionalData = new Dictionary<string, object> { ["PatientId"] = SelectedPatientId ?? "" }
                });
            }

            try
            {
                ChatMessage aiResponse;
                if (UseRealAI && AIService != null && !string.IsNullOrWhiteSpace(AIModelEndpoint) && !string.IsNullOrWhiteSpace(AIApiKey))
                {
                    aiResponse = await ProcessWithRealAI(userMessage);
                }
                else
                {
                    await Task.Delay(1500);
                    aiResponse = GenerateAIResponse(userMessage);
                }
                
                ChatMessages.Add(aiResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Processing Error: {ex.Message}");
                var fallbackResponse = GenerateAIResponse(userMessage);
                ChatMessages.Add(fallbackResponse);
            }

            IsSendingMessage = false;
            StateHasChanged();
        }

        private async Task<ChatMessage> ProcessWithRealAI(string userMessage)
        {
            var processedMessage = userMessage;

            if (EnablePHIDetection && ComplianceService != null)
            {
                var phiElements = await ComplianceService.DetectPHIAsync(userMessage);
                if (phiElements.Any())
                {
                    processedMessage = await ComplianceService.DeIdentifyAsync(userMessage, phiElements);
                }
            }

            if (AIService != null)
            {
                var response = await AIService.GenerateResponseAsync(
                    string.Join("\n", ChatMessages.Select(m => $"{m.Sender}: {m.Message}")),
                    processedMessage);
                
                if (!string.IsNullOrEmpty(response))
                {
                    if (ComplianceService != null && !string.IsNullOrWhiteSpace(UserId))
                    {
                        await ComplianceService.AuditAIOperationAsync(
                            "PatientEngagement",
                            UserId,
                            processedMessage,
                            response);
                    }

                    return new ChatMessage
                    {
                        Sender = "AI Assistant",
                        Message = response,
                        Timestamp = DateTime.Now,
                        IsPatient = false,
                        Intent = "AI Generated",
                        Confidence = 95.0
                    };
                }
            }

            return GenerateAIResponse(userMessage);
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(CurrentMessage))
            {
                await SendMessage();
            }
        }

        private void HandleSuggestedAction(string action)
        {
            CurrentMessage = action;
            StateHasChanged();
        }

        private ChatMessage GenerateAIResponse(string userMessage)
        {
            var messageLower = userMessage.ToLower();
            
            if (messageLower.Contains("appointment") || messageLower.Contains("schedule"))
            {
                return new ChatMessage
                {
                    Sender = "AI Assistant",
                    Message = "I can help you schedule an appointment. You have an upcoming appointment on November 25th at 2:00 PM with Dr. Sarah Johnson for your follow-up visit. Would you like to reschedule or need directions to the clinic?",
                    Timestamp = DateTime.Now,
                    IsPatient = false,
                    Intent = "Appointment Scheduling",
                    Confidence = 94.5,
                    SuggestedActions = new List<string> { "View appointment details", "Get directions", "Reschedule appointment" }
                };
            }
            else if (messageLower.Contains("medication") || messageLower.Contains("pill") || messageLower.Contains("medicine"))
            {
                return new ChatMessage
                {
                    Sender = "AI Assistant",
                    Message = "I see you're asking about your medications. You're currently taking Metformin 1000mg twice daily with an 87% adherence rate. Your next dose is due today at 6:00 PM. Would you like to set a reminder?",
                    Timestamp = DateTime.Now,
                    IsPatient = false,
                    Intent = "Medication Query",
                    Confidence = 96.2,
                    SuggestedActions = new List<string> { "Set reminder", "View medication list", "Report side effects" }
                };
            }
            else if (messageLower.Contains("pain") || messageLower.Contains("hurt") || messageLower.Contains("symptom"))
            {
                return new ChatMessage
                {
                    Sender = "AI Assistant",
                    Message = "I'm sorry to hear you're experiencing pain. Can you rate your pain on a scale of 1-10? Also, please let me know: When did it start? Where is the pain located? Have you taken any medication for it?",
                    Timestamp = DateTime.Now,
                    IsPatient = false,
                    Intent = "Symptom Assessment",
                    Confidence = 91.8,
                    SuggestedActions = new List<string> { "Rate pain 1-10", "Describe location", "Contact provider" }
                };
            }
            else if (messageLower.Contains("result") || messageLower.Contains("test") || messageLower.Contains("lab"))
            {
                return new ChatMessage
                {
                    Sender = "AI Assistant",
                    Message = "Your recent lab results from November 10th are available in your portal. Your HbA1c is 6.8%, which shows good diabetes control. Your provider will discuss these results at your upcoming appointment. Would you like me to explain what these numbers mean?",
                    Timestamp = DateTime.Now,
                    IsPatient = false,
                    Intent = "Lab Results Query",
                    Confidence = 93.7,
                    SuggestedActions = new List<string> { "View full results", "Explain results", "Message provider" }
                };
            }
            else if (messageLower.Contains("insurance") || messageLower.Contains("bill") || messageLower.Contains("cost"))
            {
                return new ChatMessage
                {
                    Sender = "AI Assistant",
                    Message = "I can help with billing and insurance questions. Your current insurance is Blue Cross PPO, and your last visit copay was $35. Would you like to view your billing history or speak with our billing department?",
                    Timestamp = DateTime.Now,
                    IsPatient = false,
                    Intent = "Billing/Insurance",
                    Confidence = 88.4,
                    SuggestedActions = new List<string> { "View billing history", "Contact billing", "Update insurance" }
                };
            }
            else
            {
                return new ChatMessage
                {
                    Sender = "AI Assistant",
                    Message = "Thank you for your message. I'm here to help with appointments, medications, symptoms, test results, and billing questions. How can I assist you today?",
                    Timestamp = DateTime.Now,
                    IsPatient = false,
                    Intent = "General Inquiry",
                    Confidence = 75.3,
                    SuggestedActions = new List<string> { "Schedule appointment", "Medication questions", "View test results", "Billing inquiry" }
                };
            }
        }

        private void InitializeSampleData()
        {
            // Initialize patients
            Patients = new List<PatientProfile>
            {
                new PatientProfile { PatientId = "P-2024-8945", PatientName = "Sarah Johnson", EngagementScore = 92, MessagesCount = 47, ContentViewed = 12, LastActive = DateTime.Now.AddHours(-2) },
                new PatientProfile { PatientId = "P-2024-8946", PatientName = "Michael Chen", EngagementScore = 88, MessagesCount = 38, ContentViewed = 9, LastActive = DateTime.Now.AddHours(-5) },
                new PatientProfile { PatientId = "P-2024-8947", PatientName = "Emily Rodriguez", EngagementScore = 85, MessagesCount = 42, ContentViewed = 11, LastActive = DateTime.Now.AddDays(-1) },
                new PatientProfile { PatientId = "P-2024-8948", PatientName = "David Kim", EngagementScore = 79, MessagesCount = 31, ContentViewed = 7, LastActive = DateTime.Now.AddDays(-2) },
                new PatientProfile { PatientId = "P-2024-8949", PatientName = "Lisa Anderson", EngagementScore = 76, MessagesCount = 28, ContentViewed = 8, LastActive = DateTime.Now.AddDays(-3) }
            };

            // Initialize chat messages
            ChatMessages = new List<ChatMessage>
            {
                new ChatMessage { Sender = "AI Assistant", Message = "Hello! I'm your AI health assistant. How can I help you today?", Timestamp = DateTime.Now.AddMinutes(-10), IsPatient = false, Intent = "Greeting", Confidence = 99.0 },
                new ChatMessage { Sender = "Patient", Message = "Hi, I wanted to check on my upcoming appointment", Timestamp = DateTime.Now.AddMinutes(-9), IsPatient = true },
                new ChatMessage { Sender = "AI Assistant", Message = "Of course! You have an appointment scheduled for November 25th at 2:00 PM with Dr. Sarah Johnson for your follow-up visit. Would you like more details?", Timestamp = DateTime.Now.AddMinutes(-8), IsPatient = false, Intent = "Appointment Query", Confidence = 95.5, SuggestedActions = new List<string> { "View details", "Reschedule", "Get directions" } }
            };

            // Initialize medication records
            MedicationRecords = new List<MedicationRecord>
            {
                new MedicationRecord
                {
                    MedicationName = "Metformin",
                    Dosage = "1000mg",
                    Frequency = "Twice daily",
                    DosesTaken = 52,
                    TotalDoses = 60,
                    MissedDoses = 8,
                    AdherenceRate = 86.7,
                    LastTaken = DateTime.Now.AddHours(-8),
                    NextDue = DateTime.Now.AddHours(4),
                    AdherencePattern = Enumerable.Range(0, 7).Select(i => new DayAdherence { Date = DateTime.Now.AddDays(-i), Taken = i != 2 && i != 5 }).ToList()
                },
                new MedicationRecord
                {
                    MedicationName = "Lisinopril",
                    Dosage = "10mg",
                    Frequency = "Once daily",
                    DosesTaken = 28,
                    TotalDoses = 30,
                    MissedDoses = 2,
                    AdherenceRate = 93.3,
                    LastTaken = DateTime.Now.AddHours(-10),
                    NextDue = DateTime.Now.AddHours(14),
                    AdherencePattern = Enumerable.Range(0, 7).Select(i => new DayAdherence { Date = DateTime.Now.AddDays(-i), Taken = i != 4 }).ToList()
                },
                new MedicationRecord
                {
                    MedicationName = "Atorvastatin",
                    Dosage = "40mg",
                    Frequency = "Once daily at bedtime",
                    DosesTaken = 27,
                    TotalDoses = 30,
                    MissedDoses = 3,
                    AdherenceRate = 90.0,
                    LastTaken = DateTime.Now.AddHours(-12),
                    NextDue = DateTime.Now.AddHours(12),
                    AdherencePattern = Enumerable.Range(0, 7).Select(i => new DayAdherence { Date = DateTime.Now.AddDays(-i), Taken = i != 1 && i != 6 }).ToList()
                },
                new MedicationRecord
                {
                    MedicationName = "Aspirin",
                    Dosage = "81mg",
                    Frequency = "Once daily",
                    DosesTaken = 25,
                    TotalDoses = 30,
                    MissedDoses = 5,
                    AdherenceRate = 83.3,
                    LastTaken = DateTime.Now.AddHours(-15),
                    NextDue = DateTime.Now.AddHours(9),
                    AdherencePattern = Enumerable.Range(0, 7).Select(i => new DayAdherence { Date = DateTime.Now.AddDays(-i), Taken = i != 0 && i != 3 && i != 5 }).ToList()
                }
            };

            // Initialize educational content
            EducationalContent = new List<EducationalContent>
            {
                new EducationalContent { Title = "Understanding Diabetes Management", ContentType = "Video", Duration = "12 min", Difficulty = "Beginner", Description = "Learn the basics of managing type 2 diabetes through diet, exercise, and medication.", ViewCount = 1247, Rating = 4.8 },
                new EducationalContent { Title = "Heart-Healthy Diet Guide", ContentType = "Article", Duration = "8 min read", Difficulty = "Beginner", Description = "Comprehensive guide to eating for heart health with meal planning tips.", ViewCount = 892, Rating = 4.6 },
                new EducationalContent { Title = "Blood Pressure Monitoring at Home", ContentType = "Interactive", Duration = "15 min", Difficulty = "Intermediate", Description = "Interactive tutorial on properly measuring and tracking your blood pressure.", ViewCount = 1534, Rating = 4.9 },
                new EducationalContent { Title = "Medication Side Effects: What to Watch For", ContentType = "Infographic", Duration = "5 min", Difficulty = "Beginner", Description = "Visual guide to common medication side effects and when to contact your doctor.", ViewCount = 2108, Rating = 4.7 },
                new EducationalContent { Title = "Living Well with Chronic Conditions", ContentType = "Video", Duration = "18 min", Difficulty = "Intermediate", Description = "Expert advice on maintaining quality of life while managing chronic health conditions.", ViewCount = 976, Rating = 4.5 },
                new EducationalContent { Title = "Understanding Your Lab Results", ContentType = "Article", Duration = "10 min read", Difficulty = "Intermediate", Description = "Decode common lab tests and what the numbers mean for your health.", ViewCount = 1621, Rating = 4.8 }
            };

            // Initialize appointments
            Appointments = new List<Appointment>
            {
                new Appointment { AppointmentType = "Follow-up Visit", ProviderName = "Dr. Sarah Johnson", DateTime = DateTime.Now.AddDays(6), Location = "Main Clinic - Room 305", Status = "Upcoming", Notes = "Bring recent glucose logs" },
                new Appointment { AppointmentType = "Lab Work", ProviderName = "Laboratory Services", DateTime = DateTime.Now.AddDays(13), Location = "Lab Center - 2nd Floor", Status = "Upcoming", Notes = "Fasting required - no food after midnight" },
                new Appointment { AppointmentType = "Cardiology Consultation", ProviderName = "Dr. Michael Roberts", DateTime = DateTime.Now.AddDays(20), Location = "Cardiology Dept - Suite 410", Status = "Upcoming", Notes = "" },
                new Appointment { AppointmentType = "Annual Physical", ProviderName = "Dr. Sarah Johnson", DateTime = DateTime.Now.AddDays(-30), Location = "Main Clinic - Room 305", Status = "Completed", Notes = "" },
                new Appointment { AppointmentType = "Dental Cleaning", ProviderName = "Dr. Lisa Chen", DateTime = DateTime.Now.AddDays(-45), Location = "Dental Clinic", Status = "Completed", Notes = "" }
            };

            // Initialize statistics
            Statistics = new EngagementStatistics
            {
                TotalPatients = 1247,
                TotalMessages = 15682,
                EngagementRate = 78.5,
                AvgResponseTime = 1.8
            };

            // Daily active users
            DailyActiveUsers = new List<DailyActiveUser>
            {
                new DailyActiveUser { Date = "Mon", Users = 342 },
                new DailyActiveUser { Date = "Tue", Users = 418 },
                new DailyActiveUser { Date = "Wed", Users = 395 },
                new DailyActiveUser { Date = "Thu", Users = 461 },
                new DailyActiveUser { Date = "Fri", Users = 438 },
                new DailyActiveUser { Date = "Sat", Users = 287 },
                new DailyActiveUser { Date = "Sun", Users = 256 }
            };

            // Message volume by hour
            MessageVolumeByHour = new List<MessageVolume>
            {
                new MessageVolume { Hour = "8AM", Messages = 145 },
                new MessageVolume { Hour = "10AM", Messages = 287 },
                new MessageVolume { Hour = "12PM", Messages = 342 },
                new MessageVolume { Hour = "2PM", Messages = 298 },
                new MessageVolume { Hour = "4PM", Messages = 256 },
                new MessageVolume { Hour = "6PM", Messages = 189 },
                new MessageVolume { Hour = "8PM", Messages = 124 }
            };

            // Content type distribution
            ContentTypeDistribution = new List<ContentType>
            {
                new ContentType { Type = "Videos", Count = 42 },
                new ContentType { Type = "Articles", Count = 58 },
                new ContentType { Type = "Infographics", Count = 31 },
                new ContentType { Type = "Interactive", Count = 24 }
            };
        }

        private string GetAdherenceLevel(double adherenceRate)
        {
            if (adherenceRate >= 90) return "excellent";
            if (adherenceRate >= 70) return "good";
            return "poor";
        }

        private string GetContentTypeIcon(string contentType)
        {
            return contentType.ToLower() switch
            {
                "video" => "▶",
                "article" => "📄",
                "infographic" => "📊",
                "interactive" => "🎮",
                _ => "📋"
            };
        }

        private string GetContentIcon(string contentType)
        {
            return contentType.ToLower() switch
            {
                "video" => "fas fa-play-circle",
                "article" => "fas fa-file-alt",
                "infographic" => "fas fa-chart-bar",
                "interactive" => "fas fa-gamepad",
                _ => "fas fa-book"
            };
        }
    }

    // Data models
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsPatient { get; set; }
        public string? Intent { get; set; }
        public double? Confidence { get; set; }
        public List<string>? SuggestedActions { get; set; }
    }

    public class PatientProfile
    {
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public int EngagementScore { get; set; }
        public int MessagesCount { get; set; }
        public int ContentViewed { get; set; }
        public DateTime LastActive { get; set; }
    }

    public class MedicationRecord
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int DosesTaken { get; set; }
        public int TotalDoses { get; set; }
        public int MissedDoses { get; set; }
        public double AdherenceRate { get; set; }
        public DateTime LastTaken { get; set; }
        public DateTime NextDue { get; set; }
        public List<DayAdherence> AdherencePattern { get; set; } = new();
    }

    public class DayAdherence
    {
        public DateTime Date { get; set; }
        public bool Taken { get; set; }
    }

    public class EducationalContent
    {
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public double Rating { get; set; }
    }

    public class Appointment
    {
        public string AppointmentType { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class EngagementStatistics
    {
        public int TotalPatients { get; set; }
        public int TotalMessages { get; set; }
        public double EngagementRate { get; set; }
        public double AvgResponseTime { get; set; }
    }

    public class DailyActiveUser
    {
        public string Date { get; set; } = string.Empty;
        public int Users { get; set; }
    }

    public class MessageVolume
    {
        public string Hour { get; set; } = string.Empty;
        public int Messages { get; set; }
    }

    public class ContentType
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
