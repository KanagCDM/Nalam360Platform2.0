namespace SimpleMedWayDemo.Models;

/// <summary>
/// Represents a hospital's active subscription to a plan
/// </summary>
public class HospitalSubscription
{
    public int Id { get; set; }
    public Guid HospitalId { get; set; }
    public int SubscriptionPlanId { get; set; }
    
    // Subscription Period
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoRenew { get; set; } = true;
    
    // Billing
    public bool IsYearlyBilling { get; set; } = false;
    public DateTime? LastBillingDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    // Usage Tracking
    public int CurrentUserCount { get; set; } = 0;
    public int CurrentPatientCount { get; set; } = 0;
    public decimal CurrentStorageUsedGB { get; set; } = 0;
    
    // Status
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Trial Information
    public bool IsTrial { get; set; } = false;
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    
    // Navigation properties
    public virtual Hospital? Hospital { get; set; }
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public virtual ICollection<SubscriptionUpgradeHistory> UpgradeHistory { get; set; } = new List<SubscriptionUpgradeHistory>();
}

/// <summary>
/// Subscription status enumeration
/// </summary>
public enum SubscriptionStatus
{
    Active,         // Currently active
    Expired,        // Past end date
    Cancelled,      // Cancelled by user
    Suspended,      // Suspended due to payment failure or violation
    PendingPayment, // Waiting for payment confirmation
    TrialEnded      // Trial period ended
}

/// <summary>
/// Tracks subscription upgrade/downgrade history
/// </summary>
public class SubscriptionUpgradeHistory
{
    public int Id { get; set; }
    public int HospitalSubscriptionId { get; set; }
    public int FromPlanId { get; set; }
    public int ToPlanId { get; set; }
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public decimal ProrationAmount { get; set; } // Credit/charge for upgrade/downgrade
    public string ChangedBy { get; set; } = string.Empty; // User who made the change
    
    // Navigation properties
    public virtual HospitalSubscription? HospitalSubscription { get; set; }
    public virtual SubscriptionPlan? FromPlan { get; set; }
    public virtual SubscriptionPlan? ToPlan { get; set; }
}
