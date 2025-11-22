namespace MedWay.HospitalOnboarding.Application.DTOs;

/// <summary>
/// Summary DTO for hospital list views with essential information.
/// </summary>
public class HospitalSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsInTrial { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public Guid? SubscriptionPlanId { get; set; }
    public decimal? MonthlySubscriptionCost { get; set; }
    public DateTime CreatedAt { get; set; }
}
