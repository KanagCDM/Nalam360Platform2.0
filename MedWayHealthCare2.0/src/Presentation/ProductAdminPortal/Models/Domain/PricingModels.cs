using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models.Domain;

/// <summary>
/// Represents a pricing rule
/// </summary>
[Table("pricing_rules")]
public class PricingRule
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Column("subscription_plan_id")]
    public Guid? SubscriptionPlanId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("rule_type")]
    public string RuleType { get; set; } = "per_unit"; // flat, per_unit, tiered, multiplier, percentage, bundle

    [Column("configuration", TypeName = "jsonb")]
    public string? Configuration { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(SubscriptionPlanId))]
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    
    public virtual ICollection<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();
    public virtual ICollection<ComplexityMultiplier> ComplexityMultipliers { get; set; } = new List<ComplexityMultiplier>();
}

/// <summary>
/// Represents a pricing tier for tiered pricing rules
/// </summary>
[Table("pricing_tiers")]
public class PricingTier
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("pricing_rule_id")]
    public Guid PricingRuleId { get; set; }

    [Required]
    [Column("min_units")]
    public int MinUnits { get; set; }

    [Column("max_units")]
    public int? MaxUnits { get; set; }

    [Required]
    [Column("unit_price", TypeName = "decimal(15,4)")]
    public decimal UnitPrice { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(PricingRuleId))]
    public virtual PricingRule PricingRule { get; set; } = null!;
}

/// <summary>
/// Represents complexity multipliers for pricing
/// </summary>
[Table("complexity_multipliers")]
public class ComplexityMultiplier
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("pricing_rule_id")]
    public Guid PricingRuleId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("complexity_level")]
    public string ComplexityLevel { get; set; } = string.Empty; // low, medium, high, critical

    [Required]
    [Column("multiplier", TypeName = "decimal(10,2)")]
    public decimal Multiplier { get; set; } = 1.0m;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(PricingRuleId))]
    public virtual PricingRule PricingRule { get; set; } = null!;
}

/// <summary>
/// Represents a discount code
/// </summary>
[Table("discount_codes")]
public class DiscountCode
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("discount_type")]
    public string DiscountType { get; set; } = "percentage"; // percentage, fixed

    [Required]
    [Column("discount_value", TypeName = "decimal(15,2)")]
    public decimal DiscountValue { get; set; }

    [Column("valid_from")]
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    [Column("valid_until")]
    public DateTime? ValidUntil { get; set; }

    [Column("usage_limit")]
    public int? UsageLimit { get; set; }

    [Column("usage_count")]
    public int UsageCount { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;
}
