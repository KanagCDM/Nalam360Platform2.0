using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models.Domain;

/// <summary>
/// Represents an invoice
/// </summary>
[Table("invoices")]
public class Invoice
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
    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("invoice_number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [Column("billing_period_start")]
    public DateTime BillingPeriodStart { get; set; }

    [Required]
    [Column("billing_period_end")]
    public DateTime BillingPeriodEnd { get; set; }

    [Required]
    [Column("subtotal", TypeName = "decimal(15,2)")]
    public decimal Subtotal { get; set; }

    [Column("tax_amount", TypeName = "decimal(15,2)")]
    public decimal TaxAmount { get; set; } = 0m;

    [Column("discount_amount", TypeName = "decimal(15,2)")]
    public decimal DiscountAmount { get; set; } = 0m;

    [Required]
    [Column("total_amount", TypeName = "decimal(15,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "draft"; // draft, sent, paid, overdue, canceled

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("paid_at")]
    public DateTime? PaidAt { get; set; }

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
    
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; } = null!;
    
    public virtual ICollection<InvoiceLineItem> InvoiceLineItems { get; set; } = new List<InvoiceLineItem>();
}

/// <summary>
/// Represents a line item in an invoice
/// </summary>
[Table("invoice_line_items")]
public class InvoiceLineItem
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("invoice_id")]
    public Guid InvoiceId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }

    [Required]
    [Column("unit_price", TypeName = "decimal(15,4)")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column("amount", TypeName = "decimal(15,2)")]
    public decimal Amount { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(InvoiceId))]
    public virtual Invoice Invoice { get; set; } = null!;
}

/// <summary>
/// Represents an audit log entry
/// </summary>
[Table("audit_logs")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("action")]
    public string Action { get; set; } = string.Empty; // create, update, delete, view

    [Required]
    [MaxLength(100)]
    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    public Guid? EntityId { get; set; }

    [Column("changes", TypeName = "jsonb")]
    public string? Changes { get; set; }

    [MaxLength(45)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

/// <summary>
/// Represents a configuration version for rollback capability
/// </summary>
[Table("configuration_versions")]
public class ConfigurationVersion
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Required]
    [Column("version_number")]
    public int VersionNumber { get; set; }

    [Column("configuration_snapshot", TypeName = "jsonb")]
    public string? ConfigurationSnapshot { get; set; }

    [Column("change_summary")]
    public string? ChangeSummary { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(CreatedBy))]
    public virtual User? CreatedByUser { get; set; }
}

/// <summary>
/// Represents a usage alert for monitoring thresholds
/// </summary>
[Table("usage_alerts")]
public class UsageAlert
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
    [MaxLength(20)]
    [Column("alert_type")]
    public string AlertType { get; set; } = string.Empty; // soft_limit, hard_limit

    [Required]
    [Column("threshold")]
    public int Threshold { get; set; }

    [Required]
    [Column("current_usage")]
    public int CurrentUsage { get; set; }

    [Column("is_resolved")]
    public bool IsResolved { get; set; } = false;

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

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
