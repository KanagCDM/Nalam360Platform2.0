using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("tenant_id")]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("first_name")]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    [Column("last_name")]
    public string? LastName { get; set; }

    [MaxLength(20)]
    [Phone]
    [Column("phone")]
    public string? Phone { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
