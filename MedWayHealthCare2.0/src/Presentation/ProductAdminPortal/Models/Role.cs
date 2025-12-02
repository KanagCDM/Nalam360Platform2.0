using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models;

/// <summary>
/// Represents a role in the RBAC system
/// </summary>
[Table("roles")]
public class Role
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("permissions", TypeName = "jsonb")]
    public string? Permissions { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
