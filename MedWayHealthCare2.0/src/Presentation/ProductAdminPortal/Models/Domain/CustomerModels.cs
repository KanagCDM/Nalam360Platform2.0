using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models.Domain;

/// <summary>
/// Represents a customer
/// </summary>
[Table("customers")]
public class Customer
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [Column("company_name")]
    public string? CompanyName { get; set; }

    [Column("billing_address", TypeName = "jsonb")]
    public string? BillingAddress { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active"; // active, inactive, suspended

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual ICollection<CustomerSubscription> CustomerSubscriptions { get; set; } = new List<CustomerSubscription>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

/// <summary>
/// Represents a customer's subscription
/// </summary>
[Table("customer_subscriptions")]
public class CustomerSubscription
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [Required]
    [Column("subscription_plan_id")]
    public Guid SubscriptionPlanId { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active"; // active, inactive, canceled, suspended

    [Column("auto_renew")]
    public bool AutoRenew { get; set; } = true;

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey(nameof(SubscriptionPlanId))]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public virtual ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}

/// <summary>
/// Represents a usage record for tracking entity usage
/// </summary>
[Table("usage_records")]
public class UsageRecord
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [Column("customer_subscription_id")]
    public Guid CustomerSubscriptionId { get; set; }

    [Required]
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Required]
    [Column("units")]
    public int Units { get; set; }

    [MaxLength(20)]
    [Column("complexity")]
    public string? Complexity { get; set; } // low, medium, high, critical

    [Required]
    [MaxLength(20)]
    [Column("billing_status")]
    public string BillingStatus { get; set; } = "unbilled"; // unbilled, billed, invoiced

    [Required]
    [Column("recorded_at")]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(CustomerSubscriptionId))]
    public virtual CustomerSubscription CustomerSubscription { get; set; } = null!;

    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;
}
