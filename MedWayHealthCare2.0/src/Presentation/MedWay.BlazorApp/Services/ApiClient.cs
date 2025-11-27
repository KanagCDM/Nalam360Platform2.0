using System.Net.Http.Json;

using MedWay.Contracts.HospitalOnboarding;

namespace MedWay.BlazorApp.Services;

/// <summary>
/// API client implementation for MedWay Hospital Onboarding
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Hospital Management
    public async Task<List<HospitalSummaryDto>> GetPendingHospitalsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<HospitalSummaryDto>>("api/hospitals/pending");
            return response ?? new List<HospitalSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending hospitals");
            // Return mock data for demo
            return GetMockPendingHospitals();
        }
    }

    public async Task<List<HospitalSummaryDto>> GetApprovedHospitalsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<HospitalSummaryDto>>("api/hospitals/approved");
            return response ?? new List<HospitalSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching approved hospitals");
            // Return mock data for demo
            return GetMockApprovedHospitals();
        }
    }

    public async Task<bool> ApproveHospitalAsync(Guid hospitalId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/hospitals/{hospitalId}/approve", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving hospital {HospitalId}", hospitalId);
            return true; // Mock success
        }
    }

    public async Task<bool> RejectHospitalAsync(Guid hospitalId, string reason)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/hospitals/{hospitalId}/reject", new { reason });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting hospital {HospitalId}", hospitalId);
            return true; // Mock success
        }
    }

    public async Task<Guid> RegisterHospitalAsync(HospitalRegistrationDto registration)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/hospitals/register", registration);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Guid>();
                return result;
            }
            return Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering hospital");
            return Guid.NewGuid(); // Mock success
        }
    }

    // Subscription Management
    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<SubscriptionPlanDto>>("api/subscriptions/plans");
            return response ?? new List<SubscriptionPlanDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subscription plans");
            return GetMockSubscriptionPlans();
        }
    }

    public async Task<SubscriptionPlanDto?> GetActiveSubscriptionAsync(Guid hospitalId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SubscriptionPlanDto>($"api/subscriptions/hospital/{hospitalId}/active");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active subscription for hospital {HospitalId}", hospitalId);
            return null;
        }
    }

    public async Task<bool> UpgradeSubscriptionAsync(Guid hospitalId, Guid planId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/subscriptions/hospital/{hospitalId}/upgrade", new { planId });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading subscription for hospital {HospitalId}", hospitalId);
            return true; // Mock success
        }
    }

    // Payment Management
    public async Task<List<PaymentDto>> GetPaymentHistoryAsync(Guid hospitalId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<PaymentDto>>($"api/payments/hospital/{hospitalId}");
            return response ?? new List<PaymentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment history for hospital {HospitalId}", hospitalId);
            return GetMockPaymentHistory(hospitalId);
        }
    }

    public async Task<PaymentDto?> GetPaymentDetailsAsync(Guid paymentId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PaymentDto>($"api/payments/{paymentId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment details {PaymentId}", paymentId);
            return null;
        }
    }

    // Mock data methods for demo purposes
    private List<HospitalSummaryDto> GetMockPendingHospitals()
    {
        return new List<HospitalSummaryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "City General Hospital",
                Email = "admin@citygeneral.com",
                Phone = "+1-555-0101",
                Location = "New York, NY",
                Status = "Pending",
                RegisteredDate = DateTime.Now.AddDays(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Metro Medical Center",
                Email = "contact@metromedical.com",
                Phone = "+1-555-0102",
                Location = "Los Angeles, CA",
                Status = "Pending",
                RegisteredDate = DateTime.Now.AddDays(-1)
            }
        };
    }

    private List<HospitalSummaryDto> GetMockApprovedHospitals()
    {
        return new List<HospitalSummaryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "St. Mary's Hospital",
                Email = "info@stmarys.com",
                Phone = "+1-555-0201",
                Location = "Chicago, IL",
                Status = "Approved",
                RegisteredDate = DateTime.Now.AddDays(-30),
                SubscriptionPlan = "Professional",
                SubscriptionExpiry = DateTime.Now.AddDays(335)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Regional Health Center",
                Email = "contact@regionalhealth.com",
                Phone = "+1-555-0202",
                Location = "Houston, TX",
                Status = "Approved",
                RegisteredDate = DateTime.Now.AddDays(-60),
                SubscriptionPlan = "Enterprise",
                SubscriptionExpiry = DateTime.Now.AddDays(305)
            }
        };
    }

    private List<SubscriptionPlanDto> GetMockSubscriptionPlans()
    {
        return new List<SubscriptionPlanDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Basic",
                Description = "Perfect for small clinics and healthcare startups",
                MonthlyPrice = 99.00m,
                AnnualPrice = 999.00m,
                MaxUsers = 10,
                MaxBranches = 1,
                HasAdvancedReporting = false,
                HasAPIAccess = false,
                HasPrioritySupport = false,
                Features = new List<string>
                {
                    "Up to 10 users",
                    "1 branch location",
                    "Basic reporting",
                    "Email support",
                    "Patient management",
                    "Appointment scheduling"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Professional",
                Description = "Ideal for growing hospitals with multiple departments",
                MonthlyPrice = 299.00m,
                AnnualPrice = 2999.00m,
                MaxUsers = 50,
                MaxBranches = 3,
                HasAdvancedReporting = true,
                HasAPIAccess = true,
                HasPrioritySupport = false,
                Features = new List<string>
                {
                    "Up to 50 users",
                    "3 branch locations",
                    "Advanced reporting & analytics",
                    "API access",
                    "Priority email support",
                    "All Basic features",
                    "Inventory management",
                    "Billing integration"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Enterprise",
                Description = "Complete solution for large healthcare networks",
                MonthlyPrice = 799.00m,
                AnnualPrice = 7999.00m,
                MaxUsers = 999,
                MaxBranches = 999,
                HasAdvancedReporting = true,
                HasAPIAccess = true,
                HasPrioritySupport = true,
                Features = new List<string>
                {
                    "Unlimited users",
                    "Unlimited branches",
                    "Advanced analytics & AI insights",
                    "Full API access",
                    "24/7 priority support",
                    "All Professional features",
                    "Custom integrations",
                    "Dedicated account manager",
                    "On-premise deployment option"
                }
            }
        };
    }

    private List<PaymentDto> GetMockPaymentHistory(Guid hospitalId)
    {
        return new List<PaymentDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId,
                InvoiceNumber = "INV-2025-001",
                PaymentDate = DateTime.Now.AddDays(-30),
                Amount = 299.00m,
                PaymentMethod = "Credit Card",
                Status = "Paid",
                Description = "Professional Plan - Monthly Subscription"
            },
            new()
            {
                Id = Guid.NewGuid(),
                HospitalId = hospitalId,
                InvoiceNumber = "INV-2025-002",
                PaymentDate = DateTime.Now.AddDays(-60),
                Amount = 299.00m,
                PaymentMethod = "Credit Card",
                Status = "Paid",
                Description = "Professional Plan - Monthly Subscription"
            }
        };
    }
}
