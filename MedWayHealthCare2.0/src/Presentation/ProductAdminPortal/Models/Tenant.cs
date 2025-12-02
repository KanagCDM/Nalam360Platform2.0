using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProductAdminPortal.Models.Domain;

namespace ProductAdminPortal.Models;

/// <summary>
/// Represents a tenant in the multi-tenant system
/// </summary>
[Table("tenants")]
public class Tenant
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("subdomain")]
    public string Subdomain { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active"; // active, suspended, archived

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
