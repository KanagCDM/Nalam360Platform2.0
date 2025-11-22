namespace MedWay.HospitalOnboarding.Application.DTOs;

/// <summary>
/// Payment DTO for payment information.
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid HospitalId { get; set; }
    public Guid? SubscriptionPlanId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
