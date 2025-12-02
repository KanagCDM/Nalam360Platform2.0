using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductAdminPortal.Models;

/// <summary>
/// Junction table for User-Role many-to-many relationship
/// </summary>
[Table("user_roles")]
public class UserRole
{
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("role_id")]
    public Guid RoleId { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
}
