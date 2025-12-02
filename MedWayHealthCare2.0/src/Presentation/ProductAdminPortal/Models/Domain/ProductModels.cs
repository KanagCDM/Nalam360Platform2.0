using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models.Domain;

/// <summary>
/// Represents a product (domain-agnostic)
/// </summary>
[Table("products")]
public class Product
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

    [MaxLength(100)]
    [Column("industry")]
    public string? Industry { get; set; }

    [MaxLength(20)]
    [Column("version")]
    public string Version { get; set; } = "1.0.0";

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
    
    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    public virtual ICollection<SubscriptionProduct> SubscriptionProducts { get; set; } = new List<SubscriptionProduct>();
}

/// <summary>
/// Represents a module within a product
/// </summary>
[Table("modules")]
public class Module
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

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

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    [Column("is_required")]
    public bool IsRequired { get; set; } = false;

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
    
    public virtual ICollection<ModuleEntity> ModuleEntities { get; set; } = new List<ModuleEntity>();
    public virtual ICollection<SubscriptionModule> SubscriptionModules { get; set; } = new List<SubscriptionModule>();
}

/// <summary>
/// Represents an entity (billable/trackable feature)
/// </summary>
[Table("entities")]
public class Entity
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

    [Required]
    [MaxLength(50)]
    [Column("type")]
    public string Type { get; set; } = "transaction"; // transaction, seat, storage, api_request

    [MaxLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("base_pricing", TypeName = "jsonb")]
    public string? BasePricing { get; set; }

    [Column("complexity_levels", TypeName = "jsonb")]
    public string? ComplexityLevels { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual ICollection<ModuleEntity> ModuleEntities { get; set; } = new List<ModuleEntity>();
    public virtual ICollection<SubscriptionEntity> SubscriptionEntities { get; set; } = new List<SubscriptionEntity>();
    public virtual ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}

/// <summary>
/// Junction table for Module-Entity many-to-many relationship
/// </summary>
[Table("module_entities")]
public class ModuleEntity
{
    [Required]
    [Column("module_id")]
    public Guid ModuleId { get; set; }

    [Required]
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    [Column("is_required")]
    public bool IsRequired { get; set; } = false;

    // Navigation properties
    [ForeignKey(nameof(ModuleId))]
    public virtual Module Module { get; set; } = null!;

    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;
}
