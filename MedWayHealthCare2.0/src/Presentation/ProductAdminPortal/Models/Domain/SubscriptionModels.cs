using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models.Domain;

/// <summary>
/// Represents a subscription plan
/// </summary>
[Table("subscription_plans")]
public class SubscriptionPlan
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
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("price", TypeName = "decimal(15,2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("billing_cycle")]
    public string BillingCycle { get; set; } = "monthly"; // monthly, quarterly, yearly

    [Column("trial_days")]
    public int TrialDays { get; set; } = 0;

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
    
    public virtual ICollection<SubscriptionProduct> SubscriptionProducts { get; set; } = new List<SubscriptionProduct>();
    public virtual ICollection<SubscriptionModule> SubscriptionModules { get; set; } = new List<SubscriptionModule>();
    public virtual ICollection<SubscriptionEntity> SubscriptionEntities { get; set; } = new List<SubscriptionEntity>();
    public virtual ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
    public virtual ICollection<CustomerSubscription> CustomerSubscriptions { get; set; } = new List<CustomerSubscription>();
}

/// <summary>
/// Junction table for SubscriptionPlan-Product many-to-many relationship
/// </summary>
[Table("subscription_products")]
public class SubscriptionProduct
{
    [Required]
    [Column("subscription_plan_id")]
    public Guid SubscriptionPlanId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SubscriptionPlanId))]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}

/// <summary>
/// Junction table for SubscriptionPlan-Module many-to-many relationship
/// </summary>
[Table("subscription_modules")]
public class SubscriptionModule
{
    [Required]
    [Column("subscription_plan_id")]
    public Guid SubscriptionPlanId { get; set; }

    [Required]
    [Column("module_id")]
    public Guid ModuleId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SubscriptionPlanId))]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    [ForeignKey(nameof(ModuleId))]
    public virtual Module Module { get; set; } = null!;
}

/// <summary>
/// Maps subscription plans to entities with usage limits
/// </summary>
[Table("subscription_entities")]
public class SubscriptionEntity
{
    [Required]
    [Column("subscription_plan_id")]
    public Guid SubscriptionPlanId { get; set; }

    [Required]
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("usage_limit")]
    public int? UsageLimit { get; set; }

    [Column("soft_limit_threshold")]
    public int? SoftLimitThreshold { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SubscriptionPlanId))]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;
}
