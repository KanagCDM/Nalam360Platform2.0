using System.ComponentModel.DataAnnotations;

namespace MedWay.Contracts.HospitalOnboarding;

/// <summary>
/// API client interface for MedWay Hospital Onboarding operations
/// </summary>
public interface IApiClient
{
    // Hospital Management
    Task<List<HospitalSummaryDto>> GetPendingHospitalsAsync();
    Task<List<HospitalSummaryDto>> GetApprovedHospitalsAsync();
    Task<bool> ApproveHospitalAsync(Guid hospitalId);
    Task<bool> RejectHospitalAsync(Guid hospitalId, string reason);
    Task<Guid> RegisterHospitalAsync(HospitalRegistrationDto registration);
    
    // Subscription Management
    Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync();
    Task<SubscriptionPlanDto?> GetActiveSubscriptionAsync(Guid hospitalId);
    Task<bool> UpgradeSubscriptionAsync(Guid hospitalId, Guid planId);
    
    // Payment Management
    Task<List<PaymentDto>> GetPaymentHistoryAsync(Guid hospitalId);
    Task<PaymentDto?> GetPaymentDetailsAsync(Guid paymentId);
}

/// <summary>
/// Hospital summary data transfer object
/// </summary>
public class HospitalSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SubscriptionPlan { get; set; }
}

/// <summary>
/// Subscription plan data transfer object
/// </summary>
public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int MaxUsers { get; set; }
    public int MaxPatients { get; set; }
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; }
    public string? BadgeText { get; set; }
}

/// <summary>
/// Payment data transfer object
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid HospitalId { get; set; }
}

/// <summary>
/// Hospital registration form data transfer object
/// </summary>
public class HospitalRegistrationDto
{
    [Required]
    [StringLength(200, ErrorMessage = "Hospital name must be less than 200 characters")]
    public string HospitalName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, ErrorMessage = "License number must be less than 50 characters")]
    public string LicenseNumber { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string AddressLine1 { get; set; } = string.Empty;
    
    public string? AddressLine2 { get; set; }
    
    [Required]
    public string City { get; set; } = string.Empty;
    
    [Required]
    public string State { get; set; } = string.Empty;
    
    [Required]
    public string ZipCode { get; set; } = string.Empty;
    
    [Required]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string ContactPhone { get; set; } = string.Empty;
    
    [Required]
    public string AdminFirstName { get; set; } = string.Empty;
    
    [Required]
    public string AdminLastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string AdminEmail { get; set; } = string.Empty;
    
    public Guid SelectedPlanId { get; set; }
    public string PaymentMethod { get; set; } = "CreditCard";
    public bool AcceptedTerms { get; set; }
    public bool OptInMarketing { get; set; }
}
